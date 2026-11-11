#!/usr/bin/env python3
"""Checks that every sprite has the size the code expects. Stdlib only.
Run from the repo root before building:  python3 tools/check_art.py
"""
import struct, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
# path -> (width, height) ; keep in sync with docs/ART.md.
EXPECTED = {
    "Content/Projectiles/FireAtronachMinion.png": (64, 480),   # 6 frames, tools/atronach_art.py
    "Content/Buffs/FireAtronachBuff.png": (32, 32),
    "Content/Projectiles/FrostAtronachMinion.png": (100, 684),   # 6 frames, tools/atronach_art.py
    "Content/Buffs/FrostAtronachBuff.png": (32, 32),
    "Content/Projectiles/StormAtronachMinion.png": (104, 636),   # 6 frames, tools/atronach_art.py
    "Content/Buffs/StormAtronachBuff.png": (32, 32),
    "Content/Projectiles/Blank.png": (2, 2),
    "Content/NPCs/Conjurer.png": (40, 1450),
    "icon.png": (80, 80),
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
