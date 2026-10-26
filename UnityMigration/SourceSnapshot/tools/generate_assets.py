"""Rebuild Genesis's original starter assets using only Python's standard library."""
from pathlib import Path
import hashlib
import json
import math
import random
import struct
import wave
import zlib

ROOT = Path(__file__).resolve().parents[1] / "assets" / "generated"
MANIFEST = []


def register(relative, category):
    path = ROOT / relative
    MANIFEST.append({"path": "res://assets/generated/" + relative,
                     "category": category, "sha256": hashlib.sha256(path.read_bytes()).hexdigest()})


def svg(relative, body, size=128, category="icon"):
    path = ROOT / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(f'<svg xmlns="http://www.w3.org/2000/svg" width="{size}" height="{size}" viewBox="0 0 128 128">'
                    '<defs><radialGradient id="halo"><stop stop-color="#69ead5" stop-opacity=".32"/>'
                    '<stop offset="1" stop-color="#69ead5" stop-opacity="0"/></radialGradient>'
                    '<linearGradient id="gold" x2="1" y2="1"><stop stop-color="#fff4d0"/>'
                    '<stop offset="1" stop-color="#d5a858"/></linearGradient></defs>'
                    + body + '</svg>\n', encoding="utf-8")
    register(relative, category)


def icon(name, shape, color="#79e8d5"):
    svg("icons/" + name + ".svg", '<circle cx="64" cy="64" r="61" fill="url(#halo)"/>'
        '<circle cx="64" cy="64" r="53" fill="#111b32" stroke="#314c65"/>'
        f'<g fill="none" stroke="{color}" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">'
        + shape + '</g><circle cx="64" cy="9" r="3" fill="#f7dfa0"/>')


def png(relative, kind):
    size = 64
    raw = bytearray()
    for y in range(size):
        raw.append(0)
        for x in range(size):
            dx, dy = (x - 31.5) / 31.5, (y - 31.5) / 31.5
            r = math.hypot(dx, dy)
            if kind == "glow":
                a = max(0, 1 - r) ** 3
            elif kind == "ring":
                a = max(0, 1 - abs(r - .68) / .12) ** 2
            else:
                a = max(0, 1 - min(abs(dx), abs(dy)) * 15) * max(0, 1 - r) ** 2
            raw.extend((255, 255, 255, round(255 * a)))

    def chunk(tag, data):
        return struct.pack(">I", len(data)) + tag + data + struct.pack(">I", zlib.crc32(tag + data) & 0xffffffff)

    path = ROOT / relative
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", size, size, 8, 6, 0, 0, 0))
                     + chunk(b"IDAT", zlib.compress(bytes(raw))) + chunk(b"IEND", b""))
    register(relative, "particle")


def sound(name, notes, duration):
    path = ROOT / "audio" / (name + ".wav")
    path.parent.mkdir(parents=True, exist_ok=True)
    rate = 22050
    samples = bytearray()
    for i in range(round(duration * rate)):
        t = i / rate
        value = 0.0
        for index, frequency in enumerate(notes):
            local = t - index * duration * .14
            if local >= 0:
                envelope = min(1.0, local / .008) * math.exp(-local * 6 / duration)
                envelope *= min(1.0, (duration - t) / .025)
                value += (math.sin(math.tau * frequency * local)
                          + .18 * math.sin(math.tau * frequency * 2 * local)) * envelope
        samples.extend(struct.pack("<h", round(max(-1, min(1, value * .28 / len(notes))) * 32767)))
    with wave.open(str(path), "wb") as output:
        output.setparams((1, 2, rate, 0, "NONE", "not compressed"))
        output.writeframes(samples)
    register("audio/" + name + ".wav", "sound")


def fairy_art(index, color, emblem=""):
    """Original soft moe/chibi fairies: sprites, ribbon guardians, celestial girls."""
    hair = ["#e4dcff", "#8edbff", "#82ebc4", "#d4a9f5", "#ffc777", "#8ed5b0",
            "#a692e5", "#f1d9ef", "#93e6ed", "#efacd5", "#f6df9c", "#c4b9fa", "#f8e8cb"][index % 13]
    skin = ["#ffe4d2", "#efc3a4", "#bb8065", "#dfaa89"][index % 4]
    ink = "#65516f"
    family = index % 3
    iris = ["#71bba6", "#859ddd", "#c683b9", "#b79bdd", "#75b8d0"][index % 5]
    accent = ["#f391bd", "#9ce7ec", "#f6d57e", "#c6b1fa", "#a1e0b5"][index % 5]
    hair_shapes = [
        'M40 27Q27 35 33 62Q47 69 50 59L46 35 M87 27Q101 35 95 62Q82 69 78 59L82 35',
        'M43 27Q19 23 27 65Q30 79 20 88Q43 88 46 54 M84 27Q110 23 102 65Q99 79 109 88Q86 88 81 54',
        'M39 30Q15 38 28 61Q17 68 25 81Q42 85 45 55 M89 30Q113 38 100 61Q111 68 103 81Q86 85 83 55',
        'M40 36Q24 57 33 80L43 70L46 88L52 59 M85 36Q101 57 92 80L83 70L80 88L75 59',
        'M43 29Q20 12 19 40Q18 64 34 68Q23 52 35 41 M83 29Q110 12 110 40Q111 64 95 68Q106 52 94 41',
        'M43 26Q26 17 27 42L20 60Q36 63 43 48 M84 26Q102 17 102 42L110 60Q93 63 85 48',
    ]
    art = [f'<g id="celestial-chibi" data-art-direction="soft-moe" data-family="{family}">', '<circle cx="64" cy="64" r="61" fill="url(#halo)"/>']
    for side in (-1, 1):
        art.append(f'<path d="M{64+side*9} 69Q{64+side*55} 33 {64+side*51} 62Q{64+side*46} 76 {64+side*12} 78Q{64+side*46} 70 {64+side*41} 95Q{64+side*18} 97 {64+side*9} 78Z" fill="{accent}" fill-opacity=".38" stroke="{hair}" stroke-width="1.1"/>')
        art.append(f'<path d="M{64+side*12} 75L{64+side*43} 57M{64+side*12} 79L{64+side*33} 90" fill="none" stroke="#fff6ef" stroke-opacity=".7"/>')
    art += [f'<path d="M39 37Q35 9 64 10Q94 10 89 40L92 79Q76 87 69 65H55Q50 82 35 78Z" fill="{hair}" stroke="{ink}" stroke-width="1.2"/>',
            f'<path d="{hair_shapes[index % len(hair_shapes)]}" fill="{hair}" stroke="{ink}" stroke-width="1.2"/>',
            # Tiny legs and colored boots below a short, fully covered dress.
            f'<path d="M54 98L53 112H61L64 98M66 98L69 112H77L74 98" fill="{skin}" stroke="{ink}"/>',
            f'<path d="M53 108L61 109L61 118Q54 122 49 118ZM69 109L77 108L81 118Q76 122 69 118Z" fill="{color}" stroke="{ink}"/>',
            '<path d="M53 110L60 111M70 111L77 110" stroke="#fff5ee" stroke-width="2"/>',
            f'<path d="M53 66L43 78L34 {73 + index % 3 * 3} M75 66L85 78L94 69" fill="none" stroke="{skin}" stroke-width="6" stroke-linecap="round"/>',
            '<path d="M42 76L35 74M85 76L92 71" stroke="#fff8f2" stroke-width="6" stroke-linecap="round"/>',
            f'<path d="M53 64Q64 60 75 64L79 85H49Z" fill="#fff8f4" stroke="{ink}" stroke-width="1.2"/>',
            # Broad sailor-inspired collar and contrasting bow, original palette.
            f'<path id="celestial-collar" d="M51 64L64 74L77 64L73 62H55Z" fill="{color}" stroke="{ink}"/>',
            '<path d="M54 64L64 71L74 64" fill="none" stroke="#fff8ef" stroke-width="1"/>',
            f'<path d="M63 75L52 70L53 81L63 78L58 89L64 85L70 89L66 78L76 81L77 70L65 75Z" fill="{accent}" stroke="{ink}" stroke-width=".8"/>',
            '<circle cx="64" cy="76" r="3.1" fill="url(#gold)"/>',
            f'<path id="pleated-skirt" d="M49 84L40 100Q64 111 88 100L79 84Z" fill="{color}" stroke="{ink}" stroke-width="1.2"/>',
            f'<path d="M53 86L49 101M60 87L58 104M68 87L70 104M75 86L80 101" stroke="{ink}" stroke-opacity=".4" fill="none"/>',
            '<path d="M42 99Q64 108 86 99M50 84H78" fill="none" stroke="#fff5ea" stroke-width="2"/>',
            f'<path d="M59 56H69V65Q64 69 59 65Z" fill="{skin}"/>',
            # Oversized round head, huge glossy eyes, small mouth.
            f'<path d="M39 34Q38 15 64 15Q90 15 89 35L86 47Q84 63 64 65Q43 63 41 48Z" fill="{skin}" stroke="{ink}" stroke-width="1"/>',
            f'<path d="M38 36Q34 9 64 10Q96 11 90 37L80 29L76 37L65 23L57 34L51 27L40 40Z" fill="{hair}" stroke="{ink}" stroke-width="1.1"/>',
            '<path d="M45 40Q51 35 58 39M70 39Q77 35 83 40" fill="none" stroke="#392b50" stroke-width="2" stroke-linecap="round"/>']
    # Outfit overlays replace the uniform silhouette for two thirds of the cast.
    costume = []
    if family == 0:
        costume = [f'<g id="sprite-capelet"><path d="M52 64Q64 70 76 64L81 73Q73 77 64 72Q55 77 47 73Z" fill="{color}" stroke="{ink}"/>',
                   '<path d="M51 68Q64 76 77 68" fill="none" stroke="#fff9f4" stroke-width="2"/>',
                   f'<path d="M51 83Q64 87 77 83L86 100Q64 112 42 100Z" fill="{color}" stroke="{ink}"/>',
                   f'<path d="M54 86L51 100Q64 108 77 100L74 86Z" fill="{accent}" fill-opacity=".65"/>',
                   '<path d="M43 99Q46 105 49 101Q52 107 55 103Q59 109 63 105Q67 109 71 104Q76 108 79 102Q83 105 85 100" fill="none" stroke="#fff8ef" stroke-width="2"/>',
                   f'<path d="M59 73L64 76L69 73L68 81L64 79L60 81Z" fill="{accent}" stroke="{ink}" stroke-width=".7"/></g>']
    elif family == 1:
        costume = [f'<g id="ribbon-guardian"><path d="M52 64L64 69L76 64L79 85H49Z" fill="{color}" stroke="{ink}"/>',
                   '<path d="M52 64Q55 70 58 67Q61 74 64 69Q67 74 70 67Q73 70 76 64" fill="none" stroke="#fff9f6" stroke-width="3"/>',
                   f'<path d="M49 84L40 100Q64 114 88 100L79 84Z" fill="{color}" stroke="{ink}"/>',
                   '<path d="M56 81H72L77 100Q64 107 51 100Z" fill="#fff9f6" stroke="#d7cada" stroke-width=".7"/>',
                   '<path d="M43 99Q47 107 51 102Q55 110 59 105Q64 113 69 105Q73 110 77 102Q82 107 85 99" fill="none" stroke="#fff9f6" stroke-width="3"/>',
                   f'<path d="M63 73L55 70L55 79L63 76L60 84L65 80L69 84L66 76L74 79L74 70L65 73Z" fill="{accent}" stroke="{ink}" stroke-width=".7"/>',
                   '<circle cx="64" cy="74" r="2" fill="#fff1bf"/></g>']
    # Insert overlays behind the neck and face, ahead of the skirt base.
    face_start = next(i for i, part in enumerate(art) if 'M59 56H69' in part)
    art[face_start:face_start] = costume
    for x in (52, 76):
        art += [f'<ellipse cx="{x}" cy="45" rx="5.7" ry="7.6" fill="white"/>',
                f'<ellipse cx="{x}" cy="46" rx="4.6" ry="6.7" fill="{iris}" stroke="{ink}" stroke-width=".7"/>',
                f'<ellipse cx="{x}" cy="46" rx="2.6" ry="4.8" fill="#322541"/>',
                f'<circle cx="{x-1.8}" cy="42.7" r="2.1" fill="white"/><circle cx="{x+1.5}" cy="48.5" r="1" fill="white"/>']
    art += ['<ellipse cx="45" cy="53" rx="4" ry="1.8" fill="#ed8eaa" fill-opacity=".55"/><ellipse cx="83" cy="53" rx="4" ry="1.8" fill="#ed8eaa" fill-opacity=".55"/>',
            '<path d="M61 55Q64 58 67 55" fill="none" stroke="#a35370" stroke-width="1.1" stroke-linecap="round"/>',
            # A forehead constellation tiara, not a copied costume or hairstyle.
            '<path id="constellation-tiara" d="M45 29L57 30L64 26L71 30L83 29" fill="none" stroke="#ffe3a2" stroke-width="2.3"/>',
            f'<path d="M64 25L68 30L64 35L60 30Z" fill="{accent}" stroke="#fff1bd"/>',
            f'<path d="M34 25L39 29L34 33L29 29ZM94 25L99 29L94 33L89 29Z" fill="{accent}" stroke="#fff1bd"/>',
            '<path d="M96 49V87" stroke="#ffe4a2" stroke-width="2"/>',
            f'<g transform="translate(84 28) scale(.19)" fill="none" stroke="{accent}" stroke-width="6" stroke-linecap="round">{emblem}</g>',
            '<path d="M15 27V35M11 31H19M107 96V104M103 100H111" stroke="#fff0c9" stroke-width="1.2"/>', '</g>']
    art.pop()  # close the character group after the finishing details
    art += ['<path d="M46 40L44 38M82 40L84 38" stroke="#57415f" stroke-width="1.2" stroke-linecap="round"/>',
            '<path d="M46 22Q52 17 58 18M69 17Q78 17 82 23" fill="none" stroke="#fff9ff" stroke-opacity=".5" stroke-width="2.2" stroke-linecap="round"/>']
    if family == 1:
        art += [f'<path id="soft-side-fringe" d="M44 25Q60 14 77 23Q68 40 65 51L57 54Q59 38 52 32L43 38Z" fill="{hair}" stroke="{ink}" stroke-width=".8"/>',
                '<path d="M58 26Q67 24 70 26" fill="none" stroke="#fffaff" stroke-opacity=".55" stroke-width="2"/>',
                f'<path d="M80 24L86 20L87 27L93 25L90 32L83 30Z" fill="{accent}" stroke="#fff8f3"/>',
                '<path d="M84 21L90 30M89 22L82 29" stroke="#fff9f5" stroke-width="1.5"/>']
    elif family == 0:
        art += [f'<g id="petal-hair-clips" fill="{accent}" stroke="#fff4db" stroke-width=".8"><circle cx="39" cy="29" r="3"/><circle cx="35" cy="26" r="2.5"/><circle cx="35" cy="32" r="2.5"/><circle cx="43" cy="26" r="2.5"/><circle cx="43" cy="32" r="2.5"/></g>']
    if index % 4 == 0:
        art += [f'<path d="M59 54Q64 60 69 54Q68 62 64 62Q60 62 59 54Z" fill="#b96783" stroke="{ink}" stroke-width=".6"/>',
                '<path d="M62 60Q64 58 66 60" stroke="#f1b0c4" stroke-width="1.2"/>']
    art.append('</g>')
    return "".join(art)


def main():
    shapes = {
        "hope_fluctuation": '<circle cx="64" cy="64" r="5" fill="#ffffff"/><path d="M35 72 Q50 40 64 64 Q80 88 94 56 M46 34L49 39 M85 87L89 94"/>',
        "omniverse": '<ellipse cx="48" cy="64" rx="29" ry="18" transform="rotate(35 48 64)"/><ellipse cx="80" cy="64" rx="29" ry="18" transform="rotate(-35 80 64)"/><circle cx="64" cy="64" r="10"/>',
        "reality": '<path d="M64 20L105 64L64 108L23 64Z M64 35L91 64L64 93L37 64Z M64 20V108 M23 64H105"/>',
        "quark_filament": '<path d="M32 81 Q64 12 96 81 M32 47 Q64 116 96 47"/><circle cx="64" cy="64" r="9"/>',
        "hadron_forge": '<ellipse cx="64" cy="64" rx="39" ry="16"/><ellipse cx="64" cy="64" rx="39" ry="16" transform="rotate(60 64 64)"/><ellipse cx="64" cy="64" rx="39" ry="16" transform="rotate(120 64 64)"/><circle cx="64" cy="64" r="6" fill="#fff3ce"/>',
        "cosmic_dust": '<path d="M29 76 Q10 52 40 48 Q45 21 69 38 Q98 24 98 53 Q119 76 91 84 Q62 104 29 76Z"/><path d="M43 65 L47 57 M66 77 L70 69 M82 53 L86 45"/>',
        "protostar_core": '<circle cx="64" cy="64" r="23"/><path d="M64 20V32 M64 96V108 M20 64H32 M96 64H108 M33 33L41 41 M87 87L95 95 M33 95L41 87 M87 41L95 33"/>',
        "gaia_biosphere": '<circle cx="64" cy="64" r="34"/><path d="M37 45 L54 40 L61 52 L49 64 L55 82 L44 89 M79 34 L71 49 L87 58 L80 73 L91 83"/><path d="M41 99 Q74 111 104 76"/>',
        "black_hole_anchor": '<circle cx="64" cy="64" r="19"/><ellipse cx="64" cy="64" rx="44" ry="13" transform="rotate(-25 64 64)"/><path d="M36 42 Q70 10 93 48 M35 80 Q67 113 92 85"/>',
        "white_hole_fountain": '<circle cx="64" cy="64" r="13" fill="#fff4d0"/><path d="M64 44V23 M64 84V105 M44 64H23 M84 64H105 M50 50L35 35 M78 78L93 93 M50 78L35 93 M78 50L93 35"/>',
        "wormhole_nexus": '<ellipse cx="37" cy="64" rx="13" ry="30"/><ellipse cx="91" cy="64" rx="13" ry="30"/><path d="M37 34 Q64 53 91 34 M37 94 Q64 75 91 94 M37 64H91"/>',
        "parallel_universe": '<circle cx="46" cy="64" r="27"/><circle cx="82" cy="64" r="27"/><path d="M64 26V102"/>',
        "multiverse_loom": '<path d="M64 23 L102 45 V83 L64 105 L26 83 V45Z M26 45L64 67L102 45 M64 67V105 M64 23V67 M26 83L64 67L102 83"/>',
        "relic_chronos": '<path d="M43 28H85 M43 100H85 M47 30Q45 54 64 64Q83 74 81 98 M81 30Q83 54 64 64Q45 74 47 98 M52 91L64 79L76 91Z"/>',
        "relic_seraph_grace": '<path d="M31 47L44 79H84L97 47L76 60L64 35L52 60Z M44 91H84"/><circle cx="64" cy="74" r="4"/>',
        "relic_wormhole_loom": '<path d="M35 30V98 M93 30V98 M35 41H93 M35 87H93 M43 41L85 87 M58 41L70 87 M73 41L55 87 M85 41L43 87"/>',
        "genesis_burst": '<path d="M64 24L72 53L101 64L72 75L64 104L56 75L27 64L56 53Z"/>',
        "chrono_harmony": '<circle cx="64" cy="64" r="33"/><path d="M64 42V64L79 74 M87 29L98 40 M91 24L103 36 M28 87L38 97"/>',
    }
    palette = ["#aaa1e9", "#62cbea", "#55bda2", "#b98bde", "#e9ad52", "#69b989",
               "#9272c5", "#dca9df", "#57c1d1", "#dc8cba", "#daba65", "#a398e0", "#e8c991"]
    motifs = {
        "halo": '<ellipse cx="64" cy="25" rx="24" ry="9"/><path d="M61 100V52Q33 26 18 46Q21 78 54 82M67 100V52Q95 26 110 46Q107 78 74 82M26 49L51 68M102 49L77 68"/>',
        "horns": '<path d="M41 59Q16 48 29 18Q31 43 50 41M87 59Q112 48 99 18Q97 43 78 41M43 53L64 43L85 53L90 83L64 107L38 83ZM48 70L57 75M80 70L71 75M54 89Q64 96 74 89"/>',
        "crown": '<path d="M26 40L44 59L64 23L84 59L102 40L92 94H36ZM36 83H92M64 45V72M52 83L64 71L76 83"/><circle cx="26" cy="36" r="4"/><circle cx="64" cy="19" r="4"/><circle cx="102" cy="36" r="4"/>',
        "spark": shapes["genesis_burst"], "sun": shapes["protostar_core"],
        "atom": shapes["hadron_forge"], "web": shapes["multiverse_loom"],
        "moon": '<path d="M84 24A43 43 0 1 0 95 91A36 36 0 0 1 84 24Z"/>',
        "balance": '<circle cx="64" cy="64" r="39"/><path d="M64 25A20 20 0 0 1 64 64A20 20 0 0 0 64 103"/><circle cx="64" cy="44" r="5"/><circle cx="64" cy="84" r="5"/>',
        "leaf": '<path d="M30 99Q10 25 99 26Q103 101 30 99ZM30 99L86 39M53 76L49 49M68 60L89 64"/>',
        "flame": '<path d="M64 18Q91 43 82 60Q107 84 78 103Q36 118 29 83Q25 60 46 43Q42 70 59 66Q72 54 64 18Z"/>',
        "water": '<path d="M64 18Q21 71 35 91Q64 119 93 91Q106 71 64 18ZM45 79Q43 91 59 96"/>',
        "crystal": '<path d="M64 17L101 56L83 100H45L27 56ZM27 56H101M64 17L48 56L64 109L81 56Z"/>',
        "flower": '<circle cx="64" cy="64" r="11"/><ellipse cx="64" cy="35" rx="13" ry="19"/><ellipse cx="64" cy="93" rx="13" ry="19"/><ellipse cx="35" cy="64" rx="19" ry="13"/><ellipse cx="93" cy="64" rx="19" ry="13"/>',
        "clock": shapes["chrono_harmony"],
        "confluence": '<ellipse cx="53" cy="64" rx="34" ry="22" transform="rotate(-35 53 64)"/><ellipse cx="75" cy="64" rx="34" ry="22" transform="rotate(35 75 64)"/><circle cx="44" cy="55" r="6"/><circle cx="84" cy="55" r="6"/><circle cx="64" cy="80" r="6"/><path d="M44 55L64 80L84 55M64 18V31M58 24H70"/>',
        "eye": '<path d="M16 64Q64 9 112 64Q64 119 16 64Z"/><circle cx="64" cy="64" r="18"/><circle cx="64" cy="64" r="6"/><path d="M64 13V25M30 25L37 35M98 25L91 35"/>',
        "infinity": '<path d="M64 64C15 5 2 115 49 83L79 45C126 13 113 123 64 64Z"/><circle cx="64" cy="64" r="8"/>',
        "heart": '<path d="M64 105Q7 69 25 37Q44 12 64 40Q84 12 103 37Q121 69 64 105Z"/><path d="M64 50V84M47 67H81"/>',
        "seed": '<path d="M64 108V65M64 80Q20 83 25 41Q68 39 64 80ZM64 65Q62 22 102 24Q105 67 64 65Z"/><path d="M33 49L64 80M94 32L64 65"/>',
        "wind": '<path d="M23 46H83Q112 22 89 20 M20 64H101Q118 78 96 87 M29 83H67Q88 111 102 97"/>',
    }
    catalog = json.loads((ROOT.parents[1] / "data" / "creation_stages.json").read_text(encoding="utf-8"))
    for index, row in enumerate(catalog):
        name, _, _, _, _, color, motif = row[:7]
        svg(f"icons/{name}.svg", fairy_art(index, "#" + color, motifs[motif]), 384, "fairy")
    for index, name in enumerate(("relic_chronos", "relic_seraph_grace", "relic_wormhole_loom", "genesis_burst", "chrono_harmony")):
        # Keep shared character designs stable when discoveries are appended.
        svg(f"icons/{name}.svg", fairy_art(index + 72, palette[index], shapes[name]), 384, "fairy")
    colors = ["#91bfff", "#72f0d0", "#ffce70", "#ba8bff", "#ff9dce", "#fff0b0"]
    for stage, color in enumerate(colors):
        svg(f"portraits/zenda_{stage}.svg", fairy_art(stage + 7, color, shapes["genesis_burst"]), 512, "portrait")
    rng = random.Random(217)
    clouds = '<rect width="128" height="128" fill="#080d20"/>'
    for _ in range(32):
        x, y, radius = rng.randrange(128), rng.randrange(128), rng.randrange(12, 48)
        clouds += f'<ellipse cx="{x}" cy="{y}" rx="{radius}" ry="{radius//2}" fill="url(#halo)" opacity=".16"/>'
    svg("backgrounds/nebula.svg", clouds, 1024, "background")
    for kind in ("glow", "sparkle", "ring"):
        png(f"particles/{kind}.png", kind)
    for name, notes, duration in [("tap", [660, 990], .12), ("purchase", [440, 660, 880], .32),
                                   ("chapter", [523.25, 659.25, 783.99, 1046.5], .85),
                                   ("evolution", [392, 523.25, 659.25, 783.99, 1046.5], 1.4),
                                   ("miracle", [587.33, 880, 1174.66], .65)]:
        sound(name, notes, duration)
    (ROOT / "manifest.json").write_text(json.dumps({"source": "Original procedural assets generated by tools/generate_assets.py; no external asset files or samples.",
        "generator": "Python standard library", "assets": MANIFEST}, indent=2) + "\n", encoding="utf-8")
    print(f"Generated {len(MANIFEST)} assets in {ROOT}")


if __name__ == "__main__":
    main()