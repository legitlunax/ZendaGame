using System;
using System.IO;
using UnityEngine;

namespace Genesis
{
    // Add to a GameObject in a new scene. Does not alter existing scenes or saves.
    public sealed class GenesisSession : MonoBehaviour
    {
        [SerializeField] private bool demoMode = false;
        public GenesisGame Game { get; private set; }
        public event Action StateChanged;
        private float autosave;
        private bool storageReady;
        private string SavePath => Path.Combine(Application.persistentDataPath, "genesis_unity_v1.json");
        private static double Now => (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

        private void Awake()
        {
            TextAsset catalog = Resources.Load<TextAsset>("Genesis/Data/catalog");
            if (catalog == null) throw new InvalidOperationException("Genesis catalog is missing.");
            Game = new GenesisGame(GenesisCatalog.FromJson(catalog.text));
            if (demoMode) return;
            try
            {
                if (File.Exists(SavePath))
                {
                    Game.Restore(File.ReadAllText(SavePath), Now, true);
                    // Persist the new timestamp immediately so offline income is not awarded twice.
                    WriteSave();
                }
                storageReady = true;
            }
            catch (Exception error)
            {
                // Never overwrite an unreadable player save with a fresh state.
                Debug.LogError("Genesis save unavailable; persistence disabled: " + error.Message);
            }
        }

        private void Update()
        {
            if (Game == null) return;
            Game.Tick(Time.deltaTime);
            autosave += Time.unscaledDeltaTime;
            if (autosave >= 15) { autosave = 0; Save(); }
            StateChanged?.Invoke();
        }

        public void Tap() { Game.Tap(); StateChanged?.Invoke(); }
        public void Buy(string id) { if (Game.Buy(id)) { Save(); StateChanged?.Invoke(); } }
        public void ClaimChapter() { if (Game.ClaimChapter()) { Save(); StateChanged?.Invoke(); } }
        public void Equip(string id) { if (Game.Equip(id)) { Save(); StateChanged?.Invoke(); } }
        public void Bloom() { if (Game.Bloom()) { Save(); StateChanged?.Invoke(); } }

        public void Save()
        {
            if (demoMode || !storageReady || Game == null) return;
            try { WriteSave(); }
            catch (Exception error) { Debug.LogError("Genesis save failed: " + error.Message); }
        }

        private void WriteSave()
        {
            Directory.CreateDirectory(Application.persistentDataPath);
            string temporary = SavePath + ".tmp";
            File.WriteAllText(temporary, Game.SaveJson(Now));
            if (File.Exists(SavePath)) File.Replace(temporary, SavePath, SavePath + ".bak");
            else File.Move(temporary, SavePath);
        }

        private void OnApplicationPause(bool paused) { if (paused) Save(); }
        private void OnApplicationFocus(bool focused) { if (!focused) Save(); }
        private void OnApplicationQuit() { Save(); }
    }
}