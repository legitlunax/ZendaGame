"""Verify copied artwork and GUID uniqueness without modifying Unity assets."""
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
KIT = ROOT / "Assets/EnchantedUI"
manifest = json.loads((KIT / "Documentation/SPRITE_MANIFEST.json").read_text(encoding="utf-8"))
checks = 0
for sprite in manifest["sprites"]:
    image = KIT / "Art" / sprite["file"]
    assert hashlib.sha256(image.read_bytes()).hexdigest() == sprite["sha256"], image
    checks += 1
guids = {}
for meta in (ROOT / "Assets").rglob("*.meta"):
    match = re.search(r"^guid: ([a-f0-9]{32})$", meta.read_text(encoding="utf-8-sig"), re.M)
    if match:
        guid = match.group(1)
        assert guid not in guids, f"Duplicate GUID: {meta} and {guids.get(guid)}"
        guids[guid] = meta
checks += 1
assert (ROOT / "Assets/Genesis/Resources/Genesis/UI/EnchantedConversation.prefab").is_file()
checks += 1
report = f"PASS: {checks} source integrity checks; {len(guids)} unique asset GUIDs.\n"
(ROOT / "Builds/enchanted-source-validation.txt").write_text(report, encoding="utf-8")
print(report)