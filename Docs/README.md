# 📌 AR Sticky Notes

Augmented Reality (AR) mobile app that allows users to place and manage virtual sticky notes in their real-world environment using ARCore/ARKit via Unity. Notes are anchored in physical space and persist across sessions.

---

## 🔕️ Project Timeline
**Total Duration:** 2 Weeks  
**Team:**
- 👨‍💻 Ramon (Project Management, Architecture, GitHub Setup, Testing, Releases, Deployments, Discord Bot)
- 👨‍💻 Allan D (AR Logic, Anchoring, Persistence)
- 🎨 Gabe (Wireframes, Visual Design, Branding, UI Implementation)

---

## 🚀 Milestones & Workflow

### ✅ Milestone 1: Core Setup (Day 1–2)
- Set up GitHub repository, branching strategy, and discord bot *Ramon*
- Set up valid `.gitignore` Unity git needs *(Ramon)*
- Initialize Unity Project with AR Foundation *(Allan - with Ramon assisting on input validation and project scaffolding)*
- Create mockups, wireframes and color palette  *(Ramon & Gabe)*

### 🔧 Milestone 2: Anchoring & UI (Day 2–5)
- Implement wall plane detection + anchor placement *(Allan)*
- Create sticky note prefab + interaction flow *(Gabe)*
- Finalize mockups & branding guidelines *(Gabe)*

### 📀 Milestone 3: Persistence & Management (Day 5–8)
- Store and reload notes via local persistence *(Allan)*
- Build “My Notes” UI list + delete flow *(Gabe)*
- Apply final visual polish to UI + prefab *(Gabe)*

### 🧪 Milestone 4: QA & Release (Day 9–10)
- Full QA on Android/iOS *(Ramon)*
- Bug fixes *(Allan, Gabe & Ramon)*
- Create app icon, finalize UI assets *(Gabe)*
- Build & publish on TestFlight / Google Internal *(Ramon)*

---

## 📁 Repository Structure !!ask Allan & Ramon>
```
📆ARStickyNotes/
📄 AR-Unity-Setup/       # Unity Root folder
👤 AR-Unity-Setup/Assets/
📈 AR-Unity-Setup/Prefabs/            # Sticky note prefab
📄 AR-Unity-Setup/ProjectSettings/    # Unity project configuration
📃 AR-Unity-Setup/Scripts/            # C# scripts (AR logic, UI, persistence)
📋 AR-Unity-Setup/UI/                 # Buttons, canvases, text inputs
📄 AR-Unity-Setup/Builds/             # Android/iOS test builds
📄 Docs/               # Wireframes, visual assets
README.md              # Project overview
LICENSE.md             # Open-source license (MIT)
.gitignore             # Unity-specific ignores

```

---

## 🛠️ Tech Stack
- **Unity** (2022.3+ LTS)
- **AR Foundation** (ARCore & ARKit backend)
- **C#** for development
- **GitHub Projects** for code task tracking
- **Draw.io** for UI/UX wireframes (by Gabe)

---

## 🎯 MVP Goals
- Place sticky notes via AR on walls ONLY
- Store note location + text on app exists for future sessions
- Manage notes on location and on a list (view, delete)
- Responsive and polished UI
- Release builds for iOS & Android

---

## 🧪 Testing Devices
- Android: Pixel 9 pro-Android 15, Oneplus-Android 12
- iOS: iPhone 15

---

## 📆 Installation (For Developers) 
```bash
# Clone the repo
$ git clone https://github.com/ramon-dejesus/ar-sticky-notes.git
$ cd ar-sticky-notes

# Open the Unity project in Unity Hub (2022.3+ LTS)
```

---

## 👥 Contribution Guidelines
- Use feature branches (`feature/your-name-description`)
- Submit Pull Requests to `dev`, never `main`
- Include test notes, upload screenshots to PR, and demo submitted PR at weekly sessions before PR approval

---

## 📃 License
MIT – See [LICENSE.md](./LICENSE.md)
