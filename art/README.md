# Editable sprites

Every sprite of the mod, one PNG per frame, at **1x art pixels** (Terraria draws each art pixel as 2x2
screen pixels, so the files in `Content/` are twice this size). Edit here, then pack:

```
python tools/art_folder.py pack      # art/ -> Content/ (doubles to 2x2, stacks frames into sheets)
python tools/check_art.py            # sizes
<tModLoader>/dotnet/dotnet tModLoader.dll -build <repo>   # or Workshop > Develop Mods > Build + Reload
```

`python tools/art_folder.py unpack` does the opposite (Content/ -> art/) and overwrites these files.

Editors: Aseprite, LibreSprite or Pixelorama (free), 1 px grid, pencil tool, no anti aliasing, keep the
transparent background. Terraria look: dark outline, 3 or 4 tones per material, light from the top left.

## What is where

| Folder | Frames | Notes |
|---|---|---|
| `atronachs/Fire`, `Frost`, `Storm` | `0_idle`, `1_walkA`, `2_pass`, `3_walkB`, `4_pass`, `5_attack` | facing **right**, feet on the bottom row; all frames of one creature share one canvas size (the biggest is used) |
| `buffs/` | one icon each | max 16x16 art pixels (32x32 in game) |
| `tomes/` | one each | the three Conjuration tomes (Skyrim's purple tome, gem on the spine = element). `tools/make_tomes.py` redraws them from scratch, overwriting edits |
| `npc/Conjurer/` | 25 frames, Town NPC layout | facing **left**. `00_idle`, `01_jump`, `02..15_walk`, `16-17` raised arm, `18_sit`, `19_talk`, `20_blink`, `21-24` throw. `Conjurer_Head.png` is the housing/map icon and is copied as is |

Animation in game: walk cycles `1_walkA -> 2_pass -> 3_walkB -> 4_pass` (speed follows the walking speed);
`5_attack` shows while the atronach winds up and casts or punches. Frame order and timing live in
`Content/Projectiles/AtronachMinion.cs`.

Want more frames (a smoother walk, a separate wind up)? Add files and tell the code: `FrameCount` and the
frame constants in `AtronachMinion.cs`, and the `ATRONACH_FRAMES` list in `tools/art_folder.py`.
