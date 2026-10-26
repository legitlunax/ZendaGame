"""Package allowlisted project content; never read user saves or credentials."""
from pathlib import Path
import hashlib
import json
import math
import shutil
import struct
import zipfile

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / "UnityMigration"


def included(path):
    return (path.is_file() and "__pycache__" not in path.parts
            and path.suffix not in {".pyc", ".import", ".uid"})


def read(path):
    return json.loads(path.read_text(encoding="utf-8-sig"))


def validate():
    catalog = read(OUT / "Data/catalog.json")
    rows = read(ROOT / "data/creation_stages.json")
    generators = catalog["generators"]
    chapters = catalog["chapters"]
    assert len(rows) == len(generators) == 106
    assert len(chapters) == 107
    assert len({g["id"] for g in generators}) == len(generators)
    assert len({c["id"] for c in chapters}) == len(chapters)
    for i, (row, gen) in enumerate(zip(rows, generators)):
        assert [gen[k] for k in ("id", "name", "fairy", "era", "desc")] == row[:5]
        assert gen["dimension"] == (row[7] if len(row) > 7 else "")
        assert math.isclose(gen["base_cost"], 5 * 3.2 ** i, rel_tol=1e-12)
        assert math.isclose(gen["base_prod"], max(.5, gen["base_cost"] * .025), rel_tol=1e-12)
        assert chapters[i + 1]["kind"] == gen["id"]
    ids = {c["id"] for c in chapters}
    for entry in catalog["emotions"] + catalog["wardrobe"]:
        chapter = entry.get("chapter_id", entry.get("chapter", ""))
        assert not chapter or chapter in ids
    assert len(catalog["dimensions"]) == 29
    assert len(catalog["wardrobe"]) == 6
    assert len(catalog["emotions"]) == 10
    assert len(catalog["demo_presets"]) == 18
    snapshots = read(OUT / "Data/demo_snapshots.json")["snapshots"]
    assert len(snapshots) == 18
    for snap in snapshots:
        assert set(snap["state"]["counts"]) == {g["id"] for g in generators}
        assert snap["state"]["equipped_appearance"] == "ivory_queen"
    fixtures = read(OUT / "Data/parity_fixtures.json")
    assert len(fixtures["bulk_cases"]) == 120
    by_id = {g["id"]: g for g in generators}
    for case in fixtures["bulk_cases"]:
        gen = by_id[case["generator_id"]]
        rate = gen["cost_mult"]
        first = gen["base_cost"] * case["discount"] * rate ** case["owned"]
        amount = case["buy_mode"]
        if amount == -1:
            amount = max(1, math.floor(math.log(1 + case["balance"] * (rate - 1) / first) / math.log(rate))) if case["balance"] >= first else 1
        expected = first if amount == 1 else first * (rate ** amount - 1) / (rate - 1)
        actual = case["bulk_result"]
        assert amount == actual["amount"]
        assert math.isclose(expected, actual["cost"], rel_tol=1e-6)
        assert actual["can_afford"] == (case["balance"] >= expected)
        qps = gen["base_prod"] * case["owned"] * 2 ** (case["owned"] // 25) * case["global_multiplier"]
        assert math.isclose(qps, case["generator_qps"], rel_tol=1e-6)
    poses = read(OUT / "Data/emotion_pose_samples.json")
    assert len(poses["samples"]) == 60
    glb = (OUT / "Data/fairy_prototype.glb").read_bytes()
    magic, version, length = struct.unpack_from("<III", glb)
    assert magic == 0x46546C67 and version == 2 and length == len(glb)
    chunk_length, chunk_type = struct.unpack_from("<II", glb, 12)
    assert chunk_type == 0x4E4F534A
    gltf = json.loads(glb[20:20 + chunk_length])
    assert gltf["meshes"] and gltf["nodes"]
    assert not gltf.get("skins")
    report = {"status": "passed", "discoveries": len(generators), "chapters": len(chapters),
              "dimensions": 29, "demo_snapshots": 18, "bulk_parity_cases": 120,
              "pose_samples": 60, "glb_meshes": len(gltf["meshes"]),
              "unity_editor_tested": False, "live_player_save_included": False}
    (OUT / "VALIDATION.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    return report


def main():
    # Exact allowlist: excludes .godot, builds, local settings, credentials, and user://.
    for folder in ("scripts", "scenes", "shaders", "tests", "data"):
        for path in (ROOT / folder).rglob("*"):
            if path.is_file() and path.suffix in {".gd", ".tscn", ".gdshader", ".json"}:
                dest = OUT / "SourceSnapshot" / path.relative_to(ROOT)
                dest.parent.mkdir(parents=True, exist_ok=True)
                shutil.copy2(path, dest)
    for name in ("project.godot", "README.md", "PROGRESSION.md", "FAIRY_3D.md", "PLATFORM_GUIDE.md"):
        shutil.copy2(ROOT / name, OUT / "SourceSnapshot" / name)
    for name in ("generate_assets.py", "ExportUnityData.gd", "package_unity_migration.py"):
        dest = OUT / "SourceSnapshot/tools" / name
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(ROOT / "tools" / name, dest)
    manifest = read(ROOT / "assets/generated/manifest.json")
    for asset in manifest["assets"]:
        relative = Path(asset["path"].removeprefix("res://"))
        src = ROOT / relative
        assert hashlib.sha256(src.read_bytes()).hexdigest() == asset["sha256"]
        dest = OUT / "ReusableAssets" / relative
        dest.parent.mkdir(parents=True, exist_ok=True)
        shutil.copy2(src, dest)
    shutil.copy2(ROOT / "assets/generated/manifest.json", OUT / "ReusableAssets/manifest.json")
    shutil.copy2(ROOT / "assets/generated/ASSET_NOTES.md", OUT / "ReusableAssets/ASSET_NOTES.md")
    screen_dir = OUT / "Screenshots"
    screen_dir.mkdir(exist_ok=True)
    for path in ROOT.glob("*.png"):
        shutil.copy2(path, screen_dir / path.name)
    report = validate()
    report["asset_hashes_verified"] = len(manifest["assets"])
    (OUT / "VALIDATION.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
    files = [{"path": p.relative_to(OUT).as_posix(), "bytes": p.stat().st_size,
              "sha256": hashlib.sha256(p.read_bytes()).hexdigest()}
             for p in sorted(OUT.rglob("*")) if included(p) and p.name != "PACKAGE_MANIFEST.json"]
    (OUT / "PACKAGE_MANIFEST.json").write_text(json.dumps({"files": files}, indent=2) + "\n", encoding="utf-8")
    archive = ROOT.parent / "GenesisIdleRPG-UnityMigration.zip"
    with zipfile.ZipFile(archive, "w", zipfile.ZIP_DEFLATED) as z:
        for path in sorted(OUT.rglob("*")):
            if included(path):
                z.write(path, Path("UnityMigration") / path.relative_to(OUT))
    with zipfile.ZipFile(archive) as z:
        assert z.testzip() is None
    print(json.dumps(report, indent=2))
    print(f"Package: {archive} ({archive.stat().st_size:,} bytes; {len(files) + 1} files)")


if __name__ == "__main__":
    main()