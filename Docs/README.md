# 📌 AR Sticky Notes

Augmented Reality (AR) mobile app that allows users to place and manage virtual sticky notes in their real-world environment using ARCore/ARKit via Unity. Notes are anchored in physical space and persist across sessions.

---

## 🔕️ Project Timeline

**Team:**

- 👨‍💻 Ramon (Project Management, Architecture, GitHub Setup, Testing, Releases, Deployments, Discord Bot)
- 👨‍💻 Allan D (AR Logic, Anchoring, Persistence)
- 🎨 Gabe (Wireframes, Visual Design, Branding, UI Implementation)

---

## 🚀 Milestones & Workflow

### ✅ Milestone 1: Core Setup

- Set up GitHub repository, branching strategy, and discord bot Ramon
- Set up valid `.gitignore` Unity git needs _(Ramon)_
- Initialize Unity Project with AR Foundation _(Allan - with Ramon assisting on input validation and project scaffolding)_
- Create mockups, wireframes and color palette _(Ramon & Gabe)_

### 🔧 Milestone 2: Anchoring & UI

- Implement wall plane detection + anchor placement _(Allan)_
- Create sticky note prefab + interaction flow _(Gabe)_
- Finalize mockups & branding guidelines _(Gabe)_

### 📀 Milestone 3: Persistence & Management

- Store and reload notes via local persistence _(Allan)_
- Build “My Notes” UI list + delete flow _(Gabe)_
- Apply final visual polish to UI + prefab _(Gabe)_

### 🧪 Milestone 4: QA & Release

- Full QA on Android/iOS _(Ramon)_
- Bug fixes _(Allan, Gabe & Ramon)_
- Create app icon, finalize UI assets _(Gabe)_
- Build & publish on TestFlight / Google Internal _(Ramon)_

---

## 📁 Repository Structure

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
