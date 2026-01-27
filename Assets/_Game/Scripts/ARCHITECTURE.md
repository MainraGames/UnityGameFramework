# 🏗️ Mainra Framework - Architecture Overview

## Deskripsi
Project ini menggunakan **Clean Architecture / Layered Architecture** yang disesuaikan untuk Unity Game Development. Arsitektur ini memisahkan kode berdasarkan tanggung jawab (separation of concerns) untuk meningkatkan maintainability, testability, dan scalability.

---

## 📚 Dokumentasi Terkait

| Dokumen | Deskripsi |
|---------|-----------|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | Overview arsitektur (dokumen ini) |
| [CODING_STANDARDS.md](./CODING_STANDARDS.md) | Standar penulisan kode, naming conventions, XML docs |
| [0_Infrastructure/README.md](./0_Infrastructure/README.md) | Dokumentasi layer Infrastructure |
| [1_Core/README.md](./1_Core/README.md) | Dokumentasi layer Core/Domain |
| [2_Application/README.md](./2_Application/README.md) | Dokumentasi layer Application |
| [3_Presentation/README.md](./3_Presentation/README.md) | Dokumentasi layer Presentation |

---

## 📁 Struktur Folder

```
Scripts/
├── 0_Infrastructure/    → External concerns (Database, APIs, SDKs)
├── 1_Core/              → Domain/Business logic (Pure C#)
├── 2_Application/       → Use cases, Services, Managers
├── 3_Presentation/      → UI, Input, MonoBehaviours
├── GameConfiguration/   → ScriptableObjects, Config files
├── LifetimeScope/       → Dependency Injection setup
├── Utility/             → Helper classes, Extensions
└── Test/                → Unit tests
```

---

## 📊 Dependency Flow (Aturan Ketergantungan)

```
┌─────────────────────────────────────────────────────────────┐
│                    3_Presentation                           │
│              (UI, Input, Views, MonoBehaviours)             │
│         Depends on: 2_Application, 1_Core (read-only)       │
├─────────────────────────────────────────────────────────────┤
│                    2_Application                            │
│           (Services, Managers, Use Cases, Factories)        │
│            Depends on: 1_Core, 0_Infrastructure             │
├─────────────────────────────────────────────────────────────┤
│                       1_Core                                │
│     (Domain Models, Interfaces, Enums, Events, Constants)   │
│                  Depends on: NOTHING ❌                      │
├─────────────────────────────────────────────────────────────┤
│                   0_Infrastructure                          │
│        (Database, File I/O, External APIs, SDKs)            │
│       Depends on: 1_Core (implements interfaces)            │
└─────────────────────────────────────────────────────────────┘
```

### Aturan Dependency:
| Layer | Boleh Mengakses | Tidak Boleh Mengakses |
|-------|-----------------|----------------------|
| **3_Presentation** | 2_Application, 1_Core* | 0_Infrastructure |
| **2_Application** | 1_Core, 0_Infrastructure | 3_Presentation |
| **1_Core** | - (Independen) | Semua layer lain |
| **0_Infrastructure** | 1_Core | 2_Application, 3_Presentation |

> *Presentation boleh mengakses 1_Core HANYA untuk: Enums, Domain Models (read-only), Events, dan Interfaces. **BUKAN** untuk business logic.

---

## 🎯 Prinsip Utama

### 1. **Dependency Inversion Principle (DIP)**
- Layer atas bergantung pada abstraksi (interfaces), bukan implementasi konkret
- Interfaces didefinisikan di `1_Core`, implementasi di layer lain

### 2. **Separation of Concerns**
- Setiap layer memiliki tanggung jawab spesifik
- Jangan mencampur UI logic dengan business logic

### 3. **Single Responsibility Principle (SRP)**
- Setiap class hanya memiliki satu alasan untuk berubah

### 4. **Pure Domain (1_Core)**
- Layer Core harus pure C# (tidak ada dependency Unity)
- Bisa di-test tanpa Unity Test Runner

---

## ✅ Contoh Penggunaan yang Benar

```csharp
// ✅ Presentation menggunakan enum dari Core untuk display
if (gameState == GameState.Playing) 
{ 
    ShowPlayUI(); 
}

// ✅ Presentation memanggil Application service
_gameStateService.StartGame();

// ✅ Application mengimplementasikan business logic
public void CalculateDamage(Player player, Enemy enemy)
{
    int damage = player.Attack - enemy.Defense;
    // ...
}

// ✅ Infrastructure mengimplementasikan interface dari Core
public class PlayerPrefsRepository : IPlayerRepository
{
    public void Save(PlayerData data) { /* ... */ }
}
```

---

## ❌ Contoh Penggunaan yang Salah

```csharp
// ❌ Core menggunakan UnityEngine
using UnityEngine; // JANGAN di layer Core!

// ❌ Presentation mengimplementasikan business logic
public class UIManager : MonoBehaviour
{
    void OnButtonClick()
    {
        int damage = player.Attack * 2 - enemy.Defense; // Business logic di UI!
    }
}

// ❌ Presentation langsung mengakses Infrastructure
public class GameUI : MonoBehaviour
{
    void SaveGame()
    {
        _databaseService.Save(); // Harus melalui Application!
    }
}

// ❌ Core bergantung pada layer lain
public class Player
{
    private GameManager _manager; // Core tidak boleh tahu Application!
}
```

---

## 📦 Folder Tambahan

### GameConfiguration/
- ScriptableObjects untuk game settings
- Config files yang bisa diubah dari Unity Editor

### LifetimeScope/
- Setup Dependency Injection (VContainer/Zenject)
- Binding interfaces ke implementasi konkret

### Utility/
- Helper classes, Extension methods
- Bisa diakses oleh semua layer
- Contoh: `Parameter.cs` untuk konstanta global

### Test/
- Unit tests untuk setiap layer
- Integration tests

---

## 🔄 Alur Data Tipikal

```
[User Input] 
    ↓
[3_Presentation] → Capture input, update UI
    ↓
[2_Application] → Process use case, orchestrate
    ↓
[1_Core] → Execute business logic
    ↓
[0_Infrastructure] → Persist data, call external APIs
    ↓
[Kembali ke atas untuk update UI]
```

---

## 📝 Konvensi Penamaan

| Jenis | Konvensi | Contoh |
|-------|----------|--------|
| Interface | `I` + PascalCase | `IPlayerRepository` |
| Service | PascalCase + `Service` | `GameStateService` |
| Manager | PascalCase + `Manager` | `AudioManager` |
| Factory | PascalCase + `Factory` | `EnemyFactory` |
| Event | PascalCase + `Event` | `PlayerDiedEvent` |
| Constant | UPPER_SNAKE_CASE | `MASTER_VOLUME` |

---

## 🛠️ Tools & Libraries yang Digunakan

- **Dependency Injection**: VContainer / Zenject
- **Async**: UniTask
- **Audio**: BroAudio
- **Animation**: PrimeTween
- **UI**: Unity UI / TextMesh Pro

---

> 📖 Lihat file `README.md` di setiap folder layer untuk dokumentasi lebih detail.
