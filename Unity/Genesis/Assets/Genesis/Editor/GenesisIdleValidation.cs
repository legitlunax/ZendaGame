using System;
using System.IO;
using Genesis;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

public static class GenesisIdleValidation
{
    [MenuItem("Genesis/Validate/Idle persistence, VIP and completion")]
    public static void Run()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Run outside Play mode to protect player saves.");
        var root = new GameObject("Idle validation temporary");
        var app = root.AddComponent<GenesisSanctum>();
        app.enabled = false;
        if (app.Game == null)
            typeof(GenesisSanctum).GetMethod("Awake", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(app, null);
        int checks = 0;
        Action<bool, string> check = (ok, label) => { if (!ok) throw new Exception(label); checks++; };
        try
        {
            check(!app.OpenCompletionCredits(), "credits locked on new game");
            foreach (int speed in new[] { 10, 30, 50 })
            {
                check(app.SubmitUnlockCode(" VIPx" + speed + " "), "VIP accepted");
                check(app.GameSpeed == speed, "VIP replaces rather than stacks");
            }
            check(!app.SubmitUnlockCode("vipx999") && app.GameSpeed == 50, "invalid VIP unchanged");
            check(app.SubmitUnlockCode(" ZeNdAxHoPe ") && app.GameSpeed == 1000000, "million speed code ignores case and surrounding whitespace");
            check(app.SubmitUnlockCode("zendaxhope") && app.GameSpeed == 1000000, "million speed does not stack");
            string boosted = app.ExportPlayerState(1000);
            check(app.SubmitUnlockCode("GONZALO") && app.GameSpeed == 100, "legacy code remains 100x");
            app.ImportPlayerState(boosted, 1000);
            check(app.GameSpeed == 1000000, "million speed restored from save");
            check(!app.SubmitUnlockCode("zendaxhope2") && app.GameSpeed == 1000000, "invalid code preserves million speed");
            check(app.SubmitUnlockCode("vipx50") && app.GameSpeed == 50, "VIP replaces million speed");
            foreach (string language in GenesisLocalization.Languages)
            {
                check(app.SelectLanguage(language) && app.Language == language, "language selection " + language);
                string localized = app.ExportPlayerState(1000);
                app.SelectLanguage("en");
                app.ImportPlayerState(localized, 1000);
                check(app.Language == language, "language saved and restored " + language);
            }
            check(!app.SelectLanguage("invalid") && app.Language == "zh", "invalid language leaves selection unchanged");
            var oldSave = JObject.Parse(app.ExportPlayerState(1000));
            oldSave.Remove("language");
            app.ImportPlayerState(oldSave.ToString(), 1000);
            check(app.Language == "en", "older saves default to English");
            foreach (string fontName in new[] { "NotoSans-Regular", "NotoSansCJKsc-Regular" })
            {
                Font font = Resources.Load<Font>("Genesis/Fonts/" + fontName);
                check(font != null && font.dynamic, "bundled localization font " + fontName);
                foreach (char character in fontName.Contains("CJK") ? "简体中文语言选择你的主角采集希望" : "EspañolРусскийЯзыкВыберитегероя")
                    check(font.HasCharacter(character), "localized font glyph " + character);
            }
            app.Game.AddQuanta(100);
            string first = app.Game.Catalog.generators[0].id;
            check(app.Awaken(first), "starter fairy");
            app.Game.BuyMode = 10;
            app.Gathering.Advance(0.75);
            app.SetPortraitCloseup(true);
            app.SelectTab(1);
            check(app.BeginExpedition(), "journey starts");
            string saved = app.ExportPlayerState(1000);
            double before = app.Game.Quanta;
            double rate = app.Game.TotalQps;
            app.ImportPlayerState(saved, 1000 + 86400);
            check(app.GameSpeed == 50 && app.Game.BuyMode == 10, "speed and buy configuration restored");
            check(app.PortraitCloseup && app.SelectedTab == 1, "presentation restored");
            check(app.ExpeditionReady && !app.ExpeditionRunning, "offline journey finishes");
            check(Math.Abs(app.LastOfflineGain - rate * 86400 * 50) < rate * 2, "full VIP offline gathering including cycle remainder");
            check(app.Game.Quanta > before && app.Game.Counts[first] == 1, "progress restored");
            check(app.ClaimExpedition() && !app.ClaimExpedition(), "journey claim exactly once");
            string after = app.ExportPlayerState(1000 + 86400);
            app.ImportPlayerState(after, 1000 + 86400);
            check(app.LastOfflineGain == 0, "reloading same timestamp cannot duplicate income");
            var model = JObject.Parse(app.Game.SaveJson(100));
            model["quanta"] = 1e100;
            model["current_run_quanta"] = 1e100;
            foreach (JToken row in (JArray)model["counts"]) row["value"] = 100;
            app.Game.Restore(model.ToString(), 100);
            foreach (var chapter in app.Game.Catalog.chapters)
            {
                check(app.Game.FinishStory(chapter.id), "story readable " + chapter.id);
            }
            check(app.Game.CampaignCompleted && app.Game.CompletedChapters.Count == 0, "optional gifts not required for completion");
            foreach (var chapter in app.Game.Catalog.chapters)
                check(app.Game.ClaimChapter(chapter.id), "story reward " + chapter.id);
            check(app.Game.CampaignCompleted && app.OpenCompletionCredits(), "completion credits unlock");
            app.CloseCompletionCredits();
            check(app.OpenCompletionCredits(), "credits reopen after continue");
            app.CloseCompletionCredits();
            saved = app.ExportPlayerState(1000);
            app.ImportPlayerState(saved, 1000);
            check(app.Game.CampaignCompleted && app.OpenCompletionCredits(), "completion survives restart");
            app.CloseCompletionCredits();
            app.SubmitUnlockCode("RESET");
            app.CloseCodePanel();
            check(!app.ConfirmResetCode() && app.Game.CampaignCompleted, "cancel reset preserves completion");
            app.SubmitUnlockCode("RESET");
            check(app.ConfirmResetCode(), "reset confirmed");
            check(!app.Game.CampaignCompleted && !app.CreditsOpen && !app.OpenCompletionCredits(), "completion reset");
            check(app.GameSpeed == 1 && app.Game.Quanta == 0 && app.ExpeditionWins == 0, "economy speed journeys reset");
            string report = DateTime.UtcNow.ToString("O") + " PASS " + checks + " Sanctum idle/VIP/completion regression checks.";
            Directory.CreateDirectory("Builds");
            File.WriteAllText("Builds/idle-sanctum-validation.txt", report);
            Debug.Log(report);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
}