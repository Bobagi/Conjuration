#!/usr/bin/env python3
"""Turns a ComfyUI concept image (1024, plain background) into Terraria style sprite frames.

Steps: remove the background (rembg, isnet-general-use), crop to the figure, shrink to the art grid
(Terraria draws 2x2 screen pixels per art pixel), reduce to a short palette, add a 1 art pixel dark
outline, then enlarge 2x with nearest neighbour.

  <rembg python> tools/sprite_from_concept.py concept.png --art-h 26 --out frame.png [--colors 14]

rembg lives in the ComfyUI tools venv: C:\\ComfyUIFiles\\tools\\.venv\\Scripts\\python.exe
"""
import argparse
from pathlib import Path

from PIL import Image

OUTLINE = (42, 18, 10, 255)


def cut(img):
    from rembg import new_session, remove
    out = remove(img.convert("RGB"), session=new_session("isnet-general-use"))
    # hard alpha, pixel art has no soft edges
    a = out.getchannel("A").point(lambda v: 255 if v > 110 else 0)
    out.putalpha(a)
    return out.crop(a.getbbox())


def shrink(img, art_h):
    """Area average per art pixel for color, majority vote for alpha."""
    scale = art_h / img.height
    art_w = max(1, round(img.width * scale))
    rgb = img.convert("RGB").resize((art_w, art_h), Image.BOX)
    alpha = img.getchannel("A").resize((art_w, art_h), Image.BOX).point(lambda v: 255 if v >= 128 else 0)
    rgb.putalpha(alpha)
    return rgb


# Fixed palette shared with the rest of the mod art: basalt ramp, metal ramp and fire ramp.
ROCK = [(42, 18, 10), (59, 36, 24), (94, 58, 36), (125, 77, 46)]
METAL = [(36, 32, 36), (64, 58, 62), (98, 90, 92), (140, 132, 130)]
FIRE = [(168, 36, 12), (226, 64, 16), (255, 110, 24), (255, 160, 36), (255, 210, 74), (255, 241, 168)]


def quantize(img, colors=None, fire_boost=1.25):
    """Map each art pixel to the fixed palette: warm saturated pixels go to the fire ramp by brightness
    (boosted, the box average dims flames), grey pixels to the metal ramp, the rest to basalt."""
    import colorsys
    out = img.copy()
    px = out.load()
    for y in range(out.height):
        for x in range(out.width):
            r, g, b, a = px[x, y]
            if a == 0:
                continue
            h, l, s = colorsys.rgb_to_hls(r / 255, g / 255, b / 255)
            warm = h < 0.14 or h > 0.95
            if l > 0.85:
                c = FIRE[-1] if l > 0.92 else FIRE[-2]      # white hot core
            elif warm and s > 0.45 and l > 0.45:
                v = min(0.999, (l - 0.45) / 0.4 * fire_boost)
                c = FIRE[1 + int(v * (len(FIRE) - 1))] if v > 0.1 else FIRE[0]
            elif s < 0.18:
                c = METAL[min(3, int(l / 0.6 * 4))]
            else:
                c = ROCK[min(3, int(l / 0.42 * 4))]
            px[x, y] = c + (255,)
    return out


def outline(img):
    """1 px dark outline on the outside of the silhouette (4-neighbour), canvas grown by 1 on each side."""
    w, h = img.size
    big = Image.new("RGBA", (w + 2, h + 2), (0, 0, 0, 0))
    big.paste(img, (1, 1))
    px = big.load()
    solid = [[px[x, y][3] > 0 for y in range(h + 2)] for x in range(w + 2)]
    for x in range(w + 2):
        for y in range(h + 2):
            if solid[x][y]:
                continue
            for dx, dy in ((1, 0), (-1, 0), (0, 1), (0, -1)):
                nx, ny = x + dx, y + dy
                if 0 <= nx < w + 2 and 0 <= ny < h + 2 and solid[nx][ny]:
                    px[x, y] = OUTLINE
                    break
    return big


def convert(path, art_h, colors, flip=False):
    img = cut(Image.open(path))
    if flip:
        img = img.transpose(Image.FLIP_LEFT_RIGHT)
    art = outline(quantize(shrink(img, art_h - 2), colors))
    return art


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("image")
    ap.add_argument("--art-h", type=int, default=26, help="height in art pixels, outline included")
    ap.add_argument("--colors", type=int, default=14)
    ap.add_argument("--flip", action="store_true", help="mirror (the sheet must face left)")
    ap.add_argument("--out", required=True)
    a = ap.parse_args()
    art = convert(a.image, a.art_h, a.colors, a.flip)
    art.save(Path(a.out).with_suffix(".art.png"))
    art.resize((art.width * 2, art.height * 2), Image.NEAREST).save(a.out)
    print(a.out, "art", art.size)


if __name__ == "__main__":
    main()
