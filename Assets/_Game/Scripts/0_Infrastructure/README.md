# 0_Infrastructure Layer

## 📌 Deskripsi
Layer **Infrastructure** adalah layer paling bawah yang menangani semua **external concerns** - yaitu segala sesuatu yang berada di luar aplikasi seperti database, file system, external APIs, dan third-party SDKs.

---

## 🎯 Tanggung Jawab

1. **Data Persistence**
   - PlayerPrefs wrapper
   - File I/O (Save/Load game)
   - Local database (SQLite, LiteDB)

2. **External Services**
   - REST API clients
   - Firebase, PlayFab, dll
   - Analytics services

3. **Third-Party SDKs**
   - Ads SDK (AdMob, Unity Ads)
   - In-App Purchase
   - Social media integration

4. **Platform-Specific Code**
   - Android/iOS native plugins
   - Platform-specific implementations

---

## 📁 Struktur Folder yang Direkomendasikan

```
0_Infrastructure/
├── Repositories/           → Implementasi data persistence
│   ├── PlayerPrefsRepository.cs
│   ├── FileRepository.cs
│   └── DatabaseRepository.cs
├── ExternalServices/       → Integrasi API external
│   ├── FirebaseService.cs
│   ├── AnalyticsService.cs
│   └── LeaderboardService.cs
├── ThirdParty/             → Third-party SDK wrappers
│   ├── AdsService.cs
│   ├── IAPService.cs
│   └── SocialService.cs
└── Platform/               → Platform-specific code
    ├── AndroidBridge.cs
    └── IOSBridge.cs
```

---

## ✅ Aturan Layer Ini

### Boleh (✅):
- Mengakses `1_Core` untuk mengimplementasikan interfaces
- Menggunakan Unity API (PlayerPrefs, File, dll)
- Menggunakan third-party libraries

### Tidak Boleh (❌):
- Mengakses `2_Application`
- Mengakses `3_Presentation`
- Mengandung business logic

---

## 📊 Dependency

```
0_Infrastructure
      │
      ▼
   1_Core (implements interfaces)
```

---

## 💡 Contoh Implementasi

### Interface di 1_Core:
```csharp
// 1_Core/Interfaces/IPlayerRepository.cs
namespace MainraFramework.Core.Interfaces
{
    public interface IPlayerRepository
    {
        void Save(PlayerData data);
        PlayerData Load();
        void Delete();
    }
}
```

### Implementasi di 0_Infrastructure:
```csharp
// 0_Infrastructure/Repositories/PlayerPrefsRepository.cs
using UnityEngine;
using MainraFramework.Core.Interfaces;
using MainraFramework.Core.DomainModels;

namespace MainraFramework.Infrastructure.Repositories
{
    public class PlayerPrefsRepository : IPlayerRepository
    {
        private const string PLAYER_DATA_KEY = "PlayerData";

        public void Save(PlayerData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(PLAYER_DATA_KEY, json);
            PlayerPrefs.Save();
        }

        public PlayerData Load()
        {
            string json = PlayerPrefs.GetString(PLAYER_DATA_KEY, "");
            if (string.IsNullOrEmpty(json))
                return new PlayerData();
            
            return JsonUtility.FromJson<PlayerData>(json);
        }

        public void Delete()
        {
            PlayerPrefs.DeleteKey(PLAYER_DATA_KEY);
        }
    }
}
```

### External API Service:
```csharp
// 0_Infrastructure/ExternalServices/LeaderboardService.cs
using Cysharp.Threading.Tasks;
using MainraFramework.Core.Interfaces;

namespace MainraFramework.Infrastructure.ExternalServices
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly string _apiUrl = "https://api.example.com/leaderboard";

        public async UniTask<LeaderboardData[]> GetTopScores(int count)
        {
            // Implementation untuk fetch dari API
            // ...
        }

        public async UniTask SubmitScore(string playerId, int score)
        {
            // Implementation untuk submit score
            // ...
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
        // Register Infrastructure implementations
        builder.Register<PlayerPrefsRepository>(Lifetime.Singleton)
               .As<IPlayerRepository>();
        
        builder.Register<LeaderboardService>(Lifetime.Singleton)
               .As<ILeaderboardService>();
    }
}
```

---

## ⚠️ Catatan Penting

1. **Selalu gunakan interface dari Core** - Jangan expose implementasi konkret ke layer lain
2. **Handle errors dengan baik** - External services bisa gagal, selalu handle exceptions
3. **Async operations** - Gunakan UniTask untuk operasi async
4. **Mock untuk testing** - Buat mock implementations untuk unit testing

---

> 📖 Kembali ke [Architecture Overview](../ARCHITECTURE.md)
