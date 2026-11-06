using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Explicit scene lists keep preview/demo scenes out of every player build.
public static class GenesisBuild
{
    public const string ScenePath = "Assets/Genesis/Scenes/ZendaSanctum.unity";
    // The boot scene always loads first: it shows the loading screen and warms
    // up assets and shaders before the Sanctum scene is allowed to start.
    public const string BootScenePath = "Assets/Genesis/Scenes/GenesisBoot.unity";
    public const string AndroidIdentifier = "com.zendagame.genesis";
    public const string ProductName = "ZendaGenesis";
    public const string IconKitPath = "Assets/Genesis/Art/Branding/IconKit/";
    public const string IconPath = IconKitPath + "PNG/ZendaGenesis_AppIcon_1024.png";

    public static string OutputPath(BuildTarget target)
    {
        switch (target)
        {
            case BuildTarget.StandaloneWindows64: return "Builds/Windows/ZendaGenesis.exe";
            case BuildTarget.Android: return "Builds/Android/ZendaGenesis.apk";
            case BuildTarget.WebGL: return "Builds/WebGL";
            default: throw new ArgumentOutOfRangeException(nameof(target), target, "Unsupported Genesis platform.");
        }
    }

    [MenuItem("Genesis/Build/Configure Windows, Android and WebGL")]
    public static void Configure()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Stop Play mode before configuring builds.");
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(ScenePath) == null)
            throw new FileNotFoundException("The saved Sanctum scene is required.", ScenePath);
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath) == null)
            throw new FileNotFoundException("The boot scene with the loading screen is required.", BootScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(BootScenePath, true),
            new EditorBuildSettingsScene(ScenePath, true)
        };
        // Keep the preload list in step with the art that actually ships.
        Genesis.EditorTools.GenesisBootManifestBuilder.Rebuild();
        // Collect the shader variants the game uses and register them in the
        // graphics settings. The variants stay in the build and Unity warms
        // them in the background, a few milliseconds per frame, instead of
        // the boot screen compiling every shader in one blocking step.
        GenesisShaderVariantCollector.Build();
        PlayerSettings.productName = ProductName;
        PlayerSettings.companyName = "Gonzalo Ciocca";
        ConfigureFairyIcon();
        // Zenda is a mobile game that runs on every platform. The whole
        // layout is a single portrait column of 390x700 points, and every
        // platform shows exactly that: the desktop opens a phone shaped
        // window (GenesisWindowsSupport.ApplyMobileWindow), the browser
        // opens a portrait canvas of the same size, and the phone is locked
        // to the portrait shape. A landscape window would show that column
        // as a narrow strip inside a wide screen, which is not what the
        // game looks like.
        PlayerSettings.defaultScreenWidth = Genesis.GenesisSanctum.DesktopWindowWidth;
        PlayerSettings.defaultScreenHeight = Genesis.GenesisSanctum.DesktopWindowHeight;
        PlayerSettings.defaultWebScreenWidth = Genesis.GenesisSanctum.DesktopWindowWidth;
        PlayerSettings.defaultWebScreenHeight = Genesis.GenesisSanctum.DesktopWindowHeight;
        PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
        PlayerSettings.defaultIsNativeResolution = false;
        // The window has to be able to grow. A player built without a
        // resizable window cannot be resized, cannot be maximized and stays
        // the small size it was created with, so the game looked trapped in
        // a narrow strip in the taskbar. The layout scales itself, so a
        // bigger window is simply drawn bigger.
        PlayerSettings.resizableWindow = true;
        // Alt+Enter and F11 switch between the window and the full screen.
        PlayerSettings.allowFullscreenSwitch = true;
        // Windows suspends a player that loses focus while run in background is
        // disabled. The window then stays on screen, frozen, and a launch that
        // does not take focus never finishes initializing.
        PlayerSettings.runInBackground = true;
        // A window that is hidden when it loses focus reads as a launch that
        // never happened: the game looks stuck in the background. The window
        // stays on screen instead.
        PlayerSettings.visibleInBackground = true;
        // Unity own single instance check is a hard block. It shows an
        // "Another instance is already running" message and kills the new
        // process before a window ever appears. Started from a direct link the
        // check fired even when nothing was running, so the game never opened.
        // The soft hand over in GenesisWindowsInstance replaces it and never
        // blocks a launch.
        PlayerSettings.forceSingleInstance = false;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone, ScriptingImplementation.Mono2x);
        // Use Direct3D 11 directly. Unity probes Direct3D 12 first and only
        // falls back after the device filter rejects it, which delays the
        // first frame of every launch.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.StandaloneWindows64, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows64, new[] { GraphicsDeviceType.Direct3D11 });
        PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, AndroidIdentifier);
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel26;
        PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;
        // The phone keeps the portrait shape. The layout is a single column,
        // so a rotated device would show it in a narrow strip.
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = true;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });
        EditorUserBuildSettings.buildAppBundle = false;
        EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
        PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.Android, ManagedStrippingLevel.Low);
        PlayerSettings.SetManagedStrippingLevel(NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
        // The browser shows the game in a portrait window that keeps the shape
        // of the design. The page around it is dark, so the game reads as the
        // phone game it is instead of a wide landscape canvas.
        PlayerSettings.WebGL.template = "PROJECT:Mobile";
        PlayerSettings.WebGL.threadsSupport = false;
        // The browser starts with this much memory and grows it when the game
        // asks for more. Growing the heap in a browser copies the whole heap,
        // and a growth in the middle of the boot is one of the reasons the
        // loading screen froze at eighty percent in the browser and on the
        // phone. The game holds the character, the fonts, the music and the
        // backgrounds at once, so the heap starts big enough for the boot.
        PlayerSettings.WebGL.initialMemorySize = 512;
        PlayerSettings.WebGL.maximumMemorySize = 1024;
        PlayerSettings.WebGL.memoryGrowthMode = WebGLMemoryGrowthMode.Geometric;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        PlayerSettings.WebGL.dataCaching = true;
        // Use WebGL 2, not an experimental graphics backend.
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });
        ForceLegacyInputOnly();
        AssetDatabase.SaveAssets();
        Debug.Log("Genesis build settings configured. No scenes were saved.");
    }

    // The game reads the classic input manager only. With the new input
    // system switched on the player hangs before the first managed frame,
    // because the shell libraries that Explorer injects into a shortcut
    // launch deadlock the native activation of the new input backend. Android
    // also rejects the "Both" mode. The setting lives in the input system
    // package, so it is written through that package's own helper and every
    // build forces it back to the legacy manager.
    private static void ForceLegacyInputOnly()
    {
        try
        {
            System.Type helper = null;
            // Only the assemblies that are really loaded are searched, so an
            // unloaded one can never be mistaken for the helper.
            foreach (System.Reflection.Assembly assembly in UnityEngine.Assemblies.CurrentAssemblies.GetLoadedAssemblies())
            {
                helper = assembly.GetType("UnityEngine.InputSystem.Editor.EditorPlayerSettingHelpers");
                if (helper != null) break;
            }
            if (helper == null) return;
            System.Reflection.BindingFlags flags = System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static;
            System.Reflection.PropertyInfo newBackends = helper.GetProperty("newSystemBackendsEnabled", flags);
            System.Reflection.PropertyInfo oldBackends = helper.GetProperty("oldSystemBackendsEnabled", flags);
            if (newBackends != null) newBackends.SetValue(null, false, null);
            if (oldBackends != null) oldBackends.SetValue(null, true, null);
        }
        catch (System.Exception error)
        {
            Debug.LogWarning("Could not force the legacy input manager: " + error.Message);
        }
    }

    [MenuItem("Genesis/Build/Windows EXE")]
    public static void Windows() { Build(BuildTarget.StandaloneWindows64); }

    [MenuItem("Genesis/Build/Android APK")]
    public static void Android() { Build(BuildTarget.Android); }

    [MenuItem("Genesis/Build/WebGL")]
    public static void WebGL() { Build(BuildTarget.WebGL); }

    public static void ConfigureFairyIcon()
    {
        Texture2D icon = AssetDatabase.LoadAssetAtPath<Texture2D>(IconPath);
        if (icon == null) throw new FileNotFoundException("Zenda application logo is missing.", IconPath);
        foreach (NamedBuildTarget target in new[] { NamedBuildTarget.Unknown, NamedBuildTarget.Standalone, NamedBuildTarget.WebGL })
        {
            int[] sizes = PlayerSettings.GetIconSizes(target, IconKind.Any);
            var icons = new Texture2D[sizes.Length];
            for (int i = 0; i < icons.Length; i++)
                icons[i] = target == NamedBuildTarget.Standalone
                    ? AssetDatabase.LoadAssetAtPath<Texture2D>(IconKitPath + "Windows/PNG/ZendaGenesis_" + sizes[i] + ".png")
                        ?? AssetDatabase.LoadAssetAtPath<Texture2D>(IconKitPath + "PNG/ZendaGenesis_Logo_Transparent_1024.png")
                    : icon;
            PlayerSettings.SetIcons(target, icons, IconKind.Any);
        }
        // Android's legacy launcher icons; the default icon also provides the platform fallback.
        int[] androidSizes = PlayerSettings.GetIconSizes(NamedBuildTarget.Android, IconKind.Any);
        var androidIcons = new Texture2D[androidSizes.Length];
        Texture2D legacy = AssetDatabase.LoadAssetAtPath<Texture2D>(IconKitPath + "Android/Unity/ZendaGenesis_Legacy_1024.png");
        for (int i = 0; i < androidIcons.Length; i++) androidIcons[i] = legacy;
        PlayerSettings.SetIcons(NamedBuildTarget.Android, androidIcons, IconKind.Any);
        foreach (PlatformIconKind kind in PlayerSettings.GetSupportedIconKinds(NamedBuildTarget.Android))
        {
            PlatformIcon[] slots = PlayerSettings.GetPlatformIcons(NamedBuildTarget.Android, kind);
            foreach (PlatformIcon slot in slots)
            {
                if (slot.maxLayerCount > 1)
                    slot.SetTextures(new[] {
                        AssetDatabase.LoadAssetAtPath<Texture2D>(IconKitPath + "Android/Unity/ZendaGenesis_Adaptive_Background_1024.png"),
                        AssetDatabase.LoadAssetAtPath<Texture2D>(IconKitPath + "Android/Unity/ZendaGenesis_Adaptive_Foreground_1024.png") });
                else if (kind.ToString().IndexOf("Monochrome", StringComparison.OrdinalIgnoreCase) < 0)
                    slot.SetTexture(legacy);
            }
            PlayerSettings.SetPlatformIcons(NamedBuildTarget.Android, kind, slots);
        }
    }

    public static void Build(BuildTarget target)
    {
        string output = OutputPath(target);
        if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
            throw new InvalidOperationException("Stop Play mode and wait for compilation before building.");
        if (!BuildPipeline.IsBuildTargetSupported(BuildPipeline.GetBuildTargetGroup(target), target))
            throw new InvalidOperationException("Install " + target + " Build Support for this Unity version in Unity Hub. Android also needs SDK, NDK and OpenJDK.");
        // Platform switching reloads scripts. Require explicit selection or -buildTarget
        // rather than silently changing target while a user has unsaved preview work.
        if (EditorUserBuildSettings.activeBuildTarget != target)
            throw new InvalidOperationException("Select " + target + " in Build Profiles first, or run the documented batch command with -buildTarget. Save/preserve your own work before switching; this command never saves scenes.");
        Scene scene = SceneManager.GetSceneByPath(ScenePath);
        if (scene.IsValid() && scene.isLoaded && scene.isDirty)
            throw new InvalidOperationException("The Sanctum scene has unsaved changes. Save it explicitly before building. Preview scenes must not be included.");
        Configure();
        Directory.CreateDirectory(target == BuildTarget.WebGL ? output : Path.GetDirectoryName(output));
        BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = new[] { BootScenePath, ScenePath },
            locationPathName = output,
            target = target,
            options = BuildOptions.None
        });
        if (report == null) throw new BuildFailedException("Unity did not return a build report.");
        string result = DateTime.UtcNow.ToString("O") + " " + target + ": " + report.summary.result
            + " errors=" + report.summary.totalErrors + " warnings=" + report.summary.totalWarnings
            + " bytes=" + report.summary.totalSize + " output=" + Path.GetFullPath(output);
        // The path must be absolute. A relative path resolves against the
        // current working directory, which is not the project folder.
        string resultPath = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Builds", target + "-build-result.txt"));
        File.WriteAllText(resultPath, result);
        if (report.summary.result != BuildResult.Succeeded) throw new BuildFailedException(result);
        Debug.Log(result);
    }
}
