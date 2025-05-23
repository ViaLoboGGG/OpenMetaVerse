# 🌌 W3D World Builder: Getting Started Guide

Welcome to the W3D World Builder! This guide helps you create and test your own virtual spaces using **Unity** and **GLTF** models. Whether you're designing a personal zone or a public hub, this step-by-step guide walks you through the setup and creation process.

---

## 🛠 Prerequisites

### 1. Install Unity

- Download Unity Hub: [https://unity.com/download](https://unity.com/download)
- Recommended Version: **Unity 2022.3 LTS** or newer
- Create a **3D Core** project

---

## 📦 Unity Setup

### 2. Add UnityGLTF Support

1. Open your Unity project
2. Install the `UnityGLTF` package:
   - Go to **Window → Package Manager**
   - Click the `+` button → **Add package from Git URL**
   - Paste:
     ```
     https://github.com/KhronosGroup/UnityGLTF.git
     ```
3. UnityGLTF will now appear under `Packages/UnityGLTF`

---

## 📁 Organize Your Workspace

- `Assets/Resources/Models/` → Store primitive models or prefabs here for local reuse
- `Assets/ExportedGLTF/` → Store GLTF models exported from FBX
- `Assets/Scenes/` → Save your Unity scenes
- `Assets/Editor/` → Contains import/export tools

---

## 🌍 Importing and Exporting a Space

### Import a Space

1. Open the Unity menu: `Tools → Import Space from JSON`
2. Select your `Space.json` file (this contains layout + metadata)
3. All objects will be loaded into the scene (including GLTFs if linked properly)

### Export a Space

1. Organize all your objects under a `GameObject` named **Root**
2. Attach the `ExportedSpaceData` component to Root
3. Open: `Tools → Export Space to JSON`
4. Set metadata, then click **Export**

---

## 🎮 Making the Space Playable

To turn your space into a working environment for players and events:

### ✨ Player Spawning

- **PlayerSpawner** prefab determines where players appear when the space loads
- Drop this anywhere in your scene where you want players to spawn

### 🎁 Loot Spawning

- **LootSpawner** objects act as placeholders
- Server decides what they spawn
- Place them near shelves, terrain, or inside buildings

### 🌀 Portals

- **Portal** objects act as teleporters to other spaces
- Configure the **target space name** via its component
- When a player touches a portal, they are taken to the linked space

---

## 🧰 Built-in Models

These are included in your project or accessible through remote GLTF libraries:

| Model Name       | Description                       |
|------------------|-----------------------------------|
| `PlayerSpawner`  | Player entry point                |
| `LootSpawner`    | Loot or object spawning point     |
| `Portal`         | Entry to another space            |
| `BillboardSign`  | Text or signage display object    |
| `NPC`            | Placeholder character mesh        |
| `BasicCrate`     | Simple interactive object         |

---

## 🧠 Built-in Scripts

### 🔄 Animation

| Script Name       | Purpose                            |
|-------------------|-------------------------------------|
| `SpinOnAxis`      | Spins object around Y-axis          |
| `PulseEffect`     | Grows/shrinks object in a loop      |

### 🖱 Interaction

| Script Name        | Purpose                               |
|--------------------|----------------------------------------|
| `Clickable`        | Triggers an event when clicked         |
| `HoverHighlight`   | Adds glow on hover (for VR/UI hints)   |

### 🌐 World Logic

| Script Name         | Purpose                                 |
|---------------------|------------------------------------------|
| `PortalTrigger`     | Teleports player to another space        |
| `SpawnOnStart`      | Spawns predefined prefab on load         |
| `DespawnAfterTime`  | Removes object after X seconds           |

> ℹ You can extend any of these or register your own scripts using `scripts` in the export data.

---

## 🌐 Hosting & External Models

- You may reference external `.gltf` files stored **outside the Unity project**
- These can be local (e.g. `C:\Models\Spaceship.gltf`) or remote (`https://mysite.com/assets/spaceship.gltf`)
- Configure this in the `ExportedSpaceData` under:
  - **Base Model Path** (local fallback)
  - **Web Model Location** (remote fallback)

---

## ⚠ Limitations & Tips

- ❌ **GLTF models cannot be nested** under other GLTF models (yet)
- ✅ You **can nest primitives and GameObjects** however you want
- ✅ GLTFs can be nested **under primitives or empty GameObjects**
- Avoid naming different materials `Material.001` — exporter will auto-rename them

---

## ✅ Suggested Workflow

1. Convert your `.fbx` files to `.gltf` using the built-in `FBX to GLTF Exporter` (under `Tools`)
2. Import them to Unity to preview and place them
3. Organize objects in the hierarchy under `Root`
4. Export the scene as a `.json` file
5. Share your `.json` and GLTFs with others!

---

## 💬 Questions?

Join the W3D community to get help and share your creations.  
We're building the Ready Player One metaverse — together!

