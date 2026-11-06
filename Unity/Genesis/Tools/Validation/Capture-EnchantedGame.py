"""Capture real game screens without granting currency or changing progression."""
import asyncio
import json
from pathlib import Path
from mcp import ClientSession
from mcp.client.streamable_http import streamablehttp_client

ROOT = Path(__file__).resolve().parents[2]


async def main():
    async with streamablehttp_client("http://127.0.0.1:8080/mcp") as (read, write, _):
        async with ClientSession(read, write) as session:
            await session.initialize()

            async def execute(code):
                result = await session.call_tool("execute_code", {"action": "execute", "code": code})
                data = result.structuredContent
                if result.isError or not data or not data.get("success"):
                    raise RuntimeError(result.model_dump_json())
                return data["data"]["result"]

            prefix = "var app=UnityEngine.Object.FindFirstObjectByType<Genesis.GenesisSanctum>();"
            initial = await execute(prefix + 'return new {app.SelectedTab, section=(int)typeof(Genesis.GenesisSanctum).GetField("gatheringSection",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).GetValue(app), app.LadyGalleryOpen, app.StorytellingActive};')
            if initial["StorytellingActive"] or initial["LadyGalleryOpen"]:
                raise RuntimeError("Close reader/gallery before capturing; existing state left untouched.")
            if not await execute(prefix + "return app.Game != null;"):
                raise RuntimeError("Runtime model missing after script reload; restart Play mode normally before capturing.")
            await execute(prefix + 'var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;typeof(Genesis.GenesisSanctum).GetField("gameThemeLoaded",flags).SetValue(app,false);typeof(Genesis.GenesisSanctum).GetMethod("EnsureEnchantedGameTheme",flags).Invoke(app,null);return UnityEngine.Resources.Load<EnchantedUI.EnchantedTheme>("Genesis/UI/EnchantedGameTheme") != null;')
            screens = [("goddess", "app.SelectTab(0);"), ("gather", "app.SelectTab(1);app.SelectGatheringSection(0);"),
                       ("journeys", "app.SelectTab(1);app.SelectGatheringSection(1);"), ("worlds", "app.SelectTab(1);app.SelectGatheringSection(2);"),
                       ("journal", "app.SelectTab(2);"), ("gallery", "app.OpenLadyGallery();")]
            captures = []
            try:
                for width, height in [(1080, 1920), (1280, 720)]:
                    settings = json.loads((ROOT / f"Tools/MCP/enchanted-size-{width}-{height}.json").read_text())
                    await execute(settings["code"])
                    await asyncio.sleep(1)
                    for name, action in screens:
                        await execute(prefix + "app.CloseLadyGallery();" + action + "return app.SelectedTab;")
                        await asyncio.sleep(0.5)
                        path = ROOT / f"Builds/enchanted-game-{name}-{width}-{height}.png"
                        await execute('UnityEngine.ScreenCapture.CaptureScreenshot("' + path.as_posix() + '");return new {UnityEngine.Screen.width,UnityEngine.Screen.height};')
                        await asyncio.sleep(1)
                        if not path.is_file():
                            raise RuntimeError(f"Capture missing: {path}")
                        captures.append(str(path))
                        print(path.name, path.stat().st_size, flush=True)
            finally:
                await execute(prefix + f'app.CloseLadyGallery();app.SelectGatheringSection({initial["section"]});app.SelectTab({initial["SelectedTab"]});return app.SelectedTab;')
            report = {"captures": captures, "restored_tab": initial["SelectedTab"], "note": "Rendering captures, not physical-input or device validation."}
            (ROOT / "Builds/enchanted-game-captures.json").write_text(json.dumps(report, indent=2))


asyncio.run(main())