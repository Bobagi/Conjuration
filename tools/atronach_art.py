#!/usr/bin/env python3
"""Builds the atronach sprite sheets and buff icons from pose images made from the Skyrim references.

Pipeline (docs/ART.md):
  Skyrim reference (UESP image)  ->  Qwen-Image-Edit poses, side view, same creature, white background
  ->  this script: background cut (rembg isnet), PixelOE style pixelization (contrast aware outline
      expansion, then k-centroid downscale per patch), one shared palette per creature, 1 art pixel dark
      outline, 2x2 Terraria pixels, same scale for every pose, feet aligned.

Sheet layout (facing right): 0 idle | 1 walk A | 2 pass | 3 walk B | 4 pass | 5 attack.
Run with the ComfyUI tools venv (rembg, numpy, Pillow):
  C:\\ComfyUIFiles\\tools\\.venv\\Scripts\\python.exe tools/atronach_art.py poses.json [--preview out.png]
poses.json: {"flame": {"idle": path, "walkA": path, "walkB": path, "cast": path}, "frost": {...}, ...}
"""
import json
import sys
from pathlib import Path

import numpy as np
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parent.parent
PATCH = 8   # working pixels per art pixel

# art height of the idle figure (art pixels, outline excluded), palette size, attack pose name, minion file
CREATURES = {
    "flame": dict(height=44, colors=16, attack="cast", out="FireAtronach", glow=(255, 226, 120)),
    "frost": dict(height=52, colors=12, attack="punch", out="FrostAtronach", glow=(214, 246, 255)),
    "storm": dict(height=48, colors=14, attack="cast", out="StormAtronach", glow=(206, 190, 255)),
}


def cut(path):
    from rembg import new_session, remove
    img = Image.open(path).convert("RGB")
    out = remove(img, session=new_session("isnet-general-use"))
    a = out.getchannel("A").point(lambda v: 255 if v > 100 else 0)
    out.putalpha(a)
    return out.crop(a.getbbox())


def lum(arr):
    return arr[..., 0] * 0.299 + arr[..., 1] * 0.587 + arr[..., 2] * 0.114


def outline_expand(img, k):
    """PixelOE idea: pixels brighter than their neighbourhood are dilated (bright details grow), darker
    ones are eroded (dark lines grow), weighted by local contrast, so thin features survive the shrink."""
    rgb = img.convert("RGB")
    size = k if k % 2 else k + 1
    mx = np.asarray(rgb.filter(ImageFilter.MaxFilter(size))).astype(np.float32)
    mn = np.asarray(rgb.filter(ImageFilter.MinFilter(size))).astype(np.float32)
    md = np.asarray(rgb.filter(ImageFilter.MedianFilter(size))).astype(np.float32)
    src = np.asarray(rgb).astype(np.float32)
    L, Lmd = lum(src), lum(md)
    contrast = np.clip((lum(mx) - lum(mn)) / 96.0, 0, 1)[..., None]
    brighter = (L > Lmd)[..., None]
    expanded = np.where(brighter, mx, mn)
    outp = src * (1 - contrast) + expanded * contrast
    res = Image.fromarray(np.clip(outp, 0, 255).astype(np.uint8))
    res.putalpha(img.getchannel("A"))
    return res


def k_centroid(img, patch):
    """Each art pixel = centroid of the dominant of 2 colour clusters in its patch (alpha by majority)."""
    arr = np.asarray(img).astype(np.float32)
    h, w = arr.shape[0] // patch, arr.shape[1] // patch
    out = np.zeros((h, w, 4), np.uint8)
    for y in range(h):
        for x in range(w):
            blk = arr[y * patch:(y + 1) * patch, x * patch:(x + 1) * patch].reshape(-1, 4)
            opaque = blk[blk[:, 3] > 0]
            if len(opaque) < 0.4 * len(blk):
                continue
            c = opaque[:, :3]
            cent = np.stack([c[np.argmin(lum(c))], c[np.argmax(lum(c))]])
            for _ in range(4):
                lab = np.argmin(((c[:, None, :] - cent[None]) ** 2).sum(-1), axis=1)
                for j in range(2):
                    if (lab == j).any():
                        cent[j] = c[lab == j].mean(0)
            dom = np.bincount(lab, minlength=2).argmax()
            out[y, x, :3] = cent[dom]
            out[y, x, 3] = 255
    return Image.fromarray(out, "RGBA")


def warm_highlights(img, glow):
    """Near white pixels take the element's glow colour (white hot fire reads as yellow, not beige)."""
    arr = np.asarray(img).astype(np.float32)
    L = lum(arr)
    t = np.clip((L - 190) / 55.0, 0, 1)[..., None] * 0.5
    arr[..., :3] = arr[..., :3] * (1 - t) + np.array(glow, np.float32) * t
    return Image.fromarray(arr.astype(np.uint8), "RGBA")


def despeckle(img):
    """A pixel whose colour appears in none of its 8 neighbours takes the most common neighbour colour."""
    arr = np.asarray(img).copy()
    h, w = arr.shape[:2]
    src = arr.copy()
    for y in range(h):
        for x in range(w):
            if src[y, x, 3] == 0:
                continue
            me = tuple(src[y, x])
            nb = [tuple(src[yy, xx]) for yy in range(max(0, y - 1), min(h, y + 2)) for xx in range(max(0, x - 1), min(w, x + 2))
                  if (yy, xx) != (y, x) and src[yy, xx, 3] > 0]
            if len(nb) >= 5 and me not in nb:
                arr[y, x] = max(set(nb), key=nb.count)
    return Image.fromarray(arr, "RGBA")


def pixelize(cut_img, scale, saturation=1.35):
    """scale: art pixels per source pixel. Saturation is boosted first: glowing bodies (white hot fire,
    lit ice) otherwise end up as pale beige or grey in a small palette."""
    from PIL import ImageEnhance
    alpha = cut_img.getchannel("A")
    cut_img = ImageEnhance.Color(cut_img.convert("RGB")).enhance(saturation).convert("RGBA")
    cut_img.putalpha(alpha)
    w = max(1, round(cut_img.width * scale)) * PATCH
    h = max(1, round(cut_img.height * scale)) * PATCH
    work = cut_img.resize((w, h), Image.LANCZOS)
    a = work.getchannel("A").point(lambda v: 255 if v > 127 else 0)
    work.putalpha(a)
    return k_centroid(outline_expand(work, PATCH // 2 + 1), PATCH)


def shared_palette(arts, colors):
    """Quantize all frames of one creature together, so the palette never flickers between frames."""
    strip = Image.new("RGBA", (sum(a.width for a in arts), max(a.height for a in arts)), (0, 0, 0, 0))
    x = 0
    for a in arts:
        strip.paste(a, (x, 0))
        x += a.width
    pal = strip.convert("RGB").quantize(colors=colors, method=Image.Quantize.MAXCOVERAGE, dither=Image.Dither.NONE)
    out = []
    for a in arts:
        q = a.convert("RGB").quantize(palette=pal, dither=Image.Dither.NONE).convert("RGBA")
        q.putalpha(a.getchannel("A"))
        out.append(q)
    darkest = min((tuple(pal.getpalette()[i * 3:i * 3 + 3]) for i in range(colors)), key=lambda c: sum(c))
    return out, tuple(int(v * 0.45) for v in darkest) + (255,)


def outline(img, color):
    w, h = img.size
    big = Image.new("RGBA", (w + 2, h + 2), (0, 0, 0, 0))
    big.paste(img, (1, 1))
    a = np.asarray(big)[..., 3] > 0
    ring = np.zeros_like(a)
    ring[1:, :] |= a[:-1, :]
    ring[:-1, :] |= a[1:, :]
    ring[:, 1:] |= a[:, :-1]
    ring[:, :-1] |= a[:, 1:]
    ring &= ~a
    arr = np.asarray(big).copy()
    arr[ring] = color
    return Image.fromarray(arr, "RGBA")


def torso_x(art):
    a = np.asarray(art)[..., 3] > 0
    h = a.shape[0]
    band = a[int(h * 0.3):int(h * 0.6)]
    ys, xs = np.nonzero(band)
    return xs.mean() if len(xs) else art.width / 2


def build(name, poses, cfg):
    order = ["idle", "walkA", "walkB", cfg["attack"]]
    cuts = {k: cut(poses[k]) for k in order if poses.get(k)}
    if "idle" not in cuts:
        raise SystemExit(f"{name}: no idle pose")
    scale = cfg["height"] / cuts["idle"].height
    keys = list(cuts)
    arts, ocol = shared_palette([warm_highlights(pixelize(cuts[k], scale), cfg["glow"]) for k in keys], cfg["colors"])
    arts = {k: outline(despeckle(a), ocol) for k, a in zip(keys, arts)}
    for k in ("walkA", "walkB"):
        arts.setdefault(k, arts["idle"])
    arts.setdefault(cfg["attack"], arts["idle"])

    seq = [("idle", 0), ("walkA", 0), ("idle", 1), ("walkB", 0), ("idle", 1), (cfg["attack"], 0)]
    fw = max(a.width for a in arts.values()) + 4
    fw += fw % 2
    fh = max(a.height for a in arts.values()) + 3
    frames = []
    for key, bob in seq:
        a = arts[key]
        f = Image.new("RGBA", (fw, fh), (0, 0, 0, 0))
        x = round(fw / 2 - torso_x(a))
        f.paste(a, (max(0, min(fw - a.width, x)), fh - a.height - bob), a)
        frames.append(f)
    sheet = Image.new("RGBA", (fw * 2, fh * 2 * len(frames)), (0, 0, 0, 0))
    for i, f in enumerate(frames):
        sheet.paste(f.resize((fw * 2, fh * 2), Image.NEAREST), (0, fh * 2 * i))

    idle = arts["idle"]
    head = idle.crop((0, 0, idle.width, max(8, int(idle.height * 0.42))))
    head = head.crop(head.getbbox())
    head.thumbnail((16, 16), Image.NEAREST)
    icon = Image.new("RGBA", (32, 32), (0, 0, 0, 0))
    hb = head.resize((head.width * 2, head.height * 2), Image.NEAREST)
    icon.paste(hb, ((32 - hb.width) // 2, (32 - hb.height) // 2), hb)
    return sheet, icon, frames


def main():
    poses = json.loads(Path(sys.argv[1]).read_text())
    preview = sys.argv[sys.argv.index("--preview") + 1] if "--preview" in sys.argv else None
    rows = []
    for name, cfg in CREATURES.items():
        if name not in poses:
            continue
        sheet, icon, frames = build(name, poses[name], cfg)
        rows.append(frames)
        if not preview:
            sheet.save(ROOT / f"Content/Projectiles/{cfg['out']}Minion.png")
            icon.save(ROOT / f"Content/Buffs/{cfg['out']}Buff.png")
            print("wrote", cfg["out"], "sheet", sheet.size)
    if preview:
        k = 4
        W = max(sum(f.width for f in r) + 4 * len(r) for r in rows) * k
        H = sum(max(f.height for f in r) + 4 for r in rows) * k
        out = Image.new("RGBA", (W, H), (60, 60, 60, 255))
        y = 0
        for r in rows:
            x = 0
            for f in r:
                t = Image.new("RGBA", f.size, (40, 120, 200, 255))
                t.alpha_composite(f)
                out.paste(t.resize((f.width * k * 1, f.height * k), Image.NEAREST), (x, y))
                x += (f.width + 4) * k
            y += (max(f.height for f in r) + 4) * k
        out.save(preview)
        print("preview", preview)


if __name__ == "__main__":
    main()
