#!/usr/bin/env python3
"""Draws the Flame Atronach head piece (Content/Items/AtronachCrown_Head.png) and its item icon.

Reference: the Skyrim Flame Atronach (UESP: "female humanoids engulfed in flame, wearing intricate, black,
metal armor"): black mask with glowing eyes, ram horns curling back, a plume of fire on the head, spiked
black pauldrons, black chest plates and a black hip plate over a body of fire.

The sheet is player format: 20 frames of 40x56, facing right, drawn in Terraria's 2x2 art pixels. It is
worn by the minion's dummy player (with ArmorIDs.Head.Sets.DrawHead = false), so it is drawn over the body
and moves with it. Coordinates below are art pixels on the 20x28 grid of frame 0, measured on the body
template exported with /atronachtest template (head at rows 6-12, cols 6-13, shoulders from row 13).
In walk frames 7-9 and 14-16 the body is 1 art pixel higher (Main.OffsetsPlayerHeadgear), so is the art.

  python tools/make_atronach_crown.py [--template <atronach_template.png> --preview out.png]
"""
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent

C = {
    "K": (18, 8, 8),         # outline / deepest black
    "D": (38, 20, 20),       # black armor
    "M": (62, 36, 32),       # armor mid tone
    "H": (104, 56, 44),      # armor rim light (lit by its own fire)
    "c": (255, 104, 28),     # lava crack in the armor
    "E": (255, 248, 210),    # eye glow
    "w": (255, 250, 214),    # white hot flame
    "y": (255, 214, 86),     # yellow flame
    "o": (255, 146, 36),     # orange flame
    "r": (214, 64, 18),      # red flame edge
    "l": (255, 196, 92),     # bright streak on the fire body
}

# 20 x 28 art pixel map of frame 0 ('.' = nothing drawn, the body shows through).
ART = """
....................
..........r.........
.........ryr........
....KKK..ywy........
...KMDDKoywyo.KK....
...KD.KKryyrKKDK....
...KD...KyyK..K.....
....KK.KooyKK.......
......KoyyKDDK......
......KoyyKDEEK.....
......KryoKDDDK.....
......KroyKDDK......
....K..KrrKDK.K.....
...KDK..ryyo.KDK....
....KK.......KK.....
....................
...........KD.......
...........DK.......
..........l.........
.........ll.........
....................
.........KDDK.......
..........KK........
....................
....................
....................
....................
....................
"""

RAISED = {7, 8, 9, 14, 15, 16}   # walk frames where the body sits 1 art pixel higher


def art_rows():
    rows = ART.strip("\n").split("\n")
    assert len(rows) == 28 and all(len(r) == 20 for r in rows), [len(r) for r in rows]
    return rows


def frame(dy):
    img = Image.new("RGBA", (40, 56), (0, 0, 0, 0))
    px = img.load()
    for y, row in enumerate(art_rows()):
        for x, ch in enumerate(row):
            if ch == ".":
                continue
            yy = y + dy
            if 0 <= yy < 28:
                for sx in range(2):
                    for sy in range(2):
                        px[x * 2 + sx, yy * 2 + sy] = C[ch] + (255,)
    return img


def sheet():
    out = Image.new("RGBA", (40, 56 * 20), (0, 0, 0, 0))
    for i in range(20):
        out.paste(frame(-1 if i in RAISED else 0), (0, 56 * i))
    return out


def icon():
    f = frame(0)
    head = f.crop((0, 0, 40, 32))
    return head.crop(head.getbbox())


def main():
    sh = sheet()
    if "--preview" in sys.argv:
        tpl = Image.open(sys.argv[sys.argv.index("--template") + 1]).convert("RGBA")
        k, idx = 8, [0, 3, 5, 7, 10, 14]
        out = Image.new("RGBA", (len(idx) * (40 * k + 8), 56 * k), (60, 60, 60, 255))
        for j, i in enumerate(idx):
            b = Image.new("RGBA", (40, 56), (40, 120, 200, 255))
            b.alpha_composite(tpl.crop((0, i * 56, 40, i * 56 + 56)))
            b.alpha_composite(sh.crop((0, i * 56, 40, i * 56 + 56)))
            out.paste(b.resize((40 * k, 56 * k), Image.NEAREST), (j * (40 * k + 8), 0))
        out.save(sys.argv[sys.argv.index("--preview") + 1])
        print("preview written")
        return
    sh.save(ROOT / "Content/Items/AtronachCrown_Head.png")
    ic = icon()
    ic.save(ROOT / "Content/Items/AtronachCrown.png")
    print("wrote AtronachCrown_Head.png", sh.size, "and icon", ic.size)


if __name__ == "__main__":
    main()
