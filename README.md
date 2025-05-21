# 🪐 Open Metaverse Scene Loader

An open-source Unity framework for loading 3D scenes at runtime from external JSON and glTF definitions. Designed for decentralization, user-generated content, and lightweight client-side rendering with secure, sandboxed behaviors.

---

## 🌐 Project Vision — The 3D Web for Everyone

This project isn't just about technology—it’s about **redefining the internet**.

We envision a **free, open-source metaverse** where:
- Anyone can **host** their own public or private space
- Anyone can **join** a shared 3D world simply by entering a link
- Entire environments are loaded **on-demand** using open standards like JSON and glTF
- Users, artists, developers, and organizations can build experiences that are **interconnected**, **decentralized**, and **persistently accessible**

This is the natural evolution of the internet:  
### → The **WWW in 3D** — or as we call it, **WWW3D**.

Like web pages, every virtual space can be:
- **Public** (open to all)
- **Private** (joinable only with a key)
- **Self-hosted** (your server, your rules)
- **Portable** (linkable, shareable, remixable)

No central company. No gatekeepers.  
Just an ecosystem of people and worlds—linked together like websites.

Our long-term goal is to create a 3D browsing experience that works like the web:  
Click a link → load the scene → explore → interact → move on.

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
