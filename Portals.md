## ✅ Portal Travel System – Implementation Checklist

### 🚪 1. Portal Entry Trigger
- [ ] Create a `Portal` prefab with metadata: `targetSpaceId`, `targetServer`, `targetSpawnPoint`
- [ ] Detect when the player enters the portal collider
- [ ] Fire `PortalEntered` event with relevant portal data

---

### 🖼️ 2. Loading Screen & Info
- [ ] Display loading screen UI with:
  - [ ] Target space name
  - [ ] Thumbnail or artwork
  - [ ] Optional metadata (player count, lore, etc.)
- [ ] Disable player input & pause game state

---

### 🌐 3. Background Asset Download
- [ ] Begin downloading space JSON (layout, spawn points, assets)
- [ ] Begin downloading required models and textures
- [ ] Cache new space info for transition use

---

### 🧭 4. Player Confirmation
- [ ] Show confirmation prompt: “Enter *Space Name*?”
- [ ] Display buttons: ✅ Enter, ❌ Cancel
- [ ] Allow player to cancel and resume previous state

---

### 🧹 5. Cleanup Current Space
- [ ] Despawn all remote players
- [ ] Unsubscribe from `EventBus` events
- [ ] Clear player dictionary or remote state
- [ ] Close existing TCP connection (if switching servers)

---

### 🔌 6. Connect to Target Space
- [ ] If same server: send `ChangeSpace` message
- [ ] If different server:
  - [ ] Open new TCP connection
  - [ ] Send `InitMessage` with `id` + new `space_id`
  - [ ] Await `Spawn` and environment data

---

### 🗺️ 7. Load Target Scene
- [ ] Load new Unity scene (static or via Addressables)
- [ ] Apply JSON to reconstruct space if dynamic
- [ ] Move `localPlayer` to designated spawn point

---

### 🎮 8. Resume Gameplay
- [ ] Re-enable input, physics, camera, and events
- [ ] Begin sending/receiving movement and chat again

---

### 🧠 Optional Enhancements
- [ ] Fast travel history (back to known spaces)
- [ ] Space entry validation (lock/unlock by quest, item, faction, etc.)
- [ ] Cross-space chat or notifications
- [ ] Entry cutscene or lore narration system
- [ ] Debug logging for all transitions
