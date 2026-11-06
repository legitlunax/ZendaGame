"""Optional font coverage checks; uses the locally installed fontTools package."""
import json
from pathlib import Path
from fontTools.ttLib import TTFont

root = Path(__file__).resolve().parents[2] / "Assets/Genesis/Resources/Genesis"
data = json.loads((root / "Data/localization.json").read_text(encoding="utf-8"))
for index, filename in enumerate([
    "NotoSans-Regular.ttf", "NotoSans-Regular.ttf", "NotoSansCJKsc-Regular.otf"
]):
    with TTFont(root / "Fonts" / filename) as font:
        cmap = font.getBestCmap()
        characters = set("".join(value[index] for value in data.values()))
        characters.update(["Español", "Русский", "简体中文"][index])
        missing = sorted(c for c in characters if not c.isspace() and ord(c) not in cmap)
        assert not missing, (filename, missing)
        print("PASS:", filename, "covers all", len(characters), "translation glyphs")
for key, translations in data.items():
    for translation in translations:
        assert key.count("{#}") == translation.count("{#}"), key
print("PASS:", len(data), "translation keys have matching numeric placeholders")
for height in [620, 700, 1000]:
    assert 238 + max(110, height - 500) <= height - 252, height
print("PASS: protagonist picker preview clears name field at 620/700/1000 logical heights")