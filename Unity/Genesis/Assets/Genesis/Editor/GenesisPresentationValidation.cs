using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Genesis;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class GenesisPresentationValidation
{
    [MenuItem("Genesis/Validate/RPG fonts and fairy audio")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run outside Play mode.");
        int checks = 0;
        Action<bool, string> check = (ok, label) => { if (!ok) throw new Exception(label); checks++; };
        var root = new GameObject("Presentation validation temporary");
        AudioClip chime = null;
        Texture2D icon = null;
        try
        {
            var app = root.AddComponent<GenesisSanctum>();
            app.enabled = false;
            if (app.Game == null) typeof(GenesisSanctum).GetMethod("Awake", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(app, null);
            check(app.AudioEnabled, "audio on by default");
            app.SetAudioEnabled(false);
            string saved = app.ExportPlayerState(1000);
            check((bool)JObject.Parse(saved)["audioEnabled"] == false, "mute exported");
            app.SetAudioEnabled(true);
            app.ImportPlayerState(saved, 1000);
            check(!app.AudioEnabled, "mute restored");
            var legacy = JObject.Parse(saved);
            legacy.Remove("audioEnabled");
            app.ImportPlayerState(legacy.ToString(), 1000);
            check(app.AudioEnabled, "older saves default to audio on");
            app.SetAudioEnabled(false);
            app.SubmitUnlockCode("RESET");
            check(app.ConfirmResetCode() && !app.AudioEnabled, "reset respects audio preference");
            foreach (string name in new[] { "Almendra-Bold", "CrimsonText-Regular" })
            {
                Font font = Resources.Load<Font>("Genesis/Fonts/" + name);
                check(font != null && font.dynamic, "bundled dynamic font " + name);
                foreach (char character in "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.,!?+-/:")
                    check(font.HasCharacter(character), name + " character " + character);
            }
            foreach (string name in new[] { "Asset-Credits", "Almendra-OFL", "CrimsonText-OFL" })
                check(Resources.Load<TextAsset>("Genesis/Licenses/" + name) != null, "bundled license " + name);
            AudioClip music = Resources.Load<AudioClip>("Genesis/Audio/MysticalPiano");
            check(music != null && music.length > 30 && music.samples > 0, "real background music imported");
            check(Mathf.Abs(music.length - 95) < 0.1f, "author-recommended mellow piano loop duration");
            chime = (AudioClip)typeof(GenesisSanctum).GetMethod("CreateButtonChime", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            var samples = new float[chime.samples];
            check(chime.GetData(samples, 0), "chime samples readable");
            check(samples.All(x => !float.IsNaN(x) && Mathf.Abs(x) <= 1), "chime no clipping or NaNs");
            check(samples.Any(x => Mathf.Abs(x) > 0.1f), "chime audible signal");
            check(Mathf.Abs(samples[0]) < 0.001f && Mathf.Abs(samples[samples.Length - 1]) < 0.001f, "chime faded boundaries");
            icon = (Texture2D)typeof(GenesisSanctum).GetMethod("CreateMusicIcon", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[] { false });
            check(icon != null && icon.width == 96, "procedural music icon generated");
            Rect audioRect = GenesisSanctum.GoddessAudioRect;
            check(audioRect.yMin >= GenesisSanctum.GoddessHeaderRect.yMax &&
                !audioRect.Overlaps(GenesisSanctum.GoddessHeaderRect), "audio toggle below main header");
            check(!audioRect.Overlaps(GenesisSanctum.GoddessTitleRect), "audio toggle clear of title");
            foreach (float height in new[] { 620f, 720f, 1000f })
            {
                check(audioRect.xMax < GenesisSanctum.GoddessStageRect(height).center.x, "audio at left edge of stage " + height);
                check(GenesisSanctum.GoddessControlRects(height).All(rect => !audioRect.Overlaps(rect)), "audio clear of controls " + height);
                check(!audioRect.Overlaps(GenesisSanctum.GoddessHopePanelRect(height)), "audio clear of Hope panel " + height);
            }
            check(!File.ReadAllText("Assets/Genesis/Sanctum/GenesisSanctum.Mobile.cs").Contains("DrawAudioToggle("), "audio absent from navbar");
            foreach (string path in Directory.GetFiles("Assets/Genesis/Sanctum", "*.cs"))
                if (!path.EndsWith("GenesisSanctum.Audio.cs"))
                    check(!File.ReadAllText(path).Contains("GUI.Button("), "button routed through sound " + path);
            string report = DateTime.UtcNow.ToString("O") + " PASS " + checks + " RPG font/audio/persistence checks; music duration=" + music.length + "s. Device playback/browser autoplay and visual layout still require testing.";
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/presentation-validation.txt", report);
            Debug.Log(report);
        }
        finally
        {
            if (chime != null) UnityEngine.Object.DestroyImmediate(chime);
            if (icon != null) UnityEngine.Object.DestroyImmediate(icon);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }
}