# Balance notes

Starting numbers for the Fire Atronach, to be tuned by playing. Reference is vanilla pre-Hardmode
summons around Hellstone tier (Imp Staff: 24 damage, 10 mana, hellstone-era).

| Stat | Value | Where |
|---|---|---|
| Summon damage per fireball | 22 | `Item.damage` in `TomeConjureFireAtronach.cs` |
| Mana to summon | 12 | `Item.mana` |
| Minion slots | 1 | `Projectile.minionSlots` |
| Attack | vanilla Imp Fireball (speed 11, pierces, sets On Fire!), one every 68 ticks (Imp's rate) | `AttackCooldown`, `FireballSpeed` |
| Shoot range | 520 px from the minion; looks for targets 700 px around the owner | `ShootRange`, `DetectRange` |
| Walk / run speed | 3.6 / 8 (runs when more than 240 px from its spot) | `WalkSpeed`, `RunSpeed` |
| Conjurer | moves in after Eye of Cthulhu; sells the tome after Skeletron; 250 life, throws Balls of Fire (20 dmg) | `Content/NPCs/Conjurer.cs` |
| Reappears next to the owner | over 1000 px away, or stuck 2 s | `TeleportDistance`, `StuckTimer` |
| Price when sold | 2 gold | `Item.buyPrice` |

Why this is a first guess: a ranged minion that never takes contact damage and cannot die is safe, so it
should be a bit weaker per slot than a melee minion. It throws the Imp's fireball at the Imp's rate, so at 22 damage
it is a slightly weaker Imp Staff (24). Unlike the Imp it walks, so it cannot follow you while you fly: it
reappears beside you instead.

## Checklist when testing in single player

- Summon, check the buff icon appears and the minion follows you.
- Two tomes: two minions, they stand at different spots behind you (`minionPos`).
- Walls and steps: it walks up 1 tile steps and hops over walls; drops through platforms when you are below.
- Fly or fall far away: it reappears next to you with a fire burst.
- It shoots only enemies it can see, prefers the one you right-click (target).
- Death or leaving the world removes it; re-entering does not duplicate it.
- Multiplayer: host a local server and join with a second client; the minion must shoot on both screens.
- Compare DPS against the Imp Staff on the same enemy.
