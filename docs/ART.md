# Art guide

All characters are drawn with **Terraria's own player renderer**, the trick the vanilla Mannequin uses:
a dummy `Player` dressed with vanilla items and colours. That gives the real proportions, outline,
palette, walk cycle and arm poses of the game, and the mod ships no copied art.

| What | How it is drawn | Files |
|---|---|---|
| Fire Atronach (minion) | live, every frame, `Content/Visuals/AtronachLook.cs` | `FireAtronachMinion.png` is a transparent 20x42 stand in |
| Conjurer (town NPC) | sheet exported from `Content/Visuals/ConjurerLook.cs` | `Content/NPCs/Conjurer.png` (40x1450) and `Conjurer_Head.png` |
| Buff icon, mod icon | exported head / full figure of the Atronach | `Content/Buffs/FireAtronachBuff.png`, `icon.png` (80x80) |
| Tome | the vanilla Spell Tome texture with a fire tint (`Texture` override) | none |

## The looks

- **Fire Atronach**: female, fire orange skin and clothes, spiky hair 23 in flame yellow, **Molten
  Breastplate** dyed with **Flame and Black Dye** (black plates with moving flames, like the atronach
  of the games that inspired the mod). Fire dust rises from the hair, it lights its surroundings, and it
  raises then stretches the arm toward the target when it throws (composite arm).
- **Conjurer**: old white haired man with ember eyes, **Wizard Hat** dyed Flame and Black, **Ruby Robe**.
  Deliberately not purple, so he does not look like the vanilla Wizard.

Change a look by editing those two files; nothing else needs redrawing for the Atronach.

## Exporting sheets and icons (the "Player Renderer" helper, built in)

The community tool for town NPC sheets is the *Player Renderer* mod (renders a player into a town NPC
sheet). The mod has the same thing as playtest commands, Journey mode characters only:

```
/atronachtest hairs   contact sheet of the Atronach with every hair style (to pick one)
/atronachtest icons   FireAtronachBuff.png (head and shoulders) and atronach_full.png
/atronachtest npc     Conjurer.png (25 frames) and Conjurer_Head.png
```

Files land in `Documents/My Games/Terraria/tModLoader/ConjurationExport/`. Copy them into `Content/`,
rebuild, and run `python tools/check_art.py`. `icon.png` is the full Atronach doubled on a dark ember
background (any image editor works; a Pillow made `icon_small.png` broke the packer, so the mod has none).

Town NPC sheet layout (Rijam's Town NPC guide): 0 idle, 1 jump, 2-15 walk, 16-17 raised arm, 18 sit,
19 talk, 20 blink, 21-24 throw. Each frame is a player body/legs frame pair (`NpcFrameMap`).

## Pitfalls found on the way (2026-11-10)

- `isDisplayDollOrInanimate` makes the renderer draw mannequin plastic and ignore skin and hair colours.
- Hair 15 is bald. Use the hairs contact sheet.
- The sprite batch maps to the back buffer: render on a screen sized target and crop, one frame at a
  time (a 1450 px sheet does not fit the screen and came out stretched and black).
- Begin the batch in `Immediate` mode or armor dyes are not applied; the first render after start comes
  out scaled, so render one throwaway frame first.
- `DrawPlayerHead` uses the map scale; draw the full player and crop instead.
- Earlier attempts (hand pixel art in code, then ComfyUI with SDXL + Pixel Art XL and Qwen-Image-Edit
  shrunk to the 2x2 grid) were rejected by the owner: generated art never read as Terraria. Starting from
  the game's own renderer does.
