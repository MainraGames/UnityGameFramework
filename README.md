# 🎮 Mainra Framework - Unity Game Starter Template

[![Unity](https://img.shields.io/badge/Unity-6000.x-black?logo=unity)](https://unity.com/)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](https://www.gnu.org/licenses/gpl-3.0)
[![VContainer](https://img.shields.io/badge/DI-VContainer-orange)](https://github.com/hadashiA/VContainer)

Framework Unity siap pakai dengan arsitektur Clean Architecture, Dependency Injection, dan kumpulan plugin terbaik untuk memulai project game baru dengan cepat.

---

## 🚀 Quick Start

### Clone Repository
```bash
git clone https://github.com/YOUR_USERNAME/FrameworkUnityGame.git
cd FrameworkUnityGame
```

### Atau Download ZIP
1. Klik tombol **Code** → **Download ZIP**
2. Extract dan buka dengan Unity Hub
3. Rename project sesuai kebutuhan

### Requirements
- **Unity 6000.x** (Unity 6)
- **Universal Render Pipeline (URP)**

---

## 📁 Struktur Project

```
Assets/
├── _Game/                    ← Folder utama game kamu
│   ├── Animations/           ← Animation clips & controllers
│   ├── Audio/                ← Audio files
│   ├── Config/               ← Configuration files
│   ├── Prefabs/              ← Game prefabs
│   ├── Scenes/               ← Game scenes
│   ├── ScriptableObjects/    ← Data assets
│   ├── Scripts/              ← Source code (lihat arsitektur di bawah)
│   ├── Settings/             ← URP & project settings
│   └── Sprites/              ← 2D graphics
│
└── Plugins/                  ← Third-party plugins (sudah termasuk)
```

---

## 🏗️ Arsitektur

Project ini menggunakan **Clean Architecture / Layered Architecture** yang dirancang khusus untuk Unity:

```
Scripts/
├── 0_Infrastructure/    → External concerns (Database, APIs, SDKs)
├── 1_Core/              → Domain/Business logic (Pure C#)
├── 2_Application/       → Use cases, Services, Managers
├── 3_Presentation/      → UI, Input, MonoBehaviours
├── GameConfiguration/   → ScriptableObjects, Config files
├── LifetimeScope/       → Dependency Injection setup (VContainer)
├── Utility/             → Helper classes, Extensions
└── Test/                → Unit tests
```

### Dependency Flow
```
┌─────────────────────────────────────────────────────────────┐
│                    3_Presentation                           │
│              (UI, Input, Views, MonoBehaviours)             │
├─────────────────────────────────────────────────────────────┤
│                    2_Application                            │
│           (Services, Managers, Use Cases, Factories)        │
├─────────────────────────────────────────────────────────────┤
│                       1_Core                                │
│     (Domain Models, Interfaces, Enums, Events, Constants)   │
├─────────────────────────────────────────────────────────────┤
│                   0_Infrastructure                          │
│        (Database, File I/O, External APIs, SDKs)            │
└─────────────────────────────────────────────────────────────┘
```

> 📖 Dokumentasi lengkap: [`Assets/_Game/Scripts/ARCHITECTURE.md`](Assets/_Game/Scripts/ARCHITECTURE.md)

---

## 📦 Plugin yang Termasuk

### Runtime
| Plugin | Deskripsi |
|--------|-----------|
| **VContainer** | Lightweight Dependency Injection container |
| **UniTask** | High-performance async/await untuk Unity |
| **PrimeTween** | Modern tween library berkinerja tinggi |
| **DOTween** | Tween engine populer |
| **BroAudio** | Audio management system |
| **Odin Inspector** | Powerful serialization & inspector |
| **Lean GUI** | Modern GUI components |
| **Lean Transition** | Visual transition system |
| **Save Manager** | Game save/load system |

### Editor Only
| Plugin | Deskripsi |
|--------|-----------|
| **Odin Inspector** | Custom inspector & validators |
| **Ultimate Editor Enhancer** | Editor workflow improvements |
| **Super Unity Build** | Build automation |
| **Custom Toolbar** | Quick action toolbar |
| **Clipboard Plus** | Enhanced clipboard |
| **Wingman** | Component copy utility |
| **Smooth Scene Camera** | Better scene navigation |
| **Player Prefs Editor** | View/edit PlayerPrefs |
| **Memo** | In-editor notes |

> 📋 Detail lengkap: [`readmePlugin.md`](readmePlugin.md)

---

## 🎯 Fitur Framework

- ✅ **Clean Architecture** - Separation of concerns yang jelas
- ✅ **Dependency Injection** - VContainer terintegrasi
- ✅ **Async/Await** - UniTask untuk operasi asynchronous
- ✅ **Audio System** - BroAudio siap pakai
- ✅ **Animation** - PrimeTween & DOTween
- ✅ **Save System** - Carter Games Save Manager
- ✅ **Editor Tools** - Berbagai tool untuk produktivitas
- ✅ **Coding Standards** - Dokumentasi standar kode
- ✅ **URP Ready** - Universal Render Pipeline

---

## 📖 Dokumentasi

| File | Deskripsi |
|------|-----------|
| [`ARCHITECTURE.md`](Assets/_Game/Scripts/ARCHITECTURE.md) | Arsitektur & dependency rules |
| [`CODING_STANDARDS.md`](Assets/_Game/Scripts/CODING_STANDARDS.md) | Standar penulisan kode |
| [`readmePlugin.md`](readmePlugin.md) | Daftar plugin & deskripsi |

---

## 🛠️ Cara Menggunakan

### 1. Setup VContainer
Buat `LifetimeScope` untuk scene:
```csharp
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Register services
        builder.Register<IPlayerService, PlayerService>(Lifetime.Singleton);
        builder.Register<IAudioService, AudioService>(Lifetime.Singleton);
        
        // Register MonoBehaviour
        builder.RegisterComponentInHierarchy<PlayerController>();
    }
}
```

### 2. Inject Dependencies
```csharp
public class PlayerController : MonoBehaviour
{
    [Inject] private readonly IPlayerService _playerService;
    [Inject] private readonly IAudioService _audioService;
    
    void Start()
    {
        _playerService.Initialize();
    }
}
```

### 3. Gunakan UniTask
```csharp
public async UniTaskVoid LoadGameAsync()
{
    await UniTask.Delay(1000);
    await SceneManager.LoadSceneAsync("GameScene");
}
```

---

## 📝 Memulai Project Baru

1. **Clone/Download** repository ini
2. **Rename** folder project sesuai nama game
3. **Hapus** file `.git` jika ingin repository baru
4. **Update** `ProjectSettings/ProjectSettings.asset` untuk nama product
5. **Mulai coding** di folder `Assets/_Game/`

---

## 🤝 Kontribusi

Kontribusi sangat diterima! Silakan buat Pull Request atau Issue.

---

## 📄 Lisensi

Project ini dilisensikan di bawah [GNU General Public License v3.0](LICENSE).

---

## 🙏 Credits

Framework ini menggunakan berbagai plugin open-source dan asset dari komunitas Unity. Terima kasih kepada semua developer yang telah berkontribusi.

---

*Made with ❤️ for Unity Developers*
