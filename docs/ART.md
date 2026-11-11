# Art guide

## Atronachs: sprite sheets made from the Skyrim references

The owner wants the atronachs to look like the Skyrim originals, so the sheets start from the real
reference images (UESP, `Skyrim:Daedra`: Flame, Frost and Storm Atronach). Nothing from Skyrim is shipped:
the references only feed the generator, the mod gets new Terraria scale pixel art.

1. **References**: the UESP creature images (`images.uesp.net/.../SR-creature-*_Atronach.jpg`), saved
   locally through the browser (UESP blocks scripted downloads).
2. **Poses** with Qwen-Image-Edit 2511 (local ComfyUI, `C:\ComfyUIFiles\scripts\qwen_edit.py`): first a
   side view idle **from the reference**, then every other pose **from that idle** (same character). Every
   prompt starts with: "Keep exactly the same creature design, shapes, materials and colors as the input
   image. Full body visible, side view facing right, plain flat white background, no ground, no scenery,
   nothing else." About 2.5 minutes per pose on the 3060 Ti.
   - Flame: idle, walkA, walkB, cast (fireball leaving the hand)
   - Frost: idle, walkA, walkB, punch
   - Storm: idle, cast (lightning from a floating rock arm); it hovers, so no walk poses
3. **Sheets**: `tools/atronach_art.py poses.json` (run with `C:\ComfyUIFiles\tools\.venv\Scripts\python.exe`):
   - background cut (rembg isnet), same scale for every pose of a creature, feet aligned
   - PixelOE style pixelization: contrast aware outline expansion (pixels brighter than their
     neighbourhood dilate, darker ones erode) and a k-centroid downscale per 8x8 patch, so thin features
     such as flames and horns survive the shrink. (The `pixeloe` package itself could not be installed
     here; the script implements the same idea with Pillow and numpy.)
   - saturation boost and glow tint for near white pixels, one palette per creature for all frames,
     isolated pixel cleanup, 1 art pixel dark outline, 2x2 Terraria pixels
   - writes `Content/Projectiles/<Name>AtronachMinion.png` and `Content/Buffs/<Name>AtronachBuff.png`;
     `--preview out.png` writes a contact sheet instead
4. Rebuild, then `python tools/check_art.py`.

Sheet layout (facing right, frames stacked): `0 idle | 1 walk A | 2 pass | 3 walk B | 4 pass | 5 attack`
(read by `Content/Projectiles/AtronachMinion.cs`; the frame height is the texture height / 6).

Sizes in art pixels (screen pixels are 2x): Flame 44 tall, Frost 52 (Skyrim's frost atronach is a big
melee construct), Storm 48 (floating rocks). Tune `CREATURES` in the script.

## Conjurer (town NPC)

Drawn with the game's own player renderer (a dressed up dummy `Player`, the Mannequin trick), modelled
on Phinis Gestor, the College of Winterhold conjuration master who sells the tomes in Skyrim: bald, pale,
"Coat" clothing style in slate blue over a brown red shirt, pale boots (`Content/Visuals/ConjurerLook.cs`).
The sheet (25 frames, Rijam's Town NPC layout) and head are exported in game with `/atronachtest npc`
(Journey character), the community "Player Renderer" helper built into the mod; `/atronachtest hairsm`
renders every hair style to choose from. Files land in `Documents/My Games/Terraria/tModLoader/
ConjurationExport/`; copy `Conjurer.png` and `Conjurer_Head.png` into `Content/NPCs/`.

## Tomes

Skyrim's Conjuration spell tome (UESP icon `SR-icon-book-SpellTomeConjuration`): purple leather, a
border of silver rings, the Conjuration sign. Drawn by `tools/make_tomes.py`; a gem on the spine tells
the three apart (fire red, frost cyan, storm violet).

## Hand editing

`art/` holds every sprite at 1x art pixels, one file per frame; edit there and run
`python tools/art_folder.py pack` (see `art/README.md`).

## Pitfalls found on the way (2026-11-10)

- Always start from reference images: two looks invented from memory were rejected.
- Generated art without a reference and art made of player parts did not read as the Skyrim creatures.
- In hjson a value that starts with `{` (like `{NPCName} ...`) must be quoted, or the mod fails to load
  (and tModLoader disables it).
- The game's sprite batch maps to the back buffer: export one frame at a time on a screen sized target;
  begin in `Immediate` mode so armor dyes apply; render one throwaway frame first.
- `isDisplayDollOrInanimate` draws mannequin plastic; `DrawPlayerHead` uses the map scale.
- A Pillow made `icon_small.png` broke the packer, so the mod has none.
