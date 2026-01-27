# 2_Application Layer (Use Case Layer)

## 📌 Deskripsi
Layer **Application** adalah layer yang mengatur **alur aplikasi** dan **use cases**. Layer ini bertindak sebagai **orchestrator** yang menghubungkan Core dengan Infrastructure dan menyediakan API untuk Presentation layer.

---

## 🎯 Tanggung Jawab

1. **Services** - Application services yang mengeksekusi use cases
2. **Managers** - Orchestration dan coordination
3. **Factories** - Object creation patterns
4. **Initializers** - Setup dan bootstrap logic
5. **State Management** - Game state management
6. **Event Aggregator** - Pub/Sub untuk komunikasi antar module

---

## 📁 Struktur Folder

```
2_Application/
├── EventAggregator.cs      → Pub/Sub event system
├── GameStateService.cs     → State management
├── SceneLoader.cs          → Scene management
├── Factories/              → Object creation
│   ├── EnemyFactory.cs
│   └── ItemFactory.cs
├── Initializers/           → Bootstrap logic
│   ├── GameInitializer.cs
│   └── AudioInitializer.cs
├── Managers/               → Orchestration
│   ├── GameManager.cs
│   └── LevelManager.cs
└── Services/               → Application services
    ├── PlayerService.cs
    ├── CombatService.cs
    └── InventoryService.cs
```

---

## ✅ Aturan Layer Ini

### Boleh (✅):
- Mengakses `1_Core` (models, interfaces, enums, events)
- Mengakses `0_Infrastructure` (melalui interfaces)
- Menggunakan Unity API (SceneManager, dll)
- Mengimplementasikan use cases
- Mengatur alur aplikasi

### Tidak Boleh (❌):
- Mengakses `3_Presentation` ❌
- Mengandung UI logic ❌
- Langsung memanipulasi UI elements ❌
- MonoBehaviour untuk UI ❌

---

## 📊 Dependency

```
2_Application
      │
      ├──────► 1_Core (uses models, interfaces, enums)
      │
      └──────► 0_Infrastructure (via interfaces)
```

---

## 💡 Contoh Implementasi

### Game State Service:
```csharp
// 2_Application/GameStateService.cs
using System;
using MainraFramework.Core.Enums;

namespace MainraFramework.Application
{
    public class GameStateService
    {
        public GameState CurrentState { get; private set; } = GameState.None;
        
        public event Action<GameState, GameState> OnStateChanged;

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;
            
            var oldState = CurrentState;
            CurrentState = newState;
            
            OnStateChanged?.Invoke(oldState, newState);
        }

        public bool IsPlaying => CurrentState == GameState.Playing;
        public bool IsPaused => CurrentState == GameState.Paused;
    }
}
```

### Event Aggregator (Pub/Sub):
```csharp
// 2_Application/EventAggregator.cs
using System;
using System.Collections.Generic;

namespace MainraFramework.Application
{
    public class EventAggregator
    {
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new();

        public void Subscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
            {
                _subscribers[type] = new List<Delegate>();
            }
            _subscribers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                _subscribers[type].Remove(handler);
            }
        }

        public void Publish<T>(T eventData)
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                foreach (var handler in _subscribers[type])
                {
                    ((Action<T>)handler)?.Invoke(eventData);
                }
            }
        }
    }
}
```

### Player Service:
```csharp
// 2_Application/Services/PlayerService.cs
using MainraFramework.Core.DomainModels;
using MainraFramework.Core.Interfaces;
using MainraFramework.Core.Events;

namespace MainraFramework.Application.Services
{
    public class PlayerService
    {
        private readonly IPlayerRepository _playerRepository;
        private readonly EventAggregator _eventAggregator;
        
        public PlayerData CurrentPlayer { get; private set; }

        public PlayerService(
            IPlayerRepository playerRepository,
            EventAggregator eventAggregator)
        {
            _playerRepository = playerRepository;
            _eventAggregator = eventAggregator;
        }

        public void LoadPlayer()
        {
            CurrentPlayer = _playerRepository.Load();
        }

        public void SavePlayer()
        {
            _playerRepository.Save(CurrentPlayer);
        }

        public void TakeDamage(int damage)
        {
            int oldHealth = CurrentPlayer.Health;
            CurrentPlayer.TakeDamage(damage);
            
            _eventAggregator.Publish(new PlayerHealthChangedEvent(
                CurrentPlayer, oldHealth, CurrentPlayer.Health));

            if (!CurrentPlayer.IsAlive)
            {
                _eventAggregator.Publish(new PlayerDiedEvent(
                    CurrentPlayer, "Damage"));
            }
        }

        public void AddExperience(int amount)
        {
            int oldLevel = CurrentPlayer.Level;
            CurrentPlayer.AddExperience(amount);
            
            if (CurrentPlayer.Level > oldLevel)
            {
                _eventAggregator.Publish(new PlayerLevelUpEvent(
                    CurrentPlayer, oldLevel, CurrentPlayer.Level));
            }
        }

        public void Heal(int amount)
        {
            int oldHealth = CurrentPlayer.Health;
            CurrentPlayer.Heal(amount);
            
            _eventAggregator.Publish(new PlayerHealthChangedEvent(
                CurrentPlayer, oldHealth, CurrentPlayer.Health));
        }
    }
}
```

### Scene Loader:
```csharp
// 2_Application/SceneLoader.cs
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using System;

namespace MainraFramework.Application
{
    public class SceneLoader
    {
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        public event Action<float> OnLoadProgress;

        public async UniTask LoadSceneAsync(string sceneName)
        {
            OnSceneLoadStarted?.Invoke(sceneName);
            
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                OnLoadProgress?.Invoke(operation.progress);
                await UniTask.Yield();
            }

            OnLoadProgress?.Invoke(1f);
            operation.allowSceneActivation = true;
            
            await UniTask.WaitUntil(() => operation.isDone);
            
            OnSceneLoadCompleted?.Invoke(sceneName);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
```

### Factory:
```csharp
// 2_Application/Factories/EnemyFactory.cs
using MainraFramework.Core.DomainModels;
using MainraFramework.Core.Enums;

namespace MainraFramework.Application.Factories
{
    public class EnemyFactory
    {
        public EnemyData CreateEnemy(EnemyType type, int level)
        {
            return type switch
            {
                EnemyType.Goblin => new EnemyData
                {
                    Name = "Goblin",
                    Health = 50 + (level * 10),
                    Attack = 5 + (level * 2),
                    Defense = 2 + level
                },
                EnemyType.Orc => new EnemyData
                {
                    Name = "Orc",
                    Health = 100 + (level * 20),
                    Attack = 10 + (level * 3),
                    Defense = 5 + (level * 2)
                },
                EnemyType.Dragon => new EnemyData
                {
                    Name = "Dragon",
                    Health = 500 + (level * 50),
                    Attack = 30 + (level * 5),
                    Defense = 20 + (level * 3)
                },
                _ => new EnemyData()
            };
        }
    }
}
```

### Game Manager:
```csharp
// 2_Application/Managers/GameManager.cs
using MainraFramework.Core.Enums;
using MainraFramework.Core.Events;
using MainraFramework.Application.Services;

namespace MainraFramework.Application.Managers
{
    public class GameManager
    {
        private readonly GameStateService _gameStateService;
        private readonly PlayerService _playerService;
        private readonly SceneLoader _sceneLoader;
        private readonly EventAggregator _eventAggregator;

        public GameManager(
            GameStateService gameStateService,
            PlayerService playerService,
            SceneLoader sceneLoader,
            EventAggregator eventAggregator)
        {
            _gameStateService = gameStateService;
            _playerService = playerService;
            _sceneLoader = sceneLoader;
            _eventAggregator = eventAggregator;

            // Subscribe to events
            _eventAggregator.Subscribe<PlayerDiedEvent>(OnPlayerDied);
        }

        public void StartGame()
        {
            _playerService.LoadPlayer();
            _gameStateService.ChangeState(GameState.Playing);
            _sceneLoader.LoadScene("Gameplay");
        }

        public void PauseGame()
        {
            if (_gameStateService.IsPlaying)
            {
                _gameStateService.ChangeState(GameState.Paused);
            }
        }

        public void ResumeGame()
        {
            if (_gameStateService.IsPaused)
            {
                _gameStateService.ChangeState(GameState.Playing);
            }
        }

        public void GameOver()
        {
            _playerService.SavePlayer();
            _gameStateService.ChangeState(GameState.GameOver);
            _sceneLoader.LoadScene("GameOver");
        }

        private void OnPlayerDied(PlayerDiedEvent evt)
        {
            GameOver();
        }
    }
}
```

---

## 🔄 Registrasi di Dependency Injection

```csharp
// LifetimeScope/GameLifetimeScope.cs
public class GameLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        // Application Services
        builder.Register<EventAggregator>(Lifetime.Singleton);
        builder.Register<GameStateService>(Lifetime.Singleton);
        builder.Register<SceneLoader>(Lifetime.Singleton);
        builder.Register<PlayerService>(Lifetime.Singleton);
        
        // Managers
        builder.Register<GameManager>(Lifetime.Singleton);
        
        // Factories
        builder.Register<EnemyFactory>(Lifetime.Singleton);
    }
}
```

---

## ⚠️ Catatan Penting

1. **Jangan akses UI** - Application tidak boleh tahu tentang UI
2. **Gunakan Events** - Komunikasi ke Presentation melalui events
3. **Dependency Injection** - Inject dependencies melalui constructor
4. **Single Responsibility** - Setiap service/manager fokus pada satu area

---

> 📖 Kembali ke [Architecture Overview](../ARCHITECTURE.md)
