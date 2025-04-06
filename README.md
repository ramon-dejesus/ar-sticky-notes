# 📌 AR Sticky Notes

Augmented Reality (AR) mobile app that allows users to place and manage virtual sticky notes in their real-world environment using ARCore/ARKit via Unity. Notes are anchored in physical space and persist across sessions.

---

## 🔕️ Project Timeline
**Total Duration:** 2 Weeks  
**Team:**
- 👨‍💻 Ramon (Project Management, Architecture, GitHub Setup, Testing, Releases, Deployments)
- 👨‍💻 Allan (AR Logic, Anchoring, Persistence)
- 👨‍💻 Matt (Full Stack Integration, Prefab Logic, UI Implementation)
- 🎨 Gabe (Wireframes, Visual Design, Branding)

---

## 🚀 Milestones & Workflow

### ✅ Milestone 1: Core Setup (Day 1–2)
- Set up GitHub repository, and branching strategy *Ramon*
- Set up valid `.gitignore` Unity git needs *(Matt)*
- Initialize Unity Project with AR Foundation *(Allan - with Matt assisting on input validation and project scaffolding)*
- Create low-fi wireframes, color palette *(Gabe)*

### 🔧 Milestone 2: Anchoring & UI (Day 2–5)
- Implement plane detection + anchor placement *(Allan)*
- Create sticky note prefab + interaction flow *(Matt)*
- Finalize mockups & branding guidelines *(Gabe)*

### 📀 Milestone 3: Persistence & Management (Day 5–8)
- Store and reload notes via local persistence *(Allan)*
- Build “My Notes” UI list + delete flow *(Matt)*
- Apply final visual polish to UI + prefab *(Gabe)*

### 🧪 Milestone 4: QA & Release (Day 9–10)
- Full QA on Android/iOS *(Ramon)*
- Bug fixes *(Allan & Matt)*
- Create app icon, finalize UI assets *(Gabe)*
- Build & publish on TestFlight / Google Internal *(Ramon)*

---

## 📁 Repository Structure !!ask Allan & Matt>
```
📆ARStickyNotes/
👤 Assets/
🔜 ARFoundation/       # Core AR Foundation setup
📈 Prefabs/            # Sticky note prefab
📃 Scripts/            # C# scripts (AR logic, UI, persistence)
📋 UI/                 # Buttons, canvases, text inputs
📄 Docs/                # Wireframes, visual assets
📄 Builds/              # Android/iOS test builds
README.md              # Project overview
LICENSE.md             # Open-source license (MIT)
.gitignore             # Unity-specific ignores
ProjectSettings/       # Unity project configuration
```

---

## 🛠️ Tech Stack
- **Unity** (2022.3+ LTS)
- **AR Foundation** (ARCore & ARKit backend)
- **C#** for development
- **GitHub Projects** for task tracking
- **Figma** for UI/UX wireframes (by Gabe)

---

## 🎯 MVP Goals
- Place sticky notes via AR on detected surfaces
- Store note location + text for future sessions
- Manage notes (view, delete)
- Responsive and polished UI
- Release builds for iOS & Android

---

## 🧪 Testing Devices
- Android: !!ask devs>
- iOS: !!ask devs>

---

## 📆 Installation (For Developers) !!ask Matt>
```bash
# Clone the repo
$ git clone https://github.com/your-org/ar-sticky-notes.git
$ cd ar-sticky-notes

# Open the Unity project in Unity Hub (2022.3+ LTS)
```

---

## 👥 Contribution Guidelines
- Use feature branches (`feature/your-name-description`)
- Submit Pull Requests to `dev`, never `main`
- Include test notes and screenshots in PRs

---

## 📃 License
MIT – See [LICENSE.md](./LICENSE.md)
