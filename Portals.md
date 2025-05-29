## ✅ Portal Travel System – Implementation Checklist

### 🚪 1. Portal Entry Trigger
- [ ] Detect when the player enters a portal trigger collider
- [ ] Read portal metadata from scene JSON (e.g., `targetSpaceId`, `targetServer`, `targetSpawnPoint`)
- [ ] Fire `PortalEntered` event with extracted portal data

---

### 🖼️ 2. Loading Screen & Info
- [ ] Display a loading UI with:
  - [ ] Target space name (from portal metadata)
  - [ ] Image or description (optional)
- [ ] Disable player movement and interactions

---

### 🌐 3. Background Asset Download
- [ ] Begin downloading target space JSON file
- [ ] Queue any required models, textures, or other assets for the new space
- [ ] Parse JSON and prepare scene instantiation data

---

### 🧭 4. Player Confirmation
- [ ] Show confirmation prompt: “Enter *Target Space Name*?”
- [ ] Provide buttons: ✅ Confirm, ❌ Cancel
- [ ] If canceled:
  - [ ] Hide loading screen
  - [ ] Resume current scene state

---

### 🧹 5. Cleanup Current Space
- [ ] Despawn all remote players
- [ ] Unsubscribe from relevant `EventBus` listeners
- [ ] Clear or reset in-memory player/entity state
- [ ] Close existing TCP connection (if connecting to a different server)

---

### 🔌 6. Connect to Target Space
- [ ] If target is on same server: send a `ChangeSpace` message
- [ ] If target is a different server:
  - [ ] Open new TCP connection to `targetServer`
  - [ ] Send `InitMessage` with `playerId` and `targetSpaceId`
  - [ ] Await initial scene setup and spawn data

---

### 🗺️ 7. Load Target Scene
- [ ] Load or generate new Unity scene from space JSON
- [ ] Instantiate scene elements and spawn point(s)
- [ ] Move `localPlayer` to designated entry position (`targetSpawnPoint`)

---

### 🎮 8. Resume Gameplay
- [ ] Re-enable player input and gameplay systems
- [ ] Resume player syncing, chat, and interactions
- [ ] Subscribe to necessary network event streams again

---

### 🧠 Optional Enhancements
- [ ] Travel history (return to last portal used)
- [ ] Permissions or gating logic (e.g., require key item)
- [ ] Entry visual/sound effects
- [ ] Debug logs for all major steps
- [ ] Timeout or fallback if connection/scene load fails
