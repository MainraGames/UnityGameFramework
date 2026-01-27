# 1_Core Layer (Domain Layer)

## 📌 Deskripsi
Layer **Core** adalah jantung dari aplikasi yang berisi **business logic** dan **domain models**. Layer ini harus **100% independen** - tidak bergantung pada layer lain maupun framework Unity.

> ⚠️ **PENTING**: Layer ini TIDAK BOLEH menggunakan `using UnityEngine;`

---

## 🎯 Tanggung Jawab

1. **Domain Models** - Entitas dan value objects bisnis
2. **Interfaces** - Kontrak/abstraksi untuk dependency inversion
3. **Enums** - Enumerasi untuk domain logic
4. **Events** - Domain events untuk komunikasi
5. **Constants** - Nilai konstanta domain
6. **Business Rules** - Aturan bisnis murni

---

## 📁 Struktur Folder

```
1_Core/
├── Constants/          → Nilai konstanta domain
│   └── GameConstants.cs
├── DomainModels/       → Entity dan Value Objects
│   ├── PlayerData.cs
│   ├── EnemyData.cs
│   └── ItemData.cs
├── Enums/              → Enumerasi domain
│   ├── GameState.cs
│   ├── ItemType.cs
│   └── DamageType.cs
├── Events/             → Domain events
│   ├── PlayerEvents.cs
│   └── GameEvents.cs
└── Interfaces/         → Kontrak/abstraksi
    ├── IPlayerRepository.cs
    ├── IEnemyService.cs
    └── IAudioService.cs
```

---

## ✅ Aturan Layer Ini

### Boleh (✅):
- Pure C# classes dan structs
- Mendefinisikan interfaces
- Mendefinisikan enums dan constants
- Mendefinisikan domain models/entities
- Mengandung business rules

### Tidak Boleh (❌):
- `using UnityEngine;` ❌
- `using` namespace dari layer lain ❌
- Mengakses `0_Infrastructure` ❌
- Mengakses `2_Application` ❌
- Mengakses `3_Presentation` ❌
- MonoBehaviour atau ScriptableObject ❌

---

## 📊 Dependency

```
1_Core
   │
   ▼
 NOTHING (Independen)
```

Layer lain yang bergantung pada Core:
- `0_Infrastructure` → Implements interfaces
- `2_Application` → Uses models, enums, interfaces
- `3_Presentation` → Uses enums, models (read-only), events

---

## 💡 Contoh Implementasi

### Domain Model:
```csharp
// 1_Core/DomainModels/PlayerData.cs
namespace MainraFramework.Core.DomainModels
{
    [System.Serializable]
    public class PlayerData
    {
        public string Id;
        public string Name;
        public int Level;
        public int Experience;
        public int Health;
        public int MaxHealth;
        public int Gold;

        public PlayerData()
        {
            Id = System.Guid.NewGuid().ToString();
            Name = "Player";
            Level = 1;
            Experience = 0;
            Health = 100;
            MaxHealth = 100;
            Gold = 0;
        }

        // Business logic di domain model
        public bool IsAlive => Health > 0;
        
        public int ExperienceToNextLevel => Level * 100;
        
        public bool CanLevelUp => Experience >= ExperienceToNextLevel;

        public void TakeDamage(int damage)
        {
            Health = System.Math.Max(0, Health - damage);
        }

        public void Heal(int amount)
        {
            Health = System.Math.Min(MaxHealth, Health + amount);
        }

        public void AddExperience(int amount)
        {
            Experience += amount;
            while (CanLevelUp)
            {
                Experience -= ExperienceToNextLevel;
                Level++;
                MaxHealth += 10;
                Health = MaxHealth;
            }
        }
    }
}
```

### Enum:
```csharp
// 1_Core/Enums/GameState.cs
namespace MainraFramework.Core.Enums
{
    public enum GameState
    {
        None,
        Loading,
        MainMenu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public enum DamageType
    {
        Physical,
        Magical,
        Fire,
        Ice,
        Lightning,
        Poison
    }
}
```

### Interface:
```csharp
// 1_Core/Interfaces/IPlayerRepository.cs
using MainraFramework.Core.DomainModels;

namespace MainraFramework.Core.Interfaces
{
    public interface IPlayerRepository
    {
        void Save(PlayerData data);
        PlayerData Load();
        void Delete();
        bool HasSaveData();
    }
}
```

```csharp
// 1_Core/Interfaces/IAudioService.cs
namespace MainraFramework.Core.Interfaces
{
    public interface IAudioService
    {
        void PlayBGM(string clipName);
        void PlaySFX(string clipName);
        void StopBGM();
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
    }
}
```

### Domain Event:
```csharp
// 1_Core/Events/PlayerEvents.cs
using System;
using MainraFramework.Core.DomainModels;

namespace MainraFramework.Core.Events
{
    public class PlayerDiedEvent
    {
        public PlayerData Player { get; }
        public string CauseOfDeath { get; }

        public PlayerDiedEvent(PlayerData player, string cause)
        {
            Player = player;
            CauseOfDeath = cause;
        }
    }

    public class PlayerLevelUpEvent
    {
        public PlayerData Player { get; }
        public int OldLevel { get; }
        public int NewLevel { get; }

        public PlayerLevelUpEvent(PlayerData player, int oldLevel, int newLevel)
        {
            Player = player;
            OldLevel = oldLevel;
            NewLevel = newLevel;
        }
    }

    public class PlayerHealthChangedEvent
    {
        public PlayerData Player { get; }
        public int OldHealth { get; }
        public int NewHealth { get; }

        public PlayerHealthChangedEvent(PlayerData player, int oldHealth, int newHealth)
        {
            Player = player;
            OldHealth = oldHealth;
            NewHealth = newHealth;
        }
    }
}
```

### Constants:
```csharp
// 1_Core/Constants/GameConstants.cs
namespace MainraFramework.Core.Constants
{
    public static class GameConstants
    {
        public const int MAX_PLAYER_LEVEL = 100;
        public const int BASE_EXPERIENCE_PER_LEVEL = 100;
        public const float CRITICAL_HIT_MULTIPLIER = 2.0f;
        public const int INVENTORY_MAX_SLOTS = 30;
    }
}
```

---

## 🧪 Testability

Karena Core tidak bergantung pada Unity, Anda bisa test dengan NUnit biasa:

```csharp
[Test]
public void PlayerData_TakeDamage_ReducesHealth()
{
    // Arrange
    var player = new PlayerData { Health = 100 };
    
    // Act
    player.TakeDamage(30);
    
    // Assert
    Assert.AreEqual(70, player.Health);
}

[Test]
public void PlayerData_TakeDamage_HealthCannotGoBelowZero()
{
    // Arrange
    var player = new PlayerData { Health = 50 };
    
    // Act
    player.TakeDamage(100);
    
    // Assert
    Assert.AreEqual(0, player.Health);
}
```

---

## ⚠️ Catatan Penting

1. **Tidak ada Unity** - Jika butuh Vector3, buat struct sendiri atau gunakan System.Numerics
2. **Immutable jika memungkinkan** - Pertimbangkan membuat models immutable
3. **Rich Domain Model** - Letakkan business logic di domain model, bukan di service
4. **Validation** - Tambahkan validasi di domain model

---

> 📖 Kembali ke [Architecture Overview](../ARCHITECTURE.md)
