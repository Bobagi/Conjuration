#!/usr/bin/env python3
"""Checks that every sprite has the size the code expects. Stdlib only.
Run from the repo root before building:  python3 tools/check_art.py
"""
import struct, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
# path -> (width, height) ; keep in sync with docs/ART.md and FrameCount in the minion.
EXPECTED = {
    "Content/Items/TomeConjureFireAtronach.png": (32, 32),
    "Content/Buffs/FireAtronachBuff.png": (32, 32),
    "Content/Projectiles/FireAtronachMinion.png": (52, 448),   # 7 frames of 52x64, stacked
    "icon.png": (80, 80),
    "icon_small.png": (30, 30),
}


def size(path):
    with open(path, "rb") as f:
        head = f.read(24)
    if head[:8] != b"\x89PNG\r\n\x1a\n":
        raise ValueError("not a PNG")
    return struct.unpack(">II", head[16:24])


bad = 0
for rel, want in EXPECTED.items():
    p = ROOT / rel
    if not p.exists():
        print("MISSING", rel); bad += 1; continue
    got = size(p)
    ok = got == want
    print(("ok     " if ok else "WRONG  ") + rel, got, "" if ok else f"(want {want})")
    bad += not ok
sys.exit(1 if bad else 0)
