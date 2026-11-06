"""Presentation regression checks; run with the existing MCP Python environment."""
import hashlib
import importlib.util
import json
from pathlib import Path
import struct
import sys
import unittest
import xml.etree.ElementTree as ET

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'Tools/Migration'))
spec = importlib.util.spec_from_file_location('refine', ROOT / 'Tools/Migration/Refine-Fairies.py')
refine = importlib.util.module_from_spec(spec)
spec.loader.exec_module(refine)
from FantasyLadies import CHILD_BODY_TRANSFORM, CHILD_FACE_TRANSFORM, WING_SCALE, WING_STYLES
from FairyClothes import OUTFIT_CATEGORIES, OUTFIT_STYLES
from FairyHalos import HALO_DESIGN, HALO_STYLES, HALO_TEXTURE_SIZE, halo_icon, halo_style


class FairyRefinement(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.catalog = json.loads(refine.CATALOG.read_text(encoding='utf-8-sig'))
        cls.before = json.loads((refine.BACKUP / 'catalog.json').read_text(encoding='utf-8-sig'))
        cls.manifest = json.loads((refine.OUTPUT / 'manifest.json').read_text())

    def test_gameplay_and_original_stories_unchanged(self):
        self.assertEqual(refine.signature(self.before), refine.signature(self.catalog))

    def test_names_unique_and_all_legacy_residents_resolve(self):
        names = [f['name'] for g in self.catalog['generators'] for f in g['resident_friends']]
        self.assertEqual(len(names), len(set(names)))
        current = {g['id']: g for g in self.catalog['generators']}
        for old in self.before['generators']:
            if old.get('recruit_after'):
                new = current[old['id']]
                self.assertEqual(self.catalog['fairy_name_aliases'].get(old['fairy'], old['fairy']), new['fairy'])
                self.assertEqual(new['fairy'], self.catalog['fairy_names'][new['id']])
                self.assertEqual(new['resident_friends'][0]['name'], new['fairy'])

    def test_portraits_resolution_integrity_and_diversity(self):
        portraits = self.manifest['portraits']
        self.assertEqual(len(portraits), len(self.catalog['generators']))
        self.assertEqual(len({p['sha256'] for p in portraits}), len(portraits))
        for p in portraits:
            data = (refine.ICONS / (p['id'] + '.png')).read_bytes()
            self.assertEqual(struct.unpack('>II', data[16:24]), (512, 512))
            self.assertEqual(hashlib.sha256(data).hexdigest(), p['sha256'])
            ET.parse(refine.OUTPUT / (p['id'] + '.svg'))
        for trait, count in [('hair', 9), ('outfit', len(OUTFIT_STYLES)), ('wings', 10), ('accessory', 7), ('prop', 7), ('skin', 6)]:
            self.assertEqual(len({p['traits'][trait] for p in portraits}), count)

    def test_everyday_sport_dress_and_fantasy_wardrobe(self):
        portraits = self.manifest['portraits']
        self.assertEqual({p['traits']['outfit_style'] for p in portraits}, set(OUTFIT_STYLES))
        self.assertEqual({p['traits']['outfit_category'] for p in portraits}, set(OUTFIT_CATEGORIES))
        ns = {'svg': 'http://www.w3.org/2000/svg'}
        for p in portraits:
            traits = p['traits']
            index = traits['outfit']
            self.assertEqual(traits['outfit_style'], OUTFIT_STYLES[index])
            self.assertEqual(traits['outfit_category'], OUTFIT_CATEGORIES[index])
            root = ET.parse(refine.OUTPUT / (p['id'] + '.svg')).getroot()
            outfit = root.find(".//svg:g[@id='fairy-outfit']", ns)
            self.assertIsNotNone(outfit)
            self.assertEqual(outfit.get('data-style'), traits['outfit_style'])
            self.assertEqual(outfit.get('data-category'), traits['outfit_category'])
            self.assertGreaterEqual(len(outfit.findall('svg:path', ns)), 8)

    def test_child_anime_direction_and_medium_abstract_wings(self):
        self.assertEqual({p['traits']['wing_style'] for p in self.manifest['portraits']}, set(WING_STYLES))
        ns = {'svg': 'http://www.w3.org/2000/svg'}
        for p in self.manifest['portraits']:
            traits = p['traits']
            self.assertEqual(traits['direction'], 'child-anime-fairy')
            self.assertEqual(traits['proportions'], 'child-storybook-anime')
            self.assertEqual(traits['wing_scale'], list(WING_SCALE))
            self.assertTrue(.7 <= min(traits['wing_scale']) <= max(traits['wing_scale']) <= .9)
            root = ET.parse(refine.OUTPUT / (p['id'] + '.svg')).getroot()
            face = root.find(".//svg:g[@id='anime-face']", ns)
            self.assertEqual(face.get('data-age-style'), 'child')
            self.assertEqual(face.get('transform'), CHILD_FACE_TRANSFORM)
            self.assertEqual(traits['face_transform'], CHILD_FACE_TRANSFORM)
            body = root.find(".//svg:g[@id='child-body']", ns)
            self.assertEqual(body.get('transform'), CHILD_BODY_TRANSFORM)
            self.assertEqual(traits['body_transform'], CHILD_BODY_TRANSFORM)
            self.assertIsNotNone(body.find("svg:g[@id='fairy-outfit']", ns))
            props = root.find(".//svg:g[@id='child-props']", ns)
            self.assertEqual(props.get('transform'), CHILD_BODY_TRANSFORM)
            wings = root.find(".//svg:g[@id='medium-wings']", ns)
            self.assertIsNotNone(wings)
            self.assertEqual(wings.get('data-style'), traits['wing_style'])
            halves = wings.findall('svg:g/svg:g', ns)
            self.assertEqual(len(halves), 2)
            for half in halves:
                self.assertEqual(half.get('transform'),
                                 f'translate(59 64) scale({WING_SCALE[0]} {WING_SCALE[1]}) translate(-59 -64)')

    def test_every_fairy_has_a_halo_and_runtime_mapping(self):
        ns = {'svg': 'http://www.w3.org/2000/svg'}
        data = json.loads((ROOT / 'Assets/Genesis/Resources/Genesis/Data/fairy-halos.json').read_text())
        mapping = {f['id']: f['style'] for f in data['fairies']}
        self.assertEqual(data['styles'], list(HALO_STYLES))
        self.assertEqual(set(mapping), {g['id'] for g in self.catalog['generators']})
        self.assertEqual(set(mapping.values()), set(HALO_STYLES))
        for g in self.catalog['generators']:
            self.assertEqual(mapping[g['id']], halo_style(g))
            root = ET.parse(refine.OUTPUT / (g['id'] + '.svg')).getroot()
            halos = root.findall(".//svg:g[@id='fairy-halo']", ns)
            self.assertEqual(len(halos), 1)
            self.assertEqual(halos[0].get('data-style'), mapping[g['id']])
        for style in HALO_STYLES:
            data = (ROOT / 'Assets/Genesis/Resources/Genesis/Art/halos' / (style + '.png')).read_bytes()
            self.assertEqual(struct.unpack('>II', data[16:24]), HALO_TEXTURE_SIZE)

    def test_hd_anime_halo_layers_and_unique_gradients(self):
        ns = {'svg': 'http://www.w3.org/2000/svg'}
        for style in HALO_STYLES:
            root = ET.fromstring(halo_icon(style))
            halo = root.find("svg:g[@id='fairy-halo']", ns)
            self.assertEqual(halo.get('data-design'), HALO_DESIGN)
            for layer in ('bands', 'sigils', 'ornaments', 'jewels', 'sparkles'):
                self.assertIsNotNone(halo.find(f"svg:g[@id='halo_{style}_{layer}']", ns))
            self.assertGreaterEqual(len(list(halo.iter())), 45)
            ids = [node.get('id') for node in halo.iter() if node.get('id')]
            self.assertEqual(len(ids), len(set(ids)))
        hashes = {hashlib.sha256((ROOT / 'Assets/Genesis/Resources/Genesis/Art/halos' / (s + '.png')).read_bytes()).hexdigest()
                  for s in HALO_STYLES}
        self.assertEqual(len(hashes), len(HALO_STYLES))

    def test_halo_button_uses_enchanted_socket(self):
        source = (ROOT / 'Assets/Genesis/Sanctum/GenesisSanctum.Halos.cs').read_text()
        button = source[source.index('private void DrawHaloCollectionButton()'):source.index('private Texture2D HaloTexture')]
        self.assertIn('EnsureEnchantedGameTheme();', button)
        self.assertIn('DrawEnchantedSocket(rect, haloPicker);', button)
        self.assertNotIn('glassDisc', button)
        self.assertNotIn('glassRing', button)
        kit = Path(r'E:\ZendaGame\Enchanted_Unity_UI_Kit\Assets\EnchantedUI\Art\hex_portrait_socket.png')
        imported = ROOT / 'Assets/EnchantedUI/Art/hex_portrait_socket.png'
        self.assertEqual(hashlib.sha256(imported.read_bytes()).hexdigest(), hashlib.sha256(kit.read_bytes()).hexdigest())

    def test_renderer_deterministic(self):
        for generator in self.catalog['generators']:
            content, _ = refine.portrait(generator)
            self.assertEqual(content, (refine.OUTPUT / (generator['id'] + '.svg')).read_text())

    def test_child_restyle_preserves_character_designs_and_gameplay(self):
        previous = json.loads((refine.OUTPUT / 'before-child-style.json').read_text())
        by_id = {p['id']: p for p in previous['portraits']}
        self.assertEqual(set(by_id), {p['id'] for p in self.manifest['portraits']})
        for p in self.manifest['portraits']:
            old = by_id[p['id']]
            self.assertEqual(p['name'], old['name'])
            for trait, value in old['traits'].items():
                if trait not in ('direction', 'proportions'):
                    self.assertEqual(p['traits'][trait], value, f"{p['id']}: {trait}")
            self.assertNotEqual(p['sha256'], old['sha256'])
        self.assertEqual(hashlib.sha256(refine.CATALOG.read_bytes()).hexdigest(), previous['catalog_sha256'])
        mapping = ROOT / 'Assets/Genesis/Resources/Genesis/Data/fairy-halos.json'
        self.assertEqual(hashlib.sha256(mapping.read_bytes()).hexdigest(), previous['halo_mapping_sha256'])

    def test_roster_migration_idempotent(self):
        copy = json.loads(json.dumps(self.catalog))
        refine.replace_residents(copy)
        self.assertEqual(copy, self.catalog)


if __name__ == '__main__':
    unittest.main(verbosity=2)