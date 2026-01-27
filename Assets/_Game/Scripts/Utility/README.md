# Utility Folder

## 📌 Deskripsi
Folder **Utility** berisi helper classes, extension methods, dan utility functions yang dapat digunakan oleh **semua layer** dalam arsitektur.

---

## 🎯 Tanggung Jawab

1. **Helper Classes** - Fungsi-fungsi pembantu umum
2. **Extension Methods** - Ekstensi untuk tipe data standar
3. **Constants/Parameters** - Konstanta global (Tags, Layers, dll)
4. **Editor Tools** - Custom editor utilities
5. **Common Utilities** - Tweeners, formatters, validators

---

## 📁 Struktur Folder

```
Utility/
├── Editor/             → Editor-only utilities
├── UI/                 → UI helper utilities
├── Unity/              → Unity-specific utilities
│   └── Parameter.cs    → Global constants (Tags, Scenes, etc.)
└── Tweener.cs          → Animation/tween utilities
```

---

## ✅ Aturan Folder Ini

### Karakteristik:
- ✅ Dapat diakses oleh **semua layer**
- ✅ Tidak mengandung business logic
- ✅ Bersifat **stateless** (tidak menyimpan state)
- ✅ Fokus pada fungsi **reusable**

### Boleh (✅):
- Extension methods
- Static helper functions
- Constants dan parameters
- Editor utilities

### Tidak Boleh (❌):
- Business logic
- State management
- Direct dependencies ke layer tertentu

---

## 📊 Dependency

```
        ┌─────────────────────────────┐
        │         Utility             │
        │  (Dapat diakses semua layer)│
        └─────────────────────────────┘
                    ▲
    ┌───────────────┼───────────────┐
    │               │               │
3_Presentation  2_Application   1_Core
                    │
              0_Infrastructure
```

---

## 💡 Contoh Penggunaan

### Parameter.cs - Global Constants:
```csharp
// Utility/Unity/Parameter.cs
namespace MainraFramework.Parameter
{
    public static class Parameter
    {
        public static class Tags
        {
            public const string PLAYER = "Player";
            public const string ENEMY = "Enemy";
            public const string ITEM = "Item";
        }

        public static class Scenes
        {
            public const string MAINMENU = "MainMenu";
            public const string GAMEPLAY = "Gameplay";
        }

        public static class AudioMixer
        {
            public const string MASTERVOLUME = "MasterVolume";
            public const string MUSICVOLUME = "MusicVolume";
        }
    }
}
```

### Penggunaan di Layer Lain:
```csharp
// Di 3_Presentation atau layer manapun
using MainraFramework.Parameter;

public class PlayerDetector : MonoBehaviour
{
    private void OnCollisionEnter(Collision col)
    {
        // ✅ Menggunakan constant dari Utility
        if (col.gameObject.CompareTag(Parameter.Tags.PLAYER))
        {
            // Handle collision
        }
    }
}
```

### Extension Methods:
```csharp
// Utility/Extensions/TransformExtensions.cs
using UnityEngine;

namespace MainraFramework.Utility.Extensions
{
    public static class TransformExtensions
    {
        public static void ResetLocal(this Transform transform)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            transform.localScale = Vector3.one;
        }

        public static void SetPositionX(this Transform transform, float x)
        {
            var pos = transform.position;
            pos.x = x;
            transform.position = pos;
        }
    }
}
```

---

## ⚠️ Catatan Penting

1. **Jangan taruh business logic** - Utility hanya untuk helper functions
2. **Stateless** - Utility tidak boleh menyimpan state
3. **Pure functions** - Fungsi utility harus predictable (input yang sama = output yang sama)
4. **Well-documented** - Karena digunakan banyak layer, dokumentasi penting

---

> 📖 Kembali ke [Architecture Overview](../ARCHITECTURE.md)
