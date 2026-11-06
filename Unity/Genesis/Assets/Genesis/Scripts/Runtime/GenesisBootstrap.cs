using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Genesis
{
    /// <summary>
    /// The one owner of the player start up.
    ///
    /// Everything that happens between the process starting and the Sanctum
    /// being interactive lives in this class: the platform settings, the
    /// working directory, the window, the single instance hand over, the asset
    /// warm up, the scene load and the failure handling.
    ///
    /// The parts used to be spread over the boot scene, a separate bootstrap
    /// component and the Sanctum itself. That split is what made the launch
    /// unpredictable: a stage could fail without anybody closing the window, a
    /// second copy could be blocked by a check that never looked at a running
    /// game, and the window could stay on screen, hidden and frozen, with every
    /// button dead.
    ///
    /// The rules this class follows:
    ///  - The window is always visible and the player never sleeps.
    ///  - A second copy hands the window over instead of showing a message.
    ///  - Every stage has a deadline. A stage that runs out of time is a
    ///    failure with an explanation, never an endless wait.
    ///  - The game either starts or says why it did not. It never hangs.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class GenesisBootstrap : MonoBehaviour
    {
        public const string BootSceneName = "GenesisBoot";
        public const string GameSceneName = "ZendaSanctum";
        public const string ManifestResourcePath = "Genesis/Data/boot_manifest";
        public const string CharacterModelResourcePath = "Genesis/Characters/ZendaGenesis";
        public const string LogoResourcePath = "Genesis/Art/Branding/ZendaGenesisLogo";
        public const string ShaderVariantResourcePath = "Genesis/Shaders/GenesisCriticalVariants";

        /// <summary>Seconds the failure message stays on screen before the player closes.</summary>
        private const float FailureGraceSeconds = 8f;

        /// <summary>Seconds the asset warm up may take before it counts as a failure.</summary>
        private const float PreloadDeadlineSeconds = 240f;

        /// <summary>Seconds the game scene may take to load before it counts as a failure.</summary>
        private const float SceneDeadlineSeconds = 120f;

        /// <summary>
        /// Seconds the boot waits for the shader variants to finish compiling
        /// before it opens the Sanctum anyway.
        /// </summary>
        private const float ShaderWaitSeconds = 20f;

        /// <summary>
        /// Seconds without any movement in the scene load before the loading
        /// screen says that it is still working instead of looking frozen.
        /// </summary>
        private const float StallReportSeconds = 8f;

        /// <summary>
        /// Shader variants the warm up compiles in one frame. The number
        /// moves with how long the last frame took, so a slow device warms
        /// one variant at a time and a fast one warms a few.
        /// </summary>
        private const int ShaderVariantsPerFrame = 4;

        /// <summary>
        /// Seconds the warm up of one frame may take before the number of
        /// variants per frame is cut back.
        /// </summary>
        private const float ShaderFrameBudgetSeconds = 0.02f;

        /// <summary>Frames the window is raised on, in case the focus is still locked.</summary>
        private const int WindowRaiseFrames = 120;

        /// <summary>Frames between two background attempts to raise the window.</summary>
        private const int WindowRaiseRetryFrames = 30;

        /// <summary>Milliseconds of every frame the asset warm up may use.</summary>
        private const float FrameBudgetMilliseconds = 6f;

        /// <summary>Seconds the loading screen takes to fade away.</summary>
        private const float FadeSeconds = 0.35f;

        /// <summary>Used when the generated manifest is missing from the project.</summary>
        private static readonly string[] FallbackCritical =
        {
            "Genesis/Data/catalog",
            "Genesis/Data/localization",
            "Genesis/Data/story-localization",
            "Genesis/Data/fairy-halos",
            "Genesis/Fonts/Almendra-Bold",
            "Genesis/Fonts/CrimsonText-Regular",
            "Genesis/Fonts/NotoSans-Regular",
            "Genesis/Art/Branding/ZendaGenesisLogo",
            "Genesis/Art/backgrounds/environments/world_00",
            "Genesis/UI/EnchantedGameTheme",
            "Genesis/Characters/ZendaGenesis"
        };

        private static readonly string[] Tips =
        {
            "Zenda is the goddess of possibility. Every universe begins with a single tap.",
            "Drag Zenda left and right to turn her. She answers every gesture.",
            "Each discovery lifts the whole universe: hope starts growing on its own.",
            "Stories are the spine of the journey. Remember a chapter to unlock the next.",
            "Big Bloom restarts the run but keeps every memory you earned."
        };

        private enum Phase
        {
            Starting,
            HandingOver,
            RaisingWindow,
            Intro,
            Preloading,
            ModelWarmup,
            ShaderWarmup,
            LoadingScene,
            FadeOut,
            Running,
            Failed
        }

        private static GenesisBootstrap instance;
        private static bool runtimeConfigured;

        private readonly StringBuilder failures = new StringBuilder();
        private readonly List<GenesisBootEntry> entries = new List<GenesisBootEntry>();

        private Phase phase = Phase.Starting;
        private float phaseTimer;
        private float failureTimer;
        private int entryIndex;
        private long criticalBytes;
        private long loadedBytes;
        private float shownProgress;
        private float targetProgress;
        private float fade = 1f;
        private int introFrames;
        private int warmupFrames;
        private int windowAttempts;
        private int windowRetries;
        private bool mobileWindowApplied;
        private string status = "Waking the void";
        private int tipIndex;
        private float tipTimer;
        private AsyncOperation pendingScene;
        private ShaderVariantCollection criticalVariants;
        private GameObject warmupModel;
        private Texture2D logo;
        private bool drivesBoot;

        /// <summary>Real time the current phase started, used for the deadlines.</summary>
        private float phaseStartedAt;

        /// <summary>Real time the scene load started, used for its own deadline.</summary>
        private float sceneStartedAt;

        /// <summary>Real time the shader wait started.</summary>
        private float shaderWaitStartedAt;

        /// <summary>The progress the scene load reported the last time it moved.</summary>
        private float sceneProgress;

        /// <summary>Real time the scene load last reported a different progress.</summary>
        private float sceneProgressAt;

        /// <summary>Shader variants the warm up compiles in one frame.</summary>
        private int shaderVariantsPerFrame = ShaderVariantsPerFrame;

        private GUIStyle title, subtitle, body, tip;
        private GUIStyle failureTitle, failureBody;
        private Texture2D failureBackdrop;

        /// <summary>True when the player could not start and is about to close.</summary>
        public static bool Failed { get; private set; }

        /// <summary>
        /// True when another copy of the game owns the window and this copy only
        /// hands it over. The copy that is already running stays in charge.
        /// </summary>
        public static bool HandedOver { get; private set; }

        /// <summary>Creates the bootstrap before any scene is loaded.</summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (instance != null) return;
            ConfigureRuntime();
            // A second copy of the game must never block the launch. It hands
            // the window over to the copy that is already running and closes
            // itself. The check fails open, so a stale detection can never stop
            // the game from opening.
            if (!GenesisWindowsSupport.TryClaimInstance() && GenesisWindowsSupport.TryFocusRunningInstance())
            {
                HandedOver = true;
                Debug.Log("[Genesis] another copy is running. Handing the window over to it.");
            }
            // The game is a mobile game on every platform, so the desktop
            // window is shaped like a phone before the first frame is drawn.
            // The mobile view then fills the window from the very first
            // moment instead of appearing as a column inside a wide screen.
            // A copy that only hands the window over leaves the window of the
            // running game alone.
            if (!HandedOver) GenesisWindowsSupport.ApplyMobileWindow();
            var host = new GameObject("Genesis bootstrap");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<GenesisBootstrap>();
        }

        /// <summary>
        /// Applies the platform settings that must hold before the first frame.
        /// These are also written into the build settings, but the runtime
        /// values win and protect a player built with older settings.
        /// </summary>
        private static void ConfigureRuntime()
        {
            if (runtimeConfigured) return;
            runtimeConfigured = true;
            // A launch through a link or a shortcut leaves the working
            // directory wherever the launcher happened to be. The game moves
            // it to the installation folder before anything else runs, so no
            // path the game resolves can land in the wrong place.
            GenesisWindowsSupport.NormalizeWorkingDirectory();
            // Windows suspends a player that loses focus unless it may keep
            // running. A suspended player is the frozen window in the
            // background that never recovers.
            Application.runInBackground = true;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
#if UNITY_ANDROID && !UNITY_EDITOR
            // The game is a portrait phone game. The device is locked to that
            // shape even when the player rotates it, because the whole layout
            // is a single column that was drawn for a phone.
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = true;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.orientation = ScreenOrientation.Portrait;
#endif
#if UNITY_ANDROID || UNITY_IOS || UNITY_WEBGL
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
#else
            Application.targetFrameRate = -1;
            QualitySettings.vSyncCount = 1;
#endif
            Debug.Log("[Genesis] player bootstrap. unity=" + Application.unityVersion
                + " platform=" + Application.platform
                + " graphics=" + SystemInfo.graphicsDeviceType
                + " data=" + Application.dataPath
                + " cwd=" + System.IO.Directory.GetCurrentDirectory());
        }

        private void Awake()
        {
            Failed = false;
            failures.Clear();
            failureTimer = 0f;
            phaseStartedAt = Time.realtimeSinceStartup;
            sceneStartedAt = phaseStartedAt;
            shaderWaitStartedAt = phaseStartedAt;
            sceneProgressAt = phaseStartedAt;
            // In the editor the boot sequence only runs when the boot scene is
            // the scene that is open, so the preview scenes keep working.
            drivesBoot = !Application.isEditor || SceneManager.GetActiveScene().name == BootSceneName;
            // A copy that only hands the window over never starts the boot.
            if (HandedOver) phase = Phase.HandingOver;
        }

        /// <summary>
        /// Moves to the next phase and restarts its clocks.
        ///
        /// Every deadline is measured against the real time the phase
        /// started. The frame delta was the old measure, and Unity clamps the
        /// frame delta to a third of a second, so a boot that was stalling
        /// counted three times slower than the clock on the wall. A boot that
        /// was going to fail after two minutes was reported as a failure
        /// after six, which is a long time to sit in front of a frozen
        /// window.
        /// </summary>
        private void SetPhase(Phase next)
        {
            phase = next;
            phaseTimer = 0f;
            phaseStartedAt = Time.realtimeSinceStartup;
        }

        private void Update()
        {
            phaseTimer += Time.unscaledDeltaTime;
            // While the loading screen runs the window is raised again every
            // now and then. Windows holds the focus for a moment after a link
            // launches a program, and the game becomes visible as soon as that
            // lock is gone.
            if (phase == Phase.Intro || phase == Phase.Preloading || phase == Phase.ModelWarmup)
            {
                if (++windowRetries % WindowRaiseRetryFrames == 0)
                {
                    GenesisWindowsSupport.EnsureWindowVisible();
                    // Unity puts the window of the last session back while
                    // the game loads, which moves the game to another screen
                    // and leaves the player with nothing to look at. The
                    // window is put back on the screen the game was launched
                    // from while the game loads.
                    GenesisWindowsSupport.KeepMobileWindow();
                }
            }
            switch (phase)
            {
                case Phase.Starting: RunStarting(); break;
                case Phase.HandingOver: RunHandingOver(); break;
                case Phase.RaisingWindow: RunRaisingWindow(); break;
                case Phase.Intro: RunIntro(); break;
                case Phase.Preloading: RunPreload(); break;
                case Phase.ModelWarmup: RunModelWarmup(); break;
                case Phase.ShaderWarmup: RunShaderWarmup(); break;
                case Phase.LoadingScene: RunSceneLoad(); break;
                case Phase.FadeOut: RunFadeOut(); break;
                case Phase.Failed: RunFailure(); break;
            }
            // The progress bar and the tip keep moving in every phase so the
            // loading screen never looks frozen while the player waits.
            shownProgress = Mathf.Lerp(shownProgress, targetProgress, 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
            // The bar never sits still for long. A slow machine that needs
            // more time for one step still sees the bar move, so the screen
            // never looks like it has hung.
            if (targetProgress > shownProgress) shownProgress = Mathf.Min(targetProgress, shownProgress + 0.004f);
            tipTimer += Time.unscaledDeltaTime;
            if (tipTimer >= 6f) { tipTimer = 0f; tipIndex = (tipIndex + 1) % Tips.Length; }
        }

        /// <summary>
        /// Prepares the process before anything heavy runs. The window is
        /// raised, the build is checked and the assets are queued.
        /// </summary>
        private void RunStarting()
        {
            if (!drivesBoot)
            {
                // The editor is running a scene that is not the boot scene.
                // The player settings are applied and that is all that is needed.
                SetPhase(Phase.Running);
                return;
            }
            if (!Application.CanStreamedLevelBeLoaded(GameSceneName))
            {
                Fail("The build does not contain the " + GameSceneName + " scene.");
                return;
            }
            StartPreload();
        }

        /// <summary>
        /// Brings the player window to the front before anything else happens.
        ///
        /// The window is created a frame or two after the process starts, and
        /// Windows refuses the focus for a short moment when the game is
        /// launched from a desktop link. A single attempt was therefore not
        /// enough: the game opened behind the window the player was looking
        /// at and looked like it had never started. The raise is retried until
        /// it works or the grace period runs out.
        /// </summary>
        private void RunRaisingWindow()
        {
            // The window opens with the shape of the phone. It is applied
            // again while the window handle is not found yet, which happens
            // when the game is started from a link and Windows creates the
            // window a frame later.
            if (!mobileWindowApplied) mobileWindowApplied = GenesisWindowsSupport.ApplyMobileWindow();
            // The window has to be on the screen the player is looking at and
            // it must not be minimized: a minimized window shows nothing on
            // the desktop and only a small entry in the task bar.
            GenesisWindowsSupport.KeepMobileWindow();
            bool raised = GenesisWindowsSupport.EnsureWindowVisible();
            windowAttempts++;
            if (!raised && windowAttempts < WindowRaiseFrames) return;
            if (!raised)
                Debug.LogWarning("[Genesis] the game window stayed in the background. The game is running, bring it forward with Alt+Tab.");
            SetPhase(Phase.Intro);
        }

        /// <summary>Closes this copy after the running one was brought forward.</summary>
        private void RunHandingOver()
        {
            // One frame is enough for the message to be seen.
            if (phaseTimer < 0.4f) return;
            Debug.Log("[Genesis] closing. The copy that is already running owns the window.");
            Application.Quit();
        }

        private void StartPreload()
        {
            entries.AddRange(GenesisBootManifest.Load());
            if (entries.Count == 0)
            {
                for (int i = 0; i < FallbackCritical.Length; i++)
                    entries.Add(new GenesisBootEntry { Path = FallbackCritical[i], Tier = 0, Bytes = 0 });
            }
            for (int i = 0; i < entries.Count; i++)
                if (entries[i].Tier == 0) criticalBytes += entries[i].Bytes;
            // The shader variants the game uses are collected at build time
            // and registered in the graphics settings, so Unity is already
            // warming them up while the assets load. The boot only reads
            // their state later on.
            criticalVariants = GenesisPreloadCache.Load<ShaderVariantCollection>(ShaderVariantResourcePath);
            GenesisBackgroundPreloader.Create(BootSceneName);
            introFrames = 0;
            windowAttempts = 0;
            SetPhase(Phase.RaisingWindow);
            status = "Loading the Sanctum";
        }

        /// <summary>
        /// Two rendered frames guarantee the loading screen is on the display
        /// before the first slow operation starts.
        /// </summary>
        private void RunIntro()
        {
            if (++introFrames < 2) return;
            SetPhase(Phase.Preloading);
        }

        /// <summary>
        /// Loads the critical assets a few per frame so the main thread never
        /// blocks long enough to freeze the loading screen.
        /// </summary>
        private void RunPreload()
        {
            if (Time.realtimeSinceStartup - phaseStartedAt > PreloadDeadlineSeconds)
            {
                Fail("The assets could not be prepared within " + (int)PreloadDeadlineSeconds + " seconds.");
                return;
            }
            float budget = Mathf.Max(1f, FrameBudgetMilliseconds) * 0.001f;
            float deadline = Time.realtimeSinceStartup + budget;
            while (entryIndex < entries.Count)
            {
                GenesisBootEntry entry = entries[entryIndex];
                entryIndex++;
                if (entry.Tier != 0) continue;
                LoadEntry(entry);
                if (Time.realtimeSinceStartup >= deadline) break;
            }
            targetProgress = criticalBytes > 0
                ? 0.7f * Mathf.Clamp01((float)loadedBytes / criticalBytes)
                : 0.7f * Mathf.Clamp01((float)entryIndex / Mathf.Max(1, entries.Count));
            status = "Loading assets " + Mathf.RoundToInt(targetProgress / 0.7f * 100f) + "%";
            if (entryIndex < entries.Count) return;
            logo = GenesisPreloadCache.Load<Texture2D>(LogoResourcePath);
            warmupFrames = 0;
            SetPhase(Phase.ModelWarmup);
            status = "Waking Zenda";
        }

        private void LoadEntry(GenesisBootEntry entry)
        {
            UnityEngine.Object asset = Resources.Load(entry.Path);
            if (asset == null) return;
            GenesisPreloadCache.Hold(entry.Path, asset);
            loadedBytes += entry.Bytes;
        }

        /// <summary>
        /// Draws the character model once behind the loading screen. Rendering
        /// is what compiles the MToon shader variants and uploads the model
        /// textures to the GPU; without it the Sanctum scene stalls for seconds
        /// the first time the goddess is drawn.
        /// </summary>
        private void RunModelWarmup()
        {
            warmupFrames++;
            if (warmupFrames == 1) CreateWarmupModel();
            if (warmupFrames < 4) return;
            DestroyWarmupModel();
            targetProgress = 0.78f;
            // The wait for the shader variants starts now. Unity has been
            // warming them since the first frame, so most of the time they
            // are already done when the boot reaches this point.
            shaderWaitStartedAt = Time.realtimeSinceStartup;
            SetPhase(Phase.ShaderWarmup);
            status = "Compiling shaders";
        }

        private void CreateWarmupModel()
        {
            try
            {
                GameObject prefab = GenesisPreloadCache.Load<GameObject>(CharacterModelResourcePath);
                if (prefab == null) return;
                warmupModel = Instantiate(prefab);
                warmupModel.name = "Zenda boot warm-up";
                // Sit in front of the boot camera so the renderer really draws.
                warmupModel.transform.position = new Vector3(0f, 0f, 0f);
            }
            catch (Exception error)
            {
                Debug.LogWarning("Genesis boot warm-up skipped: " + error.Message);
                warmupModel = null;
            }
        }

        private void DestroyWarmupModel()
        {
            if (warmupModel == null) return;
            Destroy(warmupModel);
            warmupModel = null;
        }

        /// <summary>
        /// Starts the scene load and lets the shaders compile in the background.
        ///
        /// This phase used to call Shader.WarmupAllShaders, which compiles
        /// every shader variant that survived the build stripping in one
        /// blocking call on the main thread. On the desktop player that takes
        /// a fraction of a second, but the browser and the phone compile the
        /// very same variants on the device, so the call froze the loading
        /// screen for minutes. The bar sat at eighty percent with nothing
        /// moving, the scene load never got its turn, and the boot ended with
        /// the scene timeout and a closed window.
        ///
        /// The variants the game actually uses are collected into a shader
        /// variant collection at build time and registered in the graphics
        /// settings. Unity warms that collection a few milliseconds per
        /// frame, so the loading screen keeps moving and the shaders are
        /// already compiled when the Sanctum opens. Nothing here blocks the
        /// main thread, and the scene load runs while the variants warm up.
        /// </summary>
        private void RunShaderWarmup()
        {
            if (phaseTimer < 0.25f) return;
            // The scene streams in while the variants warm up. The two used
            // to be one long blocking step, and running them together is
            // what takes the boot from minutes to seconds on the phone and
            // in the browser.
            StartSceneLoad();
            if (Failed) return;
            if (criticalVariants == null)
            {
                SetPhase(Phase.LoadingScene);
                status = "Opening the Sanctum";
                return;
            }
            // The variants are compiled a few at a time. The call gives back
            // as soon as that many are ready, so the loading screen keeps
            // moving and no frame ever waits for the whole collection.
            float started = Time.realtimeSinceStartup;
            bool finished = criticalVariants.WarmUpProgressively(shaderVariantsPerFrame);
            float cost = Time.realtimeSinceStartup - started;
            // Keep a frame short enough for the loading screen to stay alive.
            if (cost > ShaderFrameBudgetSeconds) shaderVariantsPerFrame = Mathf.Max(1, shaderVariantsPerFrame / 2);
            else if (cost < ShaderFrameBudgetSeconds * 0.25f) shaderVariantsPerFrame = Mathf.Min(32, shaderVariantsPerFrame + 1);
            float total = Mathf.Max(1, criticalVariants.variantCount);
            float warmed = criticalVariants.warmedUpVariantCount;
            targetProgress = 0.78f + 0.1f * Mathf.Clamp01(warmed / total);
            status = "Compiling shaders " + Mathf.RoundToInt(warmed / total * 100f) + "%";
            // A device that cannot compile the variants must still reach the
            // game instead of sitting in the loading screen.
            if (!finished && Time.realtimeSinceStartup - phaseStartedAt < ShaderWaitSeconds) return;
            SetPhase(Phase.LoadingScene);
            status = "Opening the Sanctum";
        }

        /// <summary>
        /// Starts the scene load once and keeps the handle for the rest of
        /// the boot.
        /// </summary>
        private void StartSceneLoad()
        {
            if (pendingScene != null) return;
            try
            {
                pendingScene = SceneManager.LoadSceneAsync(GameSceneName, LoadSceneMode.Single);
                pendingScene.allowSceneActivation = false;
                sceneStartedAt = Time.realtimeSinceStartup;
                sceneProgressAt = sceneStartedAt;
                sceneProgress = 0f;
            }
            catch (Exception error)
            {
                pendingScene = null;
                Fail("The " + GameSceneName + " scene could not be started: " + error.Message);
            }
        }

        /// <summary>
        /// Streams the game scene in while the loading screen keeps rendering.
        /// The scene is only activated once it is fully loaded, so the Sanctum
        /// never starts with assets that are still missing.
        /// </summary>
        private void RunSceneLoad()
        {
            if (pendingScene == null) StartSceneLoad();
            if (pendingScene == null) return;
            if (Time.realtimeSinceStartup - sceneStartedAt > SceneDeadlineSeconds)
            {
                Fail("The " + GameSceneName + " scene could not be loaded within "
                    + (int)SceneDeadlineSeconds + " seconds.");
                return;
            }
            // Remember when the load last moved so a slow load can say so
            // instead of looking like a frozen screen.
            if (pendingScene.progress > sceneProgress + 0.001f)
            {
                sceneProgress = pendingScene.progress;
                sceneProgressAt = Time.realtimeSinceStartup;
            }
            targetProgress = 0.84f + 0.16f * pendingScene.progress;
            bool sceneReady = pendingScene.progress >= 0.9f;
            bool shadersReady = ShadersAreReady();
            if (!sceneReady || !shadersReady)
            {
                if (!sceneReady)
                    status = Time.realtimeSinceStartup - sceneProgressAt > StallReportSeconds
                        ? "Still opening the Sanctum"
                        : "Opening the Sanctum";
                else status = "Compiling shaders";
                return;
            }
            SetPhase(Phase.FadeOut);
            status = "Welcome";
        }

        /// <summary>
        /// True when the shader variants the game needs are compiled.
        ///
        /// The variants are warmed by Unity in the background, a few
        /// milliseconds per frame, so this only reads the state. The wait
        /// has a limit: a device that is too slow to compile them must still
        /// reach the game instead of sitting in the loading screen.
        /// </summary>
        private bool ShadersAreReady()
        {
            if (criticalVariants == null) return true;
            if (criticalVariants.isWarmedUp) return true;
            if (Time.realtimeSinceStartup - shaderWaitStartedAt < ShaderWaitSeconds) return false;
            return true;
        }

        private void RunFadeOut()
        {
            fade = Mathf.MoveTowards(fade, 0f, Time.unscaledDeltaTime / Mathf.Max(0.05f, FadeSeconds));
            if (fade > 0f) return;
            if (pendingScene != null) pendingScene.allowSceneActivation = true;
            SetPhase(Phase.Running);
            Debug.Log("[Genesis] the Sanctum is ready.");
        }

        /// <summary>Shows the reason for the failure and then closes the player.</summary>
        private void RunFailure()
        {
            failureTimer += Time.unscaledDeltaTime;
            if (failureTimer < FailureGraceSeconds) return;
            // Never leave a broken window sitting in the background.
            Debug.LogError("[Genesis] closing after a failed startup.");
            Application.Quit();
        }

        /// <summary>Reports a startup failure from any part of the game.</summary>
        public static void ReportFailure(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return;
            if (instance == null)
            {
                Debug.LogError("[Genesis] startup failure: " + reason);
                return;
            }
            instance.Fail(reason);
        }

        private void Fail(string reason)
        {
            if (Failed) return;
            Failed = true;
            SetPhase(Phase.Failed);
            failureTimer = 0f;
            failures.AppendLine(reason);
            Debug.LogError("[Genesis] startup failure: " + reason);
        }

        private void OnDestroy()
        {
            if (warmupModel != null) Destroy(warmupModel);
        }

        private void OnGUI()
        {
            if (HandedOver)
            {
                DrawHandoff();
                return;
            }
            if (Failed)
            {
                DrawFailure();
                return;
            }
            if (phase == Phase.Running || !drivesBoot) return;
            DrawLoadingScreen();
        }

        /// <summary>The Enchanted loading screen, drawn with the kit artwork.</summary>
        private void DrawLoadingScreen()
        {
            EnsureStyles();
            Color previousColor = GUI.color;
            bool previousEnabled = GUI.enabled;
            GUI.enabled = true;
            GUI.color = new Color(1f, 1f, 1f, fade);

            // The soft amethyst backdrop of the kit.
            GenesisEnchantedArt.Gradient(new Rect(0f, 0f, Screen.width, Screen.height),
                GenesisEnchantedArt.BackgroundTop, GenesisEnchantedArt.BackgroundBottom);
            DrawOrnaments();

            Rect safe = Screen.safeArea;
            // Same canvas rule as the game: a 390 wide column that scales to fit.
            float scale = Mathf.Min(safe.width / 390f, safe.height / 620f);
            float height = safe.height / scale;
            float left = safe.x + (safe.width - 390f * scale) * 0.5f;
            Matrix4x4 previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(new Vector3(left, safe.y, 0f), Quaternion.identity, new Vector3(scale, scale, 1f));
            try
            {
                DrawLoadingPanel(height);
            }
            finally
            {
                GUI.matrix = previousMatrix;
                GUI.color = previousColor;
                GUI.enabled = previousEnabled;
            }
        }

        /// <summary>Floral corners from the kit, faded right back so they stay decoration.</summary>
        private void DrawOrnaments()
        {
            UnityEngine.Sprite blossom = GenesisEnchantedArt.Blossom;
            if (blossom == null) return;
            float size = Mathf.Max(Screen.width, Screen.height) * 0.85f;
            GenesisEnchantedArt.Ornament(new Rect(-size * 0.22f, -size * 0.06f, size, size), blossom, 0.18f);
            GenesisEnchantedArt.Ornament(new Rect(Screen.width - size * 0.78f, -size * 0.06f, size, size),
                blossom, 0.14f, 0f, true);
        }

        private void DrawLoadingPanel(float height)
        {
            const float panelWidth = 366f;
            const float panelHeight = 430f;
            Rect panel = new Rect((390f - panelWidth) * 0.5f, (height - panelHeight) * 0.5f, panelWidth, panelHeight);
            GenesisEnchantedArt.Panel(panel);

            float pad = 24f;
            float contentX = panel.x + pad;
            float contentWidth = panel.width - pad * 2f;
            float centerX = panel.center.x;

            // Crest: the game mark is already an ornate gold and amethyst badge,
            // so it plays the role of the portrait inside the kit panel.
            if (logo != null) GUI.DrawTexture(new Rect(centerX - 60f, panel.y + 18f, 120f, 120f), logo, ScaleMode.ScaleToFit);

            GenesisEnchantedArt.Divider(new Rect(contentX, panel.y + 148f, contentWidth, 14f));
            GUI.Label(new Rect(contentX, panel.y + 164f, contentWidth, 42f), "ZendaGenesis", title);
            GUI.Label(new Rect(contentX, panel.y + 204f, contentWidth, 24f), Spaced("SANCTUM OF CREATION"), subtitle);

            GenesisEnchantedArt.Progress(new Rect(contentX, panel.y + 242f, contentWidth, 16f), shownProgress);
            GUI.Label(new Rect(contentX, panel.y + 264f, contentWidth, 26f),
                "<color=#F0CC91>" + Mathf.RoundToInt(shownProgress * 100f) + "%</color>    " + status, body);

            DrawHairline(new Rect(contentX, panel.y + 300f, contentWidth, 1f));
            GUI.Label(new Rect(contentX, panel.y + 312f, contentWidth, 76f), Tips[tipIndex], tip);

            GUI.Label(new Rect(contentX, height - 40f, contentWidth, 22f), "v" + Application.version, tip);
        }

        private void DrawHairline(Rect rect)
        {
            Color previous = GUI.color;
            GUI.color = new Color(previous.r, previous.g, previous.b, previous.a * 0.35f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = previous;
        }

        /// <summary>Letter spacing for the small caps line under the title.</summary>
        private static string Spaced(string text)
        {
            var builder = new StringBuilder(text.Length * 2);
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0) builder.Append(' ');
                builder.Append(text[i]);
            }
            return builder.ToString();
        }

        /// <summary>
        /// Explains the failure before the player closes. The player sees a
        /// clear reason instead of a window that blinks and disappears.
        /// </summary>
        private void DrawFailure()
        {
            EnsureFailureStyles();
            float width = Screen.width;
            float height = Screen.height;
            Color previous = GUI.color;
            GUI.color = new Color(0.08f, 0.05f, 0.12f, 0.96f);
            GUI.DrawTexture(new Rect(0f, 0f, width, height), failureBackdrop);
            GUI.color = previous;
            float cardWidth = Mathf.Min(width - 48f, 520f);
            float cardHeight = Mathf.Min(height - 80f, 260f);
            Rect card = new Rect((width - cardWidth) * 0.5f, (height - cardHeight) * 0.5f, cardWidth, cardHeight);
            GUI.Label(card, GUIContent.none, failureTitle);
            GUI.Label(new Rect(card.x + 24f, card.y + 24f, card.width - 48f, 40f), "ZendaGenesis could not start", failureTitle);
            GUI.Label(new Rect(card.x + 24f, card.y + 70f, card.width - 48f, card.height - 120f),
                failures.ToString().Trim(), failureBody);
            GUI.Label(new Rect(card.x + 24f, card.yMax - 44f, card.width - 48f, 30f),
                "The window will close automatically. Check the player log for details.", failureBody);
        }

        /// <summary>
        /// Explains the hand over before the copy closes. The player sees a clear
        /// reason instead of a window that blinks and disappears.
        /// </summary>
        private void DrawHandoff()
        {
            EnsureFailureStyles();
            float width = Screen.width;
            float height = Screen.height;
            Color previous = GUI.color;
            GUI.color = new Color(0.08f, 0.05f, 0.12f, 0.96f);
            GUI.DrawTexture(new Rect(0f, 0f, width, height), failureBackdrop);
            GUI.color = previous;
            float cardWidth = Mathf.Min(width - 48f, 520f);
            float cardHeight = Mathf.Min(height - 80f, 200f);
            Rect card = new Rect((width - cardWidth) * 0.5f, (height - cardHeight) * 0.5f, cardWidth, cardHeight);
            GUI.Label(card, GUIContent.none, failureTitle);
            GUI.Label(new Rect(card.x + 24f, card.y + 24f, card.width - 48f, 40f), "ZendaGenesis is already open", failureTitle);
            GUI.Label(new Rect(card.x + 24f, card.y + 70f, card.width - 48f, card.height - 100f),
                "The window of the running game was brought to the front. This copy closes now.", failureBody);
        }

        private void EnsureStyles()
        {
            if (title == null)
            {
                title = new GUIStyle(GUI.skin.label);
                subtitle = new GUIStyle(GUI.skin.label);
                body = new GUIStyle(GUI.skin.label);
                tip = new GUIStyle(GUI.skin.label);
            }
            // Unity can clear the native state of a GUIStyle when the editor
            // reloads, so the theme is reapplied every frame instead of only
            // when the styles are created.
            Font heading = GenesisEnchantedArt.TitleFont;
            Font reading = GenesisEnchantedArt.BodyFont;
            ApplyStyle(title, heading, 30, FontStyle.Bold, false, false, GenesisEnchantedArt.Text);
            ApplyStyle(subtitle, reading, 12, FontStyle.Normal, false, false, GenesisEnchantedArt.MutedText);
            ApplyStyle(body, reading, 14, FontStyle.Normal, false, true, GenesisEnchantedArt.MutedText);
            ApplyStyle(tip, reading, 12, FontStyle.Normal, true, false, GenesisEnchantedArt.MutedText);
        }

        private static void ApplyStyle(GUIStyle style, Font font, int size, FontStyle fontStyle,
            bool wordWrap, bool richText, Color color)
        {
            style.font = font != null ? font : GUI.skin.label.font;
            style.fontSize = size;
            style.fontStyle = fontStyle;
            style.alignment = TextAnchor.MiddleCenter;
            style.wordWrap = wordWrap;
            style.richText = richText;
            style.normal.textColor = color;
        }

        private void EnsureFailureStyles()
        {
            if (failureBackdrop == null)
                failureBackdrop = new Texture2D(1, 1, TextureFormat.RGBA32, false) { name = "Genesis failure backdrop" };
            if (failureTitle != null) return;
            failureTitle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold, wordWrap = true };
            failureTitle.normal.textColor = new Color(0.94f, 0.80f, 0.57f);
            failureBody = new GUIStyle(GUI.skin.label) { fontSize = 14, wordWrap = true };
            failureBody.normal.textColor = new Color(0.84f, 0.76f, 0.87f);
        }
    }
}
