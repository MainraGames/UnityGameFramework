# LifetimeScope Folder (Dependency Injection)

## 📌 Deskripsi
Folder **LifetimeScope** berisi konfigurasi **Dependency Injection** menggunakan VContainer. Di sinilah semua dependencies di-register dan di-bind ke interfaces.

---

## 🎯 Tanggung Jawab

1. **Dependency Registration** - Mendaftarkan semua services, managers, dan repositories
2. **Lifetime Management** - Mengatur lifetime (Singleton, Transient, Scoped)
3. **Interface Binding** - Menghubungkan interface ke implementasi konkret
4. **Scene-specific DI** - Konfigurasi DI per scene

---

## 📁 Struktur Folder

```
LifetimeScope/
├── GameBootstrapper.cs           → Entry point aplikasi
├── GameLifetimeScope.cs          → DI untuk Gameplay scene
├── LoadingLifetimeScope.cs       → DI untuk Loading scene
├── MenuLifetimeScope.cs          → DI untuk Main Menu scene
└── SplashScreenLifetimeScope.cs  → DI untuk Splash Screen
```

---

## 📊 Peran dalam Arsitektur

```
┌─────────────────────────────────────────────────────────────┐
│                     LifetimeScope                           │
│         (Menghubungkan semua layer melalui DI)              │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   Interface (1_Core)  ◄────────►  Implementation            │
│   IPlayerRepository   ◄────────►  PlayerPrefsRepository     │
│                                   (0_Infrastructure)        │
│                                                             │
│   Services (2_Application)                                  │
│   PlayerService, GameManager, etc.                          │
│                                                             │
│   Views (3_Presentation)                                    │
│   Inject services via [Inject] attribute                    │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 💡 Contoh Implementasi

### Game Bootstrapper (Entry Point):
```csharp
// LifetimeScope/GameBootstrapper.cs
using VContainer;
using VContainer.Unity;

namespace MainraFramework.LifetimeScope
{
    public class GameBootstrapper : LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            // === 0_Infrastructure ===
            builder.Register<PlayerPrefsRepository>(Lifetime.Singleton)
                   .As<IPlayerRepository>();
            
            builder.Register<FileRepository>(Lifetime.Singleton)
                   .As<IFileRepository>();

            // === 2_Application - Core Services ===
            builder.Register<EventAggregator>(Lifetime.Singleton);
            builder.Register<GameStateService>(Lifetime.Singleton);
            builder.Register<SceneLoader>(Lifetime.Singleton);

            // === 2_Application - Game Services ===
            builder.Register<PlayerService>(Lifetime.Singleton);
            builder.Register<AudioService>(Lifetime.Singleton)
                   .As<IAudioService>();

            // === 2_Application - Managers ===
            builder.Register<GameManager>(Lifetime.Singleton);

            // === 2_Application - Factories ===
            builder.Register<EnemyFactory>(Lifetime.Singleton);
        }
    }
}
```

### Scene-Specific LifetimeScope:
```csharp
// LifetimeScope/GameLifetimeScope.cs
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace MainraFramework.LifetimeScope
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameplayScreen _gameplayScreen;
        [SerializeField] private PlayerHUD _playerHUD;

        protected override void Configure(IContainerBuilder builder)
        {
            // Register scene-specific components
            builder.RegisterComponent(_gameplayScreen);
            builder.RegisterComponent(_playerHUD);

            // Scene-specific services
            builder.Register<LevelManager>(Lifetime.Scoped);
            builder.Register<SpawnManager>(Lifetime.Scoped);
        }
    }
}
```

### Menu LifetimeScope:
```csharp
// LifetimeScope/MenuLifetimeScope.cs
using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace MainraFramework.LifetimeScope
{
    public class MenuLifetimeScope : LifetimeScope
    {
        [SerializeField] private MainMenuScreen _mainMenuScreen;
        [SerializeField] private SettingsPopup _settingsPopup;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponent(_mainMenuScreen);
            builder.RegisterComponent(_settingsPopup);
        }
    }
}
```

---

## 🔧 Lifetime Types

| Lifetime | Deskripsi | Contoh Penggunaan |
|----------|-----------|-------------------|
| `Singleton` | Satu instance untuk seluruh aplikasi | GameManager, EventAggregator |
| `Scoped` | Satu instance per scope/scene | LevelManager, SpawnManager |
| `Transient` | Instance baru setiap kali di-resolve | Factories, Builders |

---

## 📝 Cara Inject Dependencies

### Via Constructor (Recommended):
```csharp
public class PlayerService
{
    private readonly IPlayerRepository _repository;
    private readonly EventAggregator _eventAggregator;

    // VContainer akan inject otomatis
    public PlayerService(
        IPlayerRepository repository,
        EventAggregator eventAggregator)
    {
        _repository = repository;
        _eventAggregator = eventAggregator;
    }
}
```

### Via [Inject] Attribute (untuk MonoBehaviour):
```csharp
public class MainMenuScreen : MonoBehaviour
{
    private GameManager _gameManager;
    private GameStateService _gameStateService;

    [Inject]
    public void Construct(
        GameManager gameManager,
        GameStateService gameStateService)
    {
        _gameManager = gameManager;
        _gameStateService = gameStateService;
    }
}
```

---

## ✅ Best Practices

1. **Register by Interface** - Gunakan `.As<IInterface>()` untuk abstraksi
2. **Singleton untuk Shared State** - Services yang perlu share state
3. **Scoped untuk Scene-specific** - Components yang hanya ada di scene tertentu
4. **Parent-Child Scope** - Gunakan inheritance untuk share registrations

---

## ⚠️ Catatan Penting

1. **Jangan circular dependency** - A depends on B, B depends on A
2. **Order matters** - Parent scope harus di-setup sebelum child
3. **Dispose dengan benar** - Scoped objects akan di-dispose saat scene unload

---

> 📖 Kembali ke [Architecture Overview](../ARCHITECTURE.md)
