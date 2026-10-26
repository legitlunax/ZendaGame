"""Convert a user-selected Godot save to list-based Unity-friendly JSON; no automatic save discovery."""
import argparse
import json
import math
from pathlib import Path


def finite_number(value, default=0.0):
    try:
        number = float(value)
        return max(0.0, number) if math.isfinite(number) else default
    except (ValueError, TypeError):
        return default


def convert(saved, catalog):
    if not isinstance(saved, dict):
        raise ValueError("Save root must be an object")
    chapters = saved.get("completed_chapters", [])
    if not isinstance(chapters, list):
        chapters = []
    completed = [c["id"] for c in catalog["chapters"] if c["id"] in chapters]
    counts = saved.get("counts", {})
    relics = saved.get("relic_levels", {})
    if not isinstance(counts, dict):
        counts = {}
    if not isinstance(relics, dict):
        relics = {}
    unlocked = [d["id"] for d in catalog["wardrobe"] if not d["chapter"] or d["chapter"] in completed]
    equipped = saved.get("equipped_appearance", "ivory_queen")
    if equipped not in unlocked:
        equipped = "ivory_queen"
    result = {"schema_version": 1, "source_format": "godot_genesis_save_v1",
              "equipped_appearance": equipped, "completed_chapters": completed,
              "counts": [{"id": g["id"], "value": int(finite_number(counts.get(g["id"], 0)))} for g in catalog["generators"]],
              "relic_levels": [{"id": r["id"], "value": min(int(finite_number(relics.get(r["id"], 0))), r.get("max_lvl", 1000000))} for r in catalog["relics"]],
              "genesis_seeds": int(finite_number(saved.get("genesis_seeds", 0))),
              "goddess_stage": min(len(catalog["goddess_forms"]) - 1, int(finite_number(saved.get("goddess_stage", 0))))}
    for key in ("quanta", "lifetime_quanta", "current_run_quanta", "timestamp"):
        result[key] = finite_number(saved.get(key, 0))
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("source", type=Path)
    parser.add_argument("destination", type=Path)
    parser.add_argument("--catalog", type=Path, default=Path(__file__).resolve().parents[1] / "Data/catalog.json")
    args = parser.parse_args()
    if args.source.resolve() == args.destination.resolve():
        parser.error("Choose a different destination; original saves are never overwritten")
    saved = json.loads(args.source.read_text(encoding="utf-8-sig"))
    catalog = json.loads(args.catalog.read_text(encoding="utf-8"))
    result = convert(saved, catalog)
    with args.destination.open("x", encoding="utf-8") as file:
        json.dump(result, file, indent=2, allow_nan=False)
        file.write("\n")
    print(f"Converted to {args.destination}; source unchanged. Offline income has NOT been applied.")


if __name__ == "__main__":
    main()