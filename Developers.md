# 🛠️ Developers Guide

This guide is for developers who want to contribute to the Open Metaverse Scene Loader.

---

## 🧰 Tech Stack

- **Unity (LTS version recommended)**
- **C#**
- **glTF / glb** via Khronos Group standard
- **GLTFUtility** or **UnityGLTF** for glTF support

---

## 📁 Project Structure

/
├── Assets/
│ └── SceneLoader/ # Core runtime loader scripts
│ └── EditorTools/ # Exporter, validator, and tooling
│ └── Examples/ # Sample scenes and JSON manifests
├── Documentation/ # Extended setup and system design
└── README.md


---

## 🚀 Getting Started

> Clone and open in Unity (details will be filled in as development progresses)

1. Open Unity Hub
2. Add the project folder
3. Install required packages (TBD)
4. Run the example scene

---

## 🧪 Testing & Debugging

- Test new scenes with `scene.json` + glTF URLs
- Use built-in logging tools to trace manifest errors
- A sandbox loader tool is in development

---

## 👥 Contribution Guidelines

- Use PRs with descriptive messages
- Write modular, event-driven code
- Include comments and follow C# Unity conventions
- Avoid hardcoding behaviors (register them via manifest actions instead)

---

## 📦 Related Tools

- [glTF Validator](https://github.khronos.org/glTF-Validator/)
- [GLTFUtility](https://github.com/Siccity/GLTFUtility)
- [UnityGLTF](https://github.com/KhronosGroup/UnityGLTF)
