# Art guide

All mod art comes from the local **ComfyUI** (generation) plus three small scripts (turning it into
Terraria sprites). Nothing is copied from Skyrim: the look (a slender woman of living fire with flame
hair, floating embers) is described in words in the prompt, never fed as an image.

| File | Size | What |
|---|---|---|
| `Content/Projectiles/FireAtronachMinion.png` | **52x448** | 7 frames of 52x64 stacked, frame 0 on top, facing **left** |
| `Content/Buffs/FireAtronachBuff.png` | 32x32 | head and flame crown |
| `Content/Items/TomeConjureFireAtronach.png` | 32x32 | the spell tome |
| `icon.png` / `icon_small.png` | 80x80 / 30x30 | mod list icons |

Sheet frames: `0 idle | 1 walk A | 2 pass | 3 walk B | 4 pass | 5 wind up | 6 throw`. The AI in
`Content/Projectiles/FireAtronachMinion.cs` picks them (`FrameIdle`, `FrameWalk`, `FrameWindup`,
`FrameThrow`) and mirrors the sprite when it faces right. Run `python tools/check_art.py` after any change.

## Terraria style rules (why the scripts do what they do)

- Vanilla sprites use **2x2 screen pixels per art pixel** (the Fire Imp is 19x22 art pixels in a 38x44
  image). Everything is reduced to the art grid and enlarged 2x with nearest neighbour.
- Dark 1 art pixel outline, flat shading, short palette: basalt `#2a120a #3b2418 #5e3a24 #7d4d2e`,
  fire `#a8240c #e24010 #ff6e18 #ffa024 #ffd24a #fff1a8`, metal greys for armor.
- The minion is about player height (31 art pixels, 62 on screen) and stands on the ground.

## Pipeline

ComfyUI must be running on `127.0.0.1:8188` (see `C:\ComfyUIFiles\CLAUDE.md`). Models used:
`sd_xl_base_1.0` + LoRA **Pixel Art XL** (`loras/pixel-art-xl.safetensors`, nerijs, OpenRAIL-M, weight 1.2)
for concepts, and **Qwen-Image-Edit 2511** (`C:\ComfyUIFiles\scripts\qwen_edit.py`) for new poses of the
same character.

1. **Concept** (about 25 s each on the 3060 Ti):
   `python tools/comfy_atronach.py --count 6 --name v1` and pick the best in
   `C:\ComfyUIFiles\output\conjuration\`.
2. **Idle pose from the concept**, then the other poses **from the idle** (about 2.5 min each), always
   starting the prompt with "Same fire creature, same pixel art style, same colors and proportions,
   full body side view facing right, plain white background, nothing else. Change only the pose:".
   Output names must be `atr_idle_*`, `atr_walkA_*`, `atr_walkB_*`, `atr_windup_*`, `atr_throw_*`:
   ```
   python C:\ComfyUIFiles\scripts\qwen_edit.py --image <concept.png> --name atr_idle --prompt "..."
   python C:\ComfyUIFiles\scripts\qwen_edit.py --image <atr_idle.png> --name atr_walkA --prompt "... left leg forward ..."
   ```
3. **Tome**: `python tools/comfy_atronach.py --tome --count 4 --name tome` (dark covers survive the
   background cut better than white ones).
4. **Build everything** with the rembg venv:
   `C:\ComfyUIFiles\tools\.venv\Scripts\python.exe tools/build_sheet.py [--preview out.png]`.
   It cuts the background (rembg isnet), scales every pose by the **same** factor (taken from idle),
   keeps thin flames (a block with 15% fire is kept and coloured by its fire pixels), maps colours to the
   palette (`tools/sprite_from_concept.py`), outlines, aligns feet and torso, adds flickering flame
   tongues to the hair and writes all the files above.

## Lessons (2026-11-10)

- No free asset fit: the CC0 fire creatures on OpenGameArt had no outline or a cute style.
- A plain average when shrinking makes fire dull and erases the flame hair; the fire aware reduction
  and the per frame flame tongues fix it.
- Count orange rock as basalt and only bright warm pixels as fire, or the whole body turns flat orange.
- Qwen-Image-Edit keeps the character very consistent between poses; SDXL img2img does not.
