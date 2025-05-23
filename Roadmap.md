# 🌐 W3D Project Roadmap

Welcome to the core roadmap for the **W3D Project** — a decentralized, user-created 3D world platform. This document outlines the planned phases of development, technical priorities, and key milestones.

---

## ✅ Phase 1: Core Functionality (Engine First)
**Goal:** Enable real-time loading and traversal of player-made spaces.

### Milestones:
- [ ] Load `space.json` scenes at runtime (local file or remote URL)
- [ ] Implement basic movement and interaction in loaded spaces
- [ ] Add runtime object instancing (spawn objects from metadata)
- [ ] Create system for portals or links between spaces
- [ ] Implement basic "space registry" (local list of available spaces)
- [ ] Support dynamic unloading and memory management for space transitions
- [ ] Add simple UI for navigating between spaces

---

## 🧪 Phase 2: Multiplayer & Networking
**Goal:** Enable real-time shared experiences in the same space.

### Milestones:
- [ ] Implement WebSocket-based multiplayer server (C# or Rust)
- [ ] Support movement sync, chat, and player presence
- [ ] Add simple authoritative object ownership model
- [ ] Implement client connection flow: connect to server → receive `space.json` → enter world
- [ ] Enable client fallback: preload `space.json` first, then sync with server
- [ ] Support two server modes:
  - Dedicated: hosts one or more predefined spaces
  - Standby: loads spaces dynamically on demand
- [ ] Enable server to advertise to central registry (IP, status, hosted spaces)
- [ ] Allow server to fetch and load any space via space ID or remote URL
- [ ] Define protocol for server-to-client space transfer (binary or JSON)
- [ ] Explore user-hosted discovery via registry, broadcast, or relays

---

## 🌐 Phase 3: Website + Community Portal
**Goal:** Build public-facing web presence for hype, updates, and user access.

### Milestones:
- [ ] Launch basic landing page (w3d.world?)
- [ ] Add email signup / early interest capture
- [ ] Publish dev blog with project updates and media
- [ ] Show off space screenshots, portal demos, and roadmap
- [ ] Add Discord or community link

---

## 🔗 Phase 4: Blockchain Integration (Optional)
**Goal:** Enable ownership, signatures, and distributed storage.

### Milestones:
- [ ] Design minimal wallet integration (Metamask or WalletConnect)
- [ ] Let users digitally sign their space metadata (e.g. `space.json`)
- [ ] Store signed spaces on IPFS or Arweave
- [ ] Implement ownership check when visiting / editing spaces
- [ ] Optional: mint spaces as NFTs (modular, opt-in)

---

## 🧠 Phase 5: Extensibility, Mods, & Ecosystem
**Goal:** Empower users to create new mechanics, assets, and logic.

### Milestones:
- [ ] Plugin system for space-specific logic or scripts
- [ ] Custom assets loading (models, textures)
- [ ] Cross-space quest or event hooks
- [ ] Economy / inventory framework (optional)

---

## 📌 Notes & Priorities
- All features are designed to work without central servers
- Web compatibility is a priority (Unity WebGL + WebSocket preferred)
- Blockchain integration is modular and not required to use core functionality

---

## 🔄 Current Focus: Core Functionality
Start by enabling dynamic space loading and traversal. This sets the foundation for everything else.

> "If users can build a world, visit a world, and share a world — we've already changed the game."
