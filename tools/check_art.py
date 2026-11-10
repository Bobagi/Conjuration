#!/usr/bin/env python3
"""Checks that every sprite has the size the code expects. Stdlib only.
Run from the repo root before building:  python3 tools/check_art.py
"""
import struct, sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent
# path -> (width, height) ; keep in sync with docs/ART.md.
EXPECTED = {
    # The Atronach is drawn with the game's player renderer; its texture is only a transparent stand in.
    "Content/Projectiles/FireAtronachMinion.png": (20, 42),
    "Content/Buffs/FireAtronachBuff.png": (32, 32),
    # Town NPC sheet: 25 frames of 40x58, exported in game with /atronachtest npc.
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
