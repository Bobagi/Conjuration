#!/usr/bin/env python3
"""Draws the three conjuration tomes, after Skyrim's Conjuration spell tome (UESP icon
SR-icon-book-SpellTomeConjuration): dark purple leather, a border of silver rings, the silver and violet
Conjuration school sign in the middle. A small gem on the spine tells the three apart in the inventory
(fire red, frost cyan, storm violet). Terraria style: 2x2 screen pixels per art pixel.

  python tools/make_tomes.py            writes art/tomes/*.png (1x) and Content/Items/*.png (2x)
"""
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent

PAL = {
    "K": (26, 14, 34),      # outline
    "D": (52, 26, 70),      # dark purple leather / spine
    "M": (84, 44, 108),     # leather
    "L": (112, 66, 138),    # leather light
    "s": (104, 100, 116),   # silver shade
    "S": (168, 166, 182),   # silver
    "W": (226, 224, 236),   # silver highlight
    "v": (150, 104, 190),   # violet of the sign
    "P": (228, 214, 178),   # page edges
    "p": (190, 172, 132),   # page shade
}
GEMS = {"Fire": (236, 92, 36), "Frost": (110, 220, 255), "Storm": (196, 160, 255)}

# 15 x 17 art pixels; 'G' is the element gem on the spine.
ART = """
.KKKKKKKKKKKKK.
KDSWSWSWSWSWSKK
KDWMMMMMMMMMWKP
KDSMLLLLLLLMSKP
KDWMWWWWWWWMWKP
KGSMMWvMvWMMSKP
KDWMMWMvvWMMWKP
KDSMMWMvMWMMSKP
KDWMMWvMvWMMWKP
KGSMMMWvWMMMSKP
KDWMMMMWMMMMWKP
KDSMLLLLLLLMSKP
KDWMLLLLLLLMWKP
KDSMMMMMMMMMSKp
KDWSWSWSWSWSWKp
KKKKKKKKKKKKKKp
.Kppppppppppppp
"""


def draw(gem):
    rows = ART.strip("\n").split("\n")
    assert all(len(r) == 15 for r in rows), [len(r) for r in rows]
    img = Image.new("RGBA", (15, len(rows)), (0, 0, 0, 0))
    px = img.load()
    for y, row in enumerate(rows):
        for x, ch in enumerate(row):
            if ch == ".":
                continue
            px[x, y] = (gem if ch == "G" else PAL[ch]) + (255,)
    return img


def main():
    (ROOT / "art/tomes").mkdir(parents=True, exist_ok=True)
    for name, gem in GEMS.items():
        art = draw(gem)
        art.save(ROOT / f"art/tomes/TomeConjure{name}Atronach.png")
        art.resize((art.width * 2, art.height * 2), Image.NEAREST).save(ROOT / f"Content/Items/TomeConjure{name}Atronach.png")
        print("wrote", name)


if __name__ == "__main__":
    main()
