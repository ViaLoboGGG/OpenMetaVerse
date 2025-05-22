## 🌐 Project Vision — The 3D Web for Everyone

This project isn't just about technology—it’s about **redefining the internet**.

We envision a **free, open-source metaverse** where:
- Anyone can **host** their own public or private space
- Anyone can **join** a shared 3D world simply by entering a link
- Entire environments are loaded **on-demand** using open standards like JSON and glTF
- Users, artists, developers, and organizations can build experiences that are **interconnected**, **decentralized**, and **persistently accessible**

This is the natural evolution of the internet:  
### → The **WWW in 3D** — or as we call it, **W3D**.

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
```

## 🚦 Runtime Flow

When a player joins a space:

1. The client fetches the JSON manifest from a public URL.
2. For each object defined:
   - If the model URL is not cached, it is downloaded.
   - The `.gltf` or `.glb` is parsed and instantiated.
   - Object transform is applied (position, rotation, scale).
   - Whitelisted behaviors are attached (e.g., rotator, clickable).
   - Optional events are registered and bound via an internal event bus.
3. All scene objects are composed in real-time with modular, safe logic.

## 🌐 Model Hosting & Caching

- **Open Hosting**: Models can be served from any URL—GitHub Pages, IPFS, S3, or your own domain.
- **Caching**:
  - Models are cached by URL and version locally using Unity's persistent storage.
  - If a model has already been downloaded and versioned, it won’t be re-downloaded.
- **Versioning**:
  - The manifest can include a `version` tag for each model to allow content invalidation or updates.
  - This supports scene evolution without manual cache clearing.

## 🔐 Security Design

This project prioritizes safety, especially for user-generated content:

- ✅ No custom scripts are executed from downloaded JSON or glTF files.
- ✅ Only predefined, whitelisted Unity behaviors are attachable via JSON.
- ✅ Events are limited to known-safe actions like `PlaySound`, `SetActive`, etc.
- ✅ glTF files are parsed strictly as 3D data (meshes, materials, animations)—never logic.
- ✅ All remote data is sandboxed, with strict validation before instantiation.

## 📀 Exporting From Unity (WIP)

We are building tools to let creators easily generate JSON + glTF content:

- 🔧 A Unity Editor tool to export selected GameObjects and their transforms
- 🧩 Auto-generation of scene JSON, including behaviors and event bindings
- 📦 Support for generating `.gltf` files from Unity meshes (via GLTFUtility or UnityGLTF)
- 💾 Metadata tagging in the Inspector to bind behavior names

This will allow creators to "build their world in Unity, export it to JSON, and host it anywhere."

## 🧚 Planned Features

- ✅ JSON + glTF-based scene definition system
- ✅ Runtime caching of models by URL/version
- ✅ Secure behavior attachment system (no code execution)
- ✅ Modular event-action system (event bus)
- ⏳ Unity-to-JSON export tool
- ⏳ Visual editor for event/behavior binding
- ⏳ Multi-scene world navigation
- ⏳ Authentication layer for private spaces
- ⏳ Portal objects to jump between 3D scenes like hyperlinks
## 📄 License

This project is open-source and MIT licensed.

---

## 💬 Contact

Interested in collaborating, using this system, or deploying your own metaverse node?

- GitHub Discussions (Coming Soon)
- Community Discord (Coming Soon)
- Email: [YourEmail@example.com]

---

## 🧠 Inspiration

Inspired by:
- The open architecture of the World Wide Web
- 3D game engines and event-driven programming
- glTF and open 3D standards
- The promise of a decentralized metaverse that *anyone* can build on

---

Let’s build **W3D**—the 3D web that belongs to everyone.
