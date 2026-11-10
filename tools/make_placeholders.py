#!/usr/bin/env python3
"""Writes the placeholder PNGs so the mod builds before real art exists.

Stdlib only (no Pillow). Run from the repo root:  python3 tools/make_placeholders.py
Overwrites the sprites named below, so DO NOT run it after dropping in real art
(or pass --keep-existing, which skips files that already exist).
"""
import math, struct, sys, zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
KEEP = "--keep-existing" in sys.argv


def write_png(path, w, h, px):
    """px: dict {(x, y): (r, g, b, a)}; everything else is transparent."""
    if KEEP and path.exists():
        print("keep", path.relative_to(ROOT)); return
    raw = bytearray()
    for y in range(h):
        raw.append(0)
        for x in range(w):
            raw.extend(px.get((x, y), (0, 0, 0, 0)))
    def chunk(tag, data):
        c = struct.pack(">I", len(data)) + tag + data
        return c + struct.pack(">I", zlib.crc32(tag + data) & 0xFFFFFFFF)
    png = b"\x89PNG\r\n\x1a\n" + chunk(b"IHDR", struct.pack(">IIBBBBB", w, h, 8, 6, 0, 0, 0)) \
        + chunk(b"IDAT", zlib.compress(bytes(raw), 9)) + chunk(b"IEND", b"")
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_bytes(png)
    print("wrote", path.relative_to(ROOT), f"{w}x{h}")


def disc(px, cx, cy, r, color):
    for y in range(int(cy - r) - 1, int(cy + r) + 2):
        for x in range(int(cx - r) - 1, int(cx + r) + 2):
            if (x - cx) ** 2 + (y - cy) ** 2 <= r * r:
                px[(x, y)] = color


def tome():
    px = {}
    for y in range(6, 27):
        for x in range(7, 25):
            px[(x, y)] = (92, 48, 24, 255)
    for y in range(8, 25):
        for x in range(9, 23):
            px[(x, y)] = (150, 84, 40, 255)
    disc(px, 16, 16, 4, (255, 150, 30, 255))
    disc(px, 16, 15, 2, (255, 230, 120, 255))
    return px


def buff_icon():
    px = {}
    disc(px, 16, 18, 9, (200, 70, 20, 255))
    disc(px, 16, 17, 6, (255, 150, 30, 255))
    disc(px, 16, 16, 3, (255, 230, 120, 255))
    for x in range(12, 21):
        px[(x, 6 + abs(x - 16))] = (255, 120, 20, 255)
    return px


def minion_sheet(w=40, h=44, frames=4):
    px = {}
    for f in range(frames):
        oy = f * h
        bob = round(2 * math.sin(f / frames * 2 * math.pi))
        # rocky body
        disc(px, w / 2, oy + 28 + bob, 11, (70, 40, 30, 255))
        disc(px, w / 2, oy + 27 + bob, 9, (120, 60, 35, 255))
        # glowing core
        disc(px, w / 2, oy + 27 + bob, 4, (255, 160, 40, 255))
        # flame crown, flickers per frame
        for i, dx in enumerate((-7, -2, 3, 8)):
            top = 6 + ((i + f) % 3) * 2
            for y in range(oy + top + bob, oy + 20 + bob):
                half = max(1, (y - (oy + top + bob)) // 3)
                for x in range(int(w / 2 + dx - half), int(w / 2 + dx + half) + 1):
                    px[(x, y)] = (255, 90 + (y - oy) * 4 % 120, 20, 255)
        # eyes
        px[(15, oy + 24 + bob)] = px[(16, oy + 24 + bob)] = (255, 255, 200, 255)
        px[(24, oy + 24 + bob)] = px[(25, oy + 24 + bob)] = (255, 255, 200, 255)
    return w, h * frames, px


if __name__ == "__main__":
    write_png(ROOT / "Content/Items/TomeConjureFireAtronach.png", 32, 32, tome())
    write_png(ROOT / "Content/Buffs/FireAtronachBuff.png", 32, 32, buff_icon())
    w, h, px = minion_sheet()
    write_png(ROOT / "Content/Projectiles/FireAtronachMinion.png", w, h, px)
    write_png(ROOT / "icon.png", 80, 80, {(x, y): c for (x, y), c in tome().items()} | {})
