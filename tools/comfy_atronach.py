#!/usr/bin/env python3
"""Generates Fire Atronach concept sprites with the local ComfyUI (SDXL base + Pixel Art XL LoRA).

Needs the ComfyUI server on 127.0.0.1:8188 and its client at C:\\ComfyUIFiles\\scripts\\comfy_client.py.
Models: checkpoints/sd_xl_base_1.0, loras/pixel-art-xl (nerijs, OpenRAIL-M), vae/sdxl_vae_fp16_fix.

  python tools/comfy_atronach.py --count 6 --name v1
  python tools/comfy_atronach.py --count 4 --prompt "<extra words>" --seed 123
  python tools/comfy_atronach.py --tome --count 4 --name tome

Raw 1024 images land in C:\\ComfyUIFiles\\output\\conjuration\\<name>_*.png. Turning one into the
game sheet is tools/sprite_from_concept.py.
"""
import argparse
import random
import sys
from pathlib import Path

sys.path.insert(0, r"C:\ComfyUIFiles\scripts")
from comfy_client import run  # noqa: E402

CKPT = "sd_xl_base_1.0.safetensors"
LORA = "pixel-art-xl.safetensors"
VAE = "sdxl_vae_fp16_fix.safetensors"

BASE_PROMPT = (
    "pixel art, full body side view of a fire atronach, a female elemental spirit made of living flame, "
    "slender humanoid body of bright orange and yellow fire, long hair of rising flames, "
    "black obsidian armor plates on shoulders chest hips and forearms, curved horns, glowing white eyes, "
    "arms of fire, floating in the air, legs dissolve into a tail of flames, facing left, "
    "2d game sprite, dark outline, limited palette, centered, plain white background"
)
TOME_PROMPT = (
    "pixel art, game item icon of a closed spell tome, thick book bound in dark red leather with black iron corners, "
    "a glowing orange flame rune on the cover, small embers, centered, 2d game sprite, dark outline, limited palette, "
    "plain white background"
)
NEGATIVE = (
    "blurry, antialiased, smooth gradient, 3d render, realistic, photo, text, watermark, signature, "
    "multiple characters, cropped, cut off, frame, border, ground, scenery, nsfw, nude"
)


def build(prompt, seed, steps, cfg, lora_w, name):
    return {
        "ckpt": {"class_type": "CheckpointLoaderSimple", "inputs": {"ckpt_name": CKPT}},
        "vae": {"class_type": "VAELoader", "inputs": {"vae_name": VAE}},
        "lora": {"class_type": "LoraLoader", "inputs": {
            "model": ["ckpt", 0], "clip": ["ckpt", 1], "lora_name": LORA,
            "strength_model": lora_w, "strength_clip": lora_w}},
        "pos": {"class_type": "CLIPTextEncode", "inputs": {"clip": ["lora", 1], "text": prompt}},
        "neg": {"class_type": "CLIPTextEncode", "inputs": {"clip": ["lora", 1], "text": NEGATIVE}},
        "lat": {"class_type": "EmptyLatentImage", "inputs": {"width": 1024, "height": 1024, "batch_size": 1}},
        "ks": {"class_type": "KSampler", "inputs": {
            "model": ["lora", 0], "positive": ["pos", 0], "negative": ["neg", 0], "latent_image": ["lat", 0],
            "seed": seed, "steps": steps, "cfg": cfg, "sampler_name": "dpmpp_2m", "scheduler": "karras",
            "denoise": 1.0}},
        "dec": {"class_type": "VAEDecode", "inputs": {"samples": ["ks", 0], "vae": ["vae", 0]}},
        "save": {"class_type": "SaveImage", "inputs": {"images": ["dec", 0], "filename_prefix": f"conjuration/{name}"}},
    }


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--count", type=int, default=4)
    ap.add_argument("--name", default="atronach")
    ap.add_argument("--prompt", default="", help="extra words appended to the base prompt")
    ap.add_argument("--seed", type=int, default=None)
    ap.add_argument("--steps", type=int, default=30)
    ap.add_argument("--cfg", type=float, default=7.0)
    ap.add_argument("--lora", type=float, default=1.2)
    ap.add_argument("--tome", action="store_true", help="generate the tome item icon instead of the creature")
    a = ap.parse_args()
    prompt = (TOME_PROMPT if a.tome else BASE_PROMPT) + (", " + a.prompt if a.prompt else "")
    seed = a.seed if a.seed is not None else random.randint(1, 2**31)
    for i in range(a.count):
        for p in run(build(prompt, seed + i, a.steps, a.cfg, a.lora, f"{a.name}_s{seed + i}")):
            print(p)


if __name__ == "__main__":
    main()
