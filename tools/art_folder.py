#!/usr/bin/env python3
"""Hand editing workflow for the mod's sprites.

  python tools/art_folder.py unpack   Content/ sprites -> art/ (one PNG per frame, 1x art pixels)
  python tools/art_folder.py pack     art/ -> Content/ (2x2 Terraria pixels, frames stacked into sheets)

Terraria draws every art pixel as 2x2 screen pixels, so the files in Content/ are twice the size of the
art. Edit the 1x files in art/ (Aseprite, LibreSprite, Pixelorama: 1 px grid, no anti aliasing), then
pack and rebuild the mod. See art/README.md.
"""
import sys
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
ART = ROOT / "art"

ATRONACH_FRAMES = ["0_idle", "1_walkA", "2_pass", "3_walkB", "4_pass", "5_attack"]
NPC_FRAMES = (["00_idle", "01_jump"] + [f"{i:02d}_walk{i - 1}" for i in range(2, 16)]
              + ["16_arm_up", "17_arm_forward", "18_sit", "19_talk", "20_blink", "21_throw1", "22_throw2",
                 "23_throw3", "24_throw4"])
ATRONACHS = ["Fire", "Frost", "Storm"]


def half(img):
    """2x2 blocks -> 1 art pixel. Finds the grid phase first (some sprites sit 1 px off the 2x2 grid)."""
    best = None
    for ox in (0, 1):
        for oy in (0, 1):
            c = img.crop((ox, oy, img.width, img.height))
            c = c.crop((0, 0, c.width // 2 * 2, c.height // 2 * 2))
            if is_2x(c):
                best = c
                break
        if best is not None:
            break
    if best is None:
        best = img
    return best.resize((best.width // 2, best.height // 2), Image.NEAREST)


def double(img, size=None):
    """1 art pixel -> 2x2; size pads/centres onto a fixed canvas (icons must stay 32x32)."""
    big = img.resize((img.width * 2, img.height * 2), Image.NEAREST)
    if size and big.size != size:
        canvas = Image.new("RGBA", size, (0, 0, 0, 0))
        canvas.paste(big, ((size[0] - big.width) // 2 // 2 * 2, (size[1] - big.height) // 2 // 2 * 2))
        big = canvas
    return big


def is_2x(img):
    a = img.load()
    for y in range(0, img.height - 1, 2):
        for x in range(0, img.width - 1, 2):
            if not (a[x, y] == a[x + 1, y] == a[x, y + 1] == a[x + 1, y + 1]):
                return False
    return True


def split(sheet, n):
    fh = sheet.height // n
    return [sheet.crop((0, i * fh, sheet.width, (i + 1) * fh)) for i in range(n)]


def stack(frames):
    w = max(f.width for f in frames)
    h = max(f.height for f in frames)
    out = Image.new("RGBA", (w, h * len(frames)), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        out.paste(f, ((w - f.width) // 2, i * h + h - f.height))
    return out


def unpack():
    for name in ATRONACHS:
        sheet = Image.open(ROOT / f"Content/Projectiles/{name}AtronachMinion.png").convert("RGBA")
        d = ART / "atronachs" / name
        d.mkdir(parents=True, exist_ok=True)
        for fname, f in zip(ATRONACH_FRAMES, split(sheet, len(ATRONACH_FRAMES))):
            half(f).save(d / f"{fname}.png")
        buff = Image.open(ROOT / f"Content/Buffs/{name}AtronachBuff.png").convert("RGBA")
        (ART / "buffs").mkdir(parents=True, exist_ok=True)
        half(buff).save(ART / "buffs" / f"{name}AtronachBuff.png")
    (ART / "tomes").mkdir(parents=True, exist_ok=True)
    for name in ATRONACHS:
        tome = Image.open(ROOT / f"Content/Items/TomeConjure{name}Atronach.png").convert("RGBA")
        half(tome).save(ART / "tomes" / f"TomeConjure{name}Atronach.png")
    npc = Image.open(ROOT / "Content/NPCs/Conjurer.png").convert("RGBA")
    d = ART / "npc" / "Conjurer"
    d.mkdir(parents=True, exist_ok=True)
    frames = split(npc, len(NPC_FRAMES))
    two = all(is_2x(f) for f in frames)
    for fname, f in zip(NPC_FRAMES, frames):
        (half(f) if two else f).save(d / f"{fname}.png")
    (d / ("SCALE_2X" if two else "SCALE_1X")).write_text("pack doubles these frames\n" if two else "pack keeps these frames as they are\n")
    head = Image.open(ROOT / "Content/NPCs/Conjurer_Head.png").convert("RGBA")
    head.save(d.parent / "Conjurer_Head.png")
    print("unpacked into", ART)


def pack():
    for name in ATRONACHS:
        d = ART / "atronachs" / name
        frames = [Image.open(d / f"{f}.png").convert("RGBA") for f in ATRONACH_FRAMES]
        double(stack(frames)).save(ROOT / f"Content/Projectiles/{name}AtronachMinion.png")
        double(Image.open(ART / "buffs" / f"{name}AtronachBuff.png").convert("RGBA"), (32, 32)).save(ROOT / f"Content/Buffs/{name}AtronachBuff.png")
        double(Image.open(ART / "tomes" / f"TomeConjure{name}Atronach.png").convert("RGBA")).save(ROOT / f"Content/Items/TomeConjure{name}Atronach.png")
    d = ART / "npc" / "Conjurer"
    frames = [Image.open(d / f"{f}.png").convert("RGBA") for f in NPC_FRAMES]
    sheet = stack(frames)
    if (d / "SCALE_2X").exists():
        sheet = double(sheet)
    sheet.save(ROOT / "Content/NPCs/Conjurer.png")
    Image.open(d.parent / "Conjurer_Head.png").convert("RGBA").save(ROOT / "Content/NPCs/Conjurer_Head.png")
    print("packed into Content/ ; rebuild the mod and run python tools/check_art.py")


if __name__ == "__main__":
    {"unpack": unpack, "pack": pack}[sys.argv[1] if len(sys.argv) > 1 else "unpack"]()
