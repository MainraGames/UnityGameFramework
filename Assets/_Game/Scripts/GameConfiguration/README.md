# GameConfiguration Folder

## 📌 Deskripsi
Folder **GameConfiguration** berisi **ScriptableObjects** dan **configuration classes** untuk menyimpan game settings yang dapat diubah dari Unity Editor tanpa perlu mengubah kode.

---

## 🎯 Tanggung Jawab

1. **Game Settings** - Konfigurasi game yang dapat diubah dari Editor
2. **ScriptableObjects** - Data containers untuk game configuration
3. **Balance Data** - Data balancing (damage, health, speed, dll)
4. **Feature Flags** - Toggle fitur on/off

---

## 📁 Struktur Folder

```
GameConfiguration/
├── GameConfig.cs       → ScriptableObject untuk game config
├── GameSettings.cs     → Runtime settings class
└── [Other configs...]
```

---

## 💡 Contoh Implementasi

### GameConfig ScriptableObject:
```csharp
// GameConfiguration/GameConfig.cs
using UnityEngine;

namespace MainraFramework.Configuration
{
    [CreateAssetMenu(fileName = "GameConfig", menuName = "Game/Configuration/Game Config")]
    public class GameConfig : ScriptableObject
    {
        [Header("Player Settings")]
        public int StartingHealth = 100;
        public int StartingGold = 0;
        public float MoveSpeed = 5f;
        public float JumpForce = 10f;

        [Header("Game Balance")]
        public float DifficultyMultiplier = 1f;
        public int BaseExperiencePerLevel = 100;
        public float CriticalHitChance = 0.1f;
        public float CriticalHitMultiplier = 2f;

        [Header("Audio Settings")]
        [Range(0f, 1f)] public float DefaultMasterVolume = 1f;
        [Range(0f, 1f)] public float DefaultMusicVolume = 0.7f;
        [Range(0f, 1f)] public float DefaultSFXVolume = 1f;

        [Header("Feature Flags")]
        public bool EnableTutorial = true;
        public bool EnableAds = true;
        public bool EnableAnalytics = true;
    }
}
```

### GameSettings (Runtime):
```csharp
// GameConfiguration/GameSettings.cs
using UnityEngine;

namespace MainraFramework.Configuration
{
    public class GameSettings
    {
        // Loaded from PlayerPrefs or config
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 0.7f;
        public float SFXVolume { get; set; } = 1f;
        public bool VibrationEnabled { get; set; } = true;
        public int QualityLevel { get; set; } = 2;
        public string Language { get; set; } = "en";

        public void LoadFromPrefs()
        {
            MasterVolume = PlayerPrefs.GetFloat("MasterVolume", 1f);
            MusicVolume = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            SFXVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
            VibrationEnabled = PlayerPrefs.GetInt("Vibration", 1) == 1;
            QualityLevel = PlayerPrefs.GetInt("Quality", 2);
            Language = PlayerPrefs.GetString("Language", "en");
        }

        public void SaveToPrefs()
        {
            PlayerPrefs.SetFloat("MasterVolume", MasterVolume);
            PlayerPrefs.SetFloat("MusicVolume", MusicVolume);
            PlayerPrefs.SetFloat("SFXVolume", SFXVolume);
            PlayerPrefs.SetInt("Vibration", VibrationEnabled ? 1 : 0);
            PlayerPrefs.SetInt("Quality", QualityLevel);
            PlayerPrefs.SetString("Language", Language);
            PlayerPrefs.Save();
        }
    }
}
```

### Penggunaan di Application Layer:
```csharp
// 2_Application/Services/PlayerService.cs
public class PlayerService
{
    private readonly GameConfig _gameConfig;

    public PlayerService(GameConfig gameConfig)
    {
        _gameConfig = gameConfig;
    }

    public PlayerData CreateNewPlayer()
    {
        return new PlayerData
        {
            Health = _gameConfig.StartingHealth,
            Gold = _gameConfig.StartingGold
        };
    }
}
```

### Register di LifetimeScope:
```csharp
// LifetimeScope/GameBootstrapper.cs
public class GameBootstrapper : LifetimeScope
{
    [SerializeField] private GameConfig _gameConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        // Register ScriptableObject
        builder.RegisterInstance(_gameConfig);
        
        // Register runtime settings
        builder.Register<GameSettings>(Lifetime.Singleton);
    }
}
```

---

## 📊 Peran dalam Arsitektur

```
GameConfiguration
       │
       ├──► 2_Application (di-inject, digunakan untuk logic)
       │
       └──► 3_Presentation (di-inject, digunakan untuk UI defaults)
```

---

## ✅ Best Practices

1. **Gunakan ScriptableObject** untuk data yang bisa diubah di Editor
2. **Pisahkan config per domain** (PlayerConfig, EnemyConfig, AudioConfig)
3. **Gunakan [Header]** untuk organisasi di Inspector
4. **Validasi values** di OnValidate()

---

## 💡 Contoh Config Lainnya

### EnemyConfig:
```csharp
[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Game/Configuration/Enemy Config")]
public class EnemyConfig : ScriptableObject
{
    [System.Serializable]
    public class EnemyData
    {
        public string Name;
        public int BaseHealth;
        public int BaseDamage;
        public float MoveSpeed;
        public GameObject Prefab;
    }

    public EnemyData[] Enemies;
}
```

### LevelConfig:
```csharp
[CreateAssetMenu(fileName = "LevelConfig", menuName = "Game/Configuration/Level Config")]
public class LevelConfig : ScriptableObject
{
    [System.Serializable]
    public class WaveData
    {
        public int EnemyCount;
        public float SpawnInterval;
        public string[] EnemyTypes;
    }

    public WaveData[] Waves;
    public float TimeBetweenWaves = 5f;
}
```

---

> 📖 Kembali ke [Architecture Overview](../ARCHITECTURE.md)
