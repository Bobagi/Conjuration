#!/usr/bin/env python3
"""Builds the Fire Atronach sheet and icons from the ComfyUI pose images.

Poses come from tools/comfy_atronach.py (concept) and C:\\ComfyUIFiles\\scripts\\qwen_edit.py (same
character in other poses). Every pose is cut, scaled with ONE common factor (taken from the idle pose,
so the character never changes size between frames), mapped to the mod palette, outlined and placed
with the feet on the bottom row and the torso on the frame centre line.

Also writes the buff icon, the tome (from a tools/comfy_atronach.py --tome concept), icon.png and icon_small.png.
Sheet (faces LEFT): 0 idle | 1 walk A | 2 pass | 3 walk B | 4 pass | 5 wind up | 6 throw.
Run with the rembg venv:
  C:\\ComfyUIFiles\\tools\\.venv\\Scripts\\python.exe tools/build_sheet.py --poses <dir with pose pngs>
Pose files are matched by name: atr_idle*, atr_walkA*, atr_walkB*, atr_windup*, atr_throw*.
"""
import argparse
import sys
from pathlib import Path

from PIL import Image

sys.path.insert(0, str(Path(__file__).parent))
from sprite_from_concept import cut, outline, quantize  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
ART_W, ART_H = 26, 32          # frame in art pixels; the sheet is 2x (52x64 per frame)
IDLE_ART_H = 31                # idle figure height in art pixels, outline included
ORDER = ["idle", "walkA", "pass", "walkB", "pass", "windup", "throw"]


def find(poses, key):
    hits = sorted(poses.glob(f"atr_{key}_*.png"))
    if not hits:
        raise SystemExit(f"missing pose atr_{key}_*.png in {poses}")
    return hits[-1]


def feet_and_torso(img):
    """Bottom row of the figure and the x centre of the torso band (rows 35%..60% of the figure)."""
    a = img.getchannel("A")
    w, h = img.size
    px = a.load()
    xs, n = 0, 0
    for y in range(int(h * 0.35), int(h * 0.60)):
        for x in range(w):
            if px[x, y]:
                xs += x
                n += 1
    return h, (xs / n if n else w / 2)


def to_art(cut_img, factor):
    """Block reduce to the art grid. Thin flames (hair, hand fire) would vanish under a plain average, so a
    block whose fire coverage reaches FIRE_COVER is kept and coloured by its fire pixels only; other blocks
    need BODY_COVER opaque coverage. Colours average opaque pixels only (no background bleed)."""
    import numpy as np
    FIRE_COVER, BODY_COVER = 0.15, 0.45
    src = np.asarray(cut_img.convert("RGBA")).astype(np.float32)
    rgb, a = src[..., :3], src[..., 3] > 0
    r, g, b = rgb[..., 0], rgb[..., 1], rgb[..., 2]
    mx, mn = rgb.max(-1), rgb.min(-1)
    fire = a & (r > 220) & (g > 120) & (r - b > 100) & (mx - mn > 90)
    w = max(1, round(cut_img.width * factor))
    h = max(1, round(cut_img.height * factor))
    ys = np.linspace(0, src.shape[0], h + 1).astype(int)
    xs = np.linspace(0, src.shape[1], w + 1).astype(int)
    out = Image.new("RGBA", (w, h), (0, 0, 0, 0))
    px = out.load()
    for j in range(h):
        for i in range(w):
            blk = (slice(ys[j], ys[j + 1]), slice(xs[i], xs[i + 1]))
            n = a[blk].size
            if n == 0:
                continue
            fcov, acov = fire[blk].mean(), a[blk].mean()
            if fcov >= FIRE_COVER:
                c = rgb[blk][fire[blk]].mean(0)
            elif acov >= BODY_COVER:
                c = rgb[blk][a[blk]].mean(0)
            else:
                continue
            px[i, j] = tuple(int(v) for v in c) + (255,)
    return outline(quantize(out))


FIRE_RAMP = [(168, 36, 12, 255), (255, 110, 24, 255), (255, 160, 36, 255), (255, 210, 74, 255)]


def add_flames(frame, seed):
    """Flickering flame tongues on top of the hair: at this size the AI hair strands merge into one blob,
    so short tongues (different every frame) give back the spiky crown and make it burn."""
    import random
    rng = random.Random(seed)
    px = frame.load()
    tops = {}
    for x in range(frame.width):
        for y in range(frame.height):
            if px[x, y][3]:
                tops[x] = y
                break
    if not tops:
        return frame
    head_top = min(tops.values())
    cols = [x for x, y in tops.items() if y <= head_top + 4]
    for x in cols:
        if rng.random() < 0.35:
            continue
        h = rng.choice([1, 2, 2, 3, 3, 4])
        y0 = tops[x]
        for k in range(1, h + 1):
            y = y0 - k
            if y < 0:
                break
            tone = 0 if k == h else (1 if k > h / 2 else 2)
            px[x, y] = FIRE_RAMP[tone]
        if y0 >= 0:
            px[x, y0] = FIRE_RAMP[3]   # the hair line glows where a tongue starts
    return frame


def place(art, bob=0):
    frame = Image.new("RGBA", (ART_W, ART_H), (0, 0, 0, 0))
    h, cx = feet_and_torso(art)
    x = round(ART_W / 2 - cx)
    y = ART_H - h - bob
    frame.paste(art, (x, y), art)   # paste clips negative offsets, alpha_composite does not
    return frame


def up(img, k=2):
    return img.resize((img.width * k, img.height * k), Image.NEAREST)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--poses", default=r"C:\ComfyUIFiles\output\qwen")
    ap.add_argument("--preview", help="write an enlarged contact sheet here instead of the game files")
    ap.add_argument("--tome", default=r"C:\ComfyUIFiles\output\conjuration\tome_s900_2057be_00001_.png",
                    help="ComfyUI tome concept (tools/comfy_atronach.py --tome)")
    a = ap.parse_args()
    poses = Path(a.poses)

    cuts = {}
    for key in ("idle", "walkA", "walkB", "windup", "throw"):
        c = cut(Image.open(find(poses, key))).transpose(Image.FLIP_LEFT_RIGHT)
        cuts[key] = c
    factor = (IDLE_ART_H - 2) / cuts["idle"].height
    arts = {k: to_art(v, factor) for k, v in cuts.items()}
    arts["pass"] = arts["idle"]

    frames = []
    for i, key in enumerate(ORDER):
        bob = 1 if key == "pass" else 0     # passing pose rises a little, it reads as a stride
        f = add_flames(place(arts[key], bob), 77 + i)
        if f.getbbox() and (f.getbbox()[2] - f.getbbox()[0] >= ART_W or arts[key].height > ART_H):
            print(f"warning: frame {i} ({key}) touches the frame edge, art {arts[key].size}")
        frames.append(f)

    sheet = Image.new("RGBA", (ART_W * 2, ART_H * 2 * len(frames)), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(up(f), (0, ART_H * 2 * i))

    head = arts["idle"].crop((0, 0, arts["idle"].width, 13))
    buff = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    hb = up(head.crop(head.getbbox()))
    buff.alpha_composite(hb, ((32 - hb.width) // 2, max(0, (32 - hb.height) // 2)))

    icon = Image.new("RGBA", (80, 80), (28, 12, 8, 255))
    ipx = icon.load()
    for y in range(80):
        for x in range(80):
            d = (((x // 4) * 4 - 40) ** 2 + ((y // 4) * 4 - 44) ** 2) ** 0.5
            if d < 34:
                ipx[x, y] = (92, 30, 10, 255) if d < 18 else (64, 22, 10, 255) if d < 26 else (44, 16, 9, 255)
    big = up(arts["windup"].crop(arts["windup"].getbbox()), 2)
    icon.alpha_composite(big, ((80 - big.width) // 2, 80 - big.height - 4))
    for i in range(80):
        for t in (0, 79):
            ipx[i, t] = (42, 18, 10, 255)
            ipx[t, i] = (42, 18, 10, 255)

    tc = cut(Image.open(a.tome))
    tart = to_art(tc, 14 / max(tc.width, tc.height))
    tome = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
    tome.paste(tart, ((16 - tart.width) // 2, (16 - tart.height) // 2), tart)
    tome = up(tome)

    small = Image.new("RGBA", (30, 30), (28, 12, 8, 255))
    hs = up(head.crop(head.getbbox()))
    hs.thumbnail((28, 28), Image.NEAREST)
    small.alpha_composite(hs, ((30 - hs.width) // 2, (30 - hs.height) // 2))

    if a.preview:
        k = 4
        bgs = [(40, 120, 200, 255), (20, 14, 30, 255)]
        prev = Image.new("RGBA", (ART_W * 2 * k * len(frames), ART_H * 2 * k * 2 + 90 * k), (60, 60, 60, 255))
        for r, bg in enumerate(bgs):
            for i, f in enumerate(frames):
                t = Image.new("RGBA", (ART_W * 2, ART_H * 2), bg)
                t.alpha_composite(up(f))
                prev.paste(up(t, k), (i * ART_W * 2 * k, r * ART_H * 2 * k))
        t = Image.new("RGBA", (32, 32), bgs[0])
        t.alpha_composite(buff)
        prev.paste(up(t, k), (0, ART_H * 4 * k))
        prev.paste(up(icon, k), (40 * k, ART_H * 4 * k))
        prev.save(a.preview)
        print("preview:", a.preview)
        return

    sheet.save(ROOT / "Content/Projectiles/FireAtronachMinion.png")
    buff.save(ROOT / "Content/Buffs/FireAtronachBuff.png")
    icon.save(ROOT / "icon.png")
    small.save(ROOT / "icon_small.png")
    tome.save(ROOT / "Content/Items/TomeConjureFireAtronach.png")
    print("wrote sheet", sheet.size, "buff, tome, icon, icon_small")


if __name__ == "__main__":
    main()
