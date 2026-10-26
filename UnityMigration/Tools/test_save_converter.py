"""Self-contained converter tests; does not discover or open live player files."""
import copy
import json
from pathlib import Path
import unittest

from convert_save import convert

ROOT = Path(__file__).resolve().parents[1]


class SaveMigrationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.catalog = json.loads((ROOT / "Data/catalog.json").read_text(encoding="utf-8"))

    def test_old_save_defaults(self):
        result = convert({}, self.catalog)
        self.assertEqual(result["equipped_appearance"], "ivory_queen")
        self.assertEqual(len(result["counts"]), 106)
        self.assertEqual(len(result["relic_levels"]), 3)
        self.assertTrue(all(v["value"] == 0 for v in result["counts"]))

    def test_claim_unlock_and_original_unchanged(self):
        saved = {"completed_chapters": ["first_life", "unknown"], "equipped_appearance": "earth_bloom",
                 "counts": {"gaia_biosphere": 3}, "timestamp": 1234567890}
        before = copy.deepcopy(saved)
        result = convert(saved, self.catalog)
        self.assertEqual(saved, before)
        self.assertEqual(result["equipped_appearance"], "earth_bloom")
        self.assertEqual(result["completed_chapters"], ["first_life"])
        self.assertEqual(result["timestamp"], 1234567890)
        self.assertEqual(result["quanta"], 0)

    def test_ownership_does_not_unlock(self):
        result = convert({"counts": {"angel_realm": 1}, "equipped_appearance": "angel_dawn"}, self.catalog)
        self.assertEqual(result["equipped_appearance"], "ivory_queen")

    def test_invalid_values(self):
        result = convert({"quanta": float("nan"), "genesis_seeds": -1, "goddess_stage": 99,
                          "counts": {"hope_fluctuation": -3},
                          "relic_levels": {"relic_wormhole_loom": 999}}, self.catalog)
        self.assertEqual(result["quanta"], 0)
        self.assertEqual(result["genesis_seeds"], 0)
        self.assertEqual(result["goddess_stage"], 5)
        self.assertEqual(result["counts"][0]["value"], 0)
        self.assertEqual(result["relic_levels"][2]["value"], 10)
        json.dumps(result, allow_nan=False)

    def test_every_demo_snapshot(self):
        snapshots = json.loads((ROOT / "Data/demo_snapshots.json").read_text(encoding="utf-8"))["snapshots"]
        for snapshot in snapshots:
            saved = snapshot["state"]
            result = convert(saved, self.catalog)
            self.assertEqual({v["id"]: v["value"] for v in result["counts"]}, saved["counts"])
            self.assertEqual(result["completed_chapters"], saved["completed_chapters"])
            self.assertEqual(result["quanta"], saved["quanta"])

    def test_non_object(self):
        with self.assertRaises(ValueError):
            convert([], self.catalog)


if __name__ == "__main__":
    unittest.main(verbosity=2)