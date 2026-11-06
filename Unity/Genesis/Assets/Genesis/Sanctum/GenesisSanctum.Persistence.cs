using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Genesis
{
    public sealed partial class GenesisSanctum
    {
        private bool persistenceReady, suspended, restoring;
        private bool resetting;
        private float saveTimer;
        private double suspendedAt;
        private static double UtcSeconds => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        // Never mix editor smoke tests / previews with the player's release save.
        private string PlayerSavePath => Path.Combine(Application.persistentDataPath,
            Application.isEditor ? "genesis_sanctum_editor_v2.json" : "genesis_sanctum_v2.json");
        public double LastOfflineGain { get; private set; }
        public string PersistenceStatus { get; private set; } = "Local save ready.";

        public string ExportPlayerState(double timestamp)
        {
            return new JObject
            {
                ["version"] = 2, ["timestamp"] = timestamp,
                ["game"] = JObject.Parse(Game.SaveJson(timestamp)), ["gathering"] = Gathering.SaveState(),
                ["speed"] = GameSpeed, ["tab"] = SelectedTab, ["style"] = SceneStyle,
                ["portrait"] = PortraitCloseup, ["yaw"] = GoddessYaw,
                ["companion"] = companionLadyName, ["dimension"] = SelectedDimension,
                ["route"] = SelectedRoute, ["wins"] = ExpeditionWins,
                ["activeRoute"] = ActiveRoute, ["expeditionDimension"] = ExpeditionDimension,
                ["running"] = ExpeditionRunning, ["ready"] = ExpeditionReady,
                ["remaining"] = ExpeditionRemaining, ["duration"] = ExpeditionDuration,
                ["reward"] = ExpeditionReward, ["creditsSeen"] = creditsSeen,
                ["gatheringSection"] = gatheringSection, ["gatheringWorld"] = gatheringWorld,
                ["audioEnabled"] = AudioEnabled, ["halo"] = EquippedHalo,
                ["protagonist"] = protagonist.Save(), ["language"] = Language
            }.ToString();
        }

        public void ImportPlayerState(string json, double now)
        {
            JObject state = JObject.Parse(json);
            if ((int?)state["version"] != 2 || !(state["game"] is JObject))
                throw new InvalidDataException("Unsupported player save version.");
            // Parse the model before replacing the running game.
            var model = new GenesisGame(Game.Catalog);
            model.Restore(state["game"].ToString(), now, false);
            var gathering = new GenesisGathering(model);
            gathering.RestoreState(state["gathering"] as JObject);
            restoring = true;
            try
            {
                Game = model;
                Gathering = gathering;
                Localization.Select("en");
                Localization.Select((string)state["language"]);
                enchantedPageKey = null;
                string savedHalo = (string)state["halo"];
                EquippedHalo = IsHaloUnlocked(savedHalo) ? savedHalo : "angel";
                haloPicker = false;
                unlockedChronicles.Clear();
                RefreshChronicleUnlocks();
                float speed = (float?)state["speed"] ?? 1;
                GameSpeed = speed == 10 || speed == 30 || speed == 50 || speed == 100 || speed == MaximumGameSpeed ? speed : 1;
                ExpeditionWins = Math.Max(0, (int?)state["wins"] ?? 0);
                SelectedDimension = Mathf.Clamp((int?)state["dimension"] ?? 0, 0, Dimensions.Count - 1);
                SelectedRoute = Mathf.Clamp((int?)state["route"] ?? 0, 0, 2);
                ActiveRoute = Mathf.Clamp((int?)state["activeRoute"] ?? 0, 0, 2);
                ExpeditionDimension = Mathf.Clamp((int?)state["expeditionDimension"] ?? 0, 0, Dimensions.Count - 1);
                ExpeditionDuration = Mathf.Clamp((float?)state["duration"] ?? 30, 30, 90);
                ExpeditionRemaining = Mathf.Clamp((float?)state["remaining"] ?? 0, 0, ExpeditionDuration);
                ExpeditionReward = (double?)state["reward"] ?? 0;
                if (!EconomyMath.IsFinite(ExpeditionReward) || ExpeditionReward < 0) ExpeditionReward = 0;
                ExpeditionReady = (bool?)state["ready"] == true;
                ExpeditionRunning = !ExpeditionReady && (bool?)state["running"] == true;
                companionLadyName = Game.Catalog.ResolveFairyName((string)state["companion"]);
                creditsSeen = (bool?)state["creditsSeen"] == true;
                AudioEnabled = (bool?)state["audioEnabled"] ?? true;
                RefreshAudioPlayback();
                GoddessYaw = Mathf.Repeat((float?)state["yaw"] ?? 0, 360);
                SetSceneStyle((int?)state["style"] ?? 0);
                SetPortraitCloseup((bool?)state["portrait"] == true);
                SelectTab((int?)state["tab"] ?? 0);
                SelectGatheringSection(Mathf.Clamp((int?)state["gatheringSection"] ?? 0, 0, 2));
                gatheringWorld = Mathf.Clamp((int?)state["gatheringWorld"] ?? -1, -1, Dimensions.Count - 1);
                double timestamp = (double?)state["timestamp"] ?? now;
                CatchUp(EconomyMath.IsFinite(timestamp) && timestamp > 0 ? Math.Max(0, now - timestamp) : 0);
                RefreshChronicleUnlocks();
                CheckCompletion();
                RestoreProtagonist(state["protagonist"] as JObject);
            }
            finally { restoring = false; }
        }

        private void CatchUp(double elapsed)
        {
            double before = Game.Quanta;
            double seconds = elapsed * GameSpeed;
            // Split only at the temporary multiplier boundary, not once per offline frame.
            double boosted = Math.Min(seconds, Game.ChronoTimer);
            Gathering.Advance(boosted);
            Game.Tick(boosted, false);
            Gathering.Advance(seconds - boosted);
            Game.Tick(seconds - boosted, false);
            if (ExpeditionRunning) AdvanceExpedition((float)Math.Min(float.MaxValue, seconds));
            LastOfflineGain = Game.Quanta - before;
            if (LastOfflineGain > 0) message = "Welcome back! Your fairies gathered " + Num(LastOfflineGain) + " while you were away.";
        }

        private void LoadPlayerProgress()
        {
            if (!Application.isPlaying) return;
            try { MigrateRenamedPlayerSave(); }
            catch (Exception error)
            {
                PersistenceStatus = "Previous save preserved; migration failed: " + error.Message;
                Debug.LogError(PersistenceStatus);
                return;
            }
            string path = PlayerSavePath;
            bool exists = File.Exists(path) || File.Exists(path + ".bak") || File.Exists(path + ".tmp");
            Exception failure = null;
            foreach (string candidate in new[] { path, path + ".tmp", path + ".bak" })
            {
                if (!File.Exists(candidate)) continue;
                try
                {
                    ImportPlayerState(File.ReadAllText(candidate), UtcSeconds);
                    persistenceReady = true;
                    SavePlayerProgress();
                    return;
                }
                catch (Exception error) { failure = error; }
            }
            if (exists)
            {
                PersistenceStatus = "Save could not be loaded; existing files are preserved. " + failure?.Message;
                Debug.LogError(PersistenceStatus);
                return;
            }
            // Upgrade the older standalone-session save if present; never delete it.
            string legacy = Path.Combine(Application.persistentDataPath, "genesis_unity_v1.json");
            if (!Application.isEditor && File.Exists(legacy))
            {
                try { Game.Restore(File.ReadAllText(legacy), UtcSeconds, true); }
                catch (Exception error) { PersistenceStatus = "Legacy save preserved: " + error.Message; Debug.LogError(PersistenceStatus); return; }
            }
            persistenceReady = true;
            SavePlayerProgress();
        }

        private void MigrateRenamedPlayerSave()
        {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
            // Windows derives persistentDataPath from company/product. Copy, never move,
            // the former product's saves only when the new product has no save at all.
            string directory = Path.GetDirectoryName(PlayerSavePath);
            string oldDirectory = Path.Combine(Path.GetDirectoryName(directory), "Genesis");
            if (string.Equals(directory, oldDirectory, StringComparison.OrdinalIgnoreCase)) return;
            foreach (string suffix in new[] { "", ".bak", ".tmp" })
                if (File.Exists(PlayerSavePath + suffix)) return;
            Directory.CreateDirectory(directory);
            string name = Path.GetFileName(PlayerSavePath);
            foreach (string suffix in new[] { "", ".bak", ".tmp" })
            {
                string old = Path.Combine(oldDirectory, name + suffix);
                if (File.Exists(old)) File.Copy(old, PlayerSavePath + suffix, false);
            }
            string legacy = "genesis_unity_v1.json";
            if (!Application.isEditor && !File.Exists(Path.Combine(directory, legacy)) && File.Exists(Path.Combine(oldDirectory, legacy)))
                File.Copy(Path.Combine(oldDirectory, legacy), Path.Combine(directory, legacy), false);
#endif
        }

        public void SavePlayerProgress()
        {
            if (!persistenceReady || restoring || resetting || Game == null || !Application.isPlaying) return;
            try
            {
                Directory.CreateDirectory(Application.persistentDataPath);
                string path = PlayerSavePath;
                // Preserve the suspension timestamp until catch-up has actually been applied.
                File.WriteAllText(path + ".tmp", ExportPlayerState(suspended ? suspendedAt : UtcSeconds));
                if (File.Exists(path)) File.Copy(path, path + ".bak", true);
                File.Copy(path + ".tmp", path, true);
                File.Delete(path + ".tmp");
#if UNITY_WEBGL && !UNITY_EDITOR
                GenesisSyncSave();
#endif
                PersistenceStatus = "Progress saved locally.";
            }
            catch (Exception error) { PersistenceStatus = "Save failed: " + error.Message; Debug.LogError(PersistenceStatus); }
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("__Internal")]
        private static extern void GenesisSyncSave();
#endif

        private bool lostFocus, appPaused;
        private void SetSuspension(bool focusLost, bool paused)
        {
            lostFocus = focusLost;
            appPaused = paused;
            // Focus callbacks can run after editor domain reload, before a model
            // exists; never attempt offline catch-up on a missing session.
            if (Game == null || Gathering == null) { suspended = false; return; }
            bool inactive = lostFocus || appPaused;
            if (inactive && !suspended)
            {
                suspendedAt = UtcSeconds;
                suspended = true;
                SavePlayerProgress();
            }
            else if (!inactive && suspended)
            {
                suspended = false;
                CatchUp(Math.Max(0, UtcSeconds - suspendedAt));
                SavePlayerProgress();
            }
        }

        private void OnApplicationQuit() { SavePlayerProgress(); }
    }
}