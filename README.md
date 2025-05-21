# 🪐 Open Metaverse Scene Loader

An open-source Unity framework for loading 3D scenes at runtime from external JSON and glTF definitions. Designed for decentralization, user-generated content, and lightweight client-side rendering with secure, sandboxed behaviors.

---

## 🌍 Project Vision

The goal is to create a runtime scene loading system for Unity that allows users to:

- Host and serve 3D scenes as open JSON + glTF documents
- Define scene behavior using a whitelist of safe, predefined scripts
- Share and explore user-made spaces with no recompilation or Unity knowledge
- Cache all models and content locally for fast revisits
- Maintain a secure runtime that never executes untrusted code

This project is ideal for:
- Metaverse-style platforms
- User-generated worlds
- Decentralized content hosting
- Lightweight multiplayer lobbies or exploration spaces

---

## 🧱 How It Works

### 1. Scene JSON Manifest

Scenes are defined using a JSON file that includes:
- Object name and transform
- Public URL to the associated `.gltf` or `.glb` model
- A list of whitelisted behaviors/scripts to attach
- Optional event-action bindings for interactions

```json
{
  "objects": [
    {
      "name": "MagicChair",
      "model": "https://example.com/models/chair1.gltf",
      "position": [0, 0, 0],
      "rotation": [0, 180, 0],
      "scripts": ["SitOnInteract"],
      "events": {
        "onInteract": [
          { "action": "PlaySound", "params": { "clip": "chair_squeak" } }
        ]
      }
    }
  ],
  "formatVersion": 1
}
