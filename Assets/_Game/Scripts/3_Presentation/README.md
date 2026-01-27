# 3_Presentation Layer (UI/View Layer)

## 📌 Deskripsi
Layer **Presentation** adalah layer paling atas yang menangani **User Interface**, **Input**, dan **Visual Feedback**. Layer ini berisi semua MonoBehaviour yang berinteraksi langsung dengan player.

---

## 🎯 Tanggung Jawab

1. **UI Components** - UI elements, Views, Screens
2. **Input Handling** - Keyboard, mouse, touch input
3. **Visual Feedback** - Animations, particles, effects
4. **Audio Triggers** - Triggering sound effects
5. **User Interaction** - Button clicks, gestures

---

## 📁 Struktur Folder

```
3_Presentation/
├── Input/                  → Input handling
│   ├── PlayerInputHandler.cs
│   └── TouchInputHandler.cs
└── UI/                     → User Interface
    ├── Screens/            → Full screens
    │   ├── MainMenuScreen.cs
    │   ├── GameplayScreen.cs
    │   ├── PauseScreen.cs
    │   └── GameOverScreen.cs
    ├── Components/         → Reusable UI components
    │   ├── HealthBar.cs
    │   ├── ScoreDisplay.cs
    │   └── ButtonBase.cs
    ├── Popups/             → Modal dialogs
    │   ├── ConfirmPopup.cs
    │   └── SettingsPopup.cs
    └── HUD/                → In-game HUD
        ├── PlayerHUD.cs
        └── BossHealthBar.cs
```

---

## ✅ Aturan Layer Ini

### Boleh (✅):
- Mengakses `2_Application` (services, managers)
- Mengakses `1_Core` untuk:
  - ✅ Enums (untuk display logic)
  - ✅ Domain Models (read-only, untuk display)
  - ✅ Events (subscribe to domain events)
  - ✅ Constants (untuk UI configuration)
- Menggunakan Unity UI, MonoBehaviour
- Menghandle user input
- Triggering animations dan effects

### Tidak Boleh (❌):
- Mengakses `0_Infrastructure` langsung ❌
- Mengimplementasikan business logic ❌
- Memodifikasi domain models langsung ❌
- Melakukan data persistence langsung ❌

---

## 📊 Dependency

```
3_Presentation
      │
      ├──────► 2_Application (calls services/managers)
      │
      └──────► 1_Core (read-only: enums, models, events, constants)
```

---

## 💡 Contoh Implementasi

### Main Menu Screen:
```csharp
// 3_Presentation/UI/Screens/MainMenuScreen.cs
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using MainraFramework.Application.Managers;
using MainraFramework.Application;

namespace MainraFramework.Presentation.UI.Screens
{
    public class MainMenuScreen : MonoBehaviour
    {
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;

        private GameManager _gameManager;

        [Inject]
        public void Construct(GameManager gameManager)
        {
            _gameManager = gameManager;
        }

        private void Start()
        {
            _startButton.onClick.AddListener(OnStartClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
            _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDestroy()
        {
            _startButton.onClick.RemoveListener(OnStartClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnStartClicked()
        {
            // ✅ Memanggil Application layer
            _gameManager.StartGame();
        }

        private void OnSettingsClicked()
        {
            // Open settings popup
        }

        private void OnQuitClicked()
        {
            Application.Quit();
        }
    }
}
```

### Health Bar Component:
```csharp
// 3_Presentation/UI/Components/HealthBar.cs
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using MainraFramework.Application;
using MainraFramework.Core.Events;

namespace MainraFramework.Presentation.UI.Components
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Image _fillImage;
        [SerializeField] private Gradient _colorGradient;

        private EventAggregator _eventAggregator;

        [Inject]
        public void Construct(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        private void OnEnable()
        {
            // ✅ Subscribe to Core event
            _eventAggregator.Subscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        }

        private void OnDisable()
        {
            _eventAggregator.Unsubscribe<PlayerHealthChangedEvent>(OnHealthChanged);
        }

        private void OnHealthChanged(PlayerHealthChangedEvent evt)
        {
            float healthPercent = (float)evt.NewHealth / evt.Player.MaxHealth;
            UpdateHealthBar(healthPercent);
        }

        private void UpdateHealthBar(float percent)
        {
            _fillImage.fillAmount = percent;
            _fillImage.color = _colorGradient.Evaluate(percent);
        }
    }
}
```

### Gameplay Screen dengan State Handling:
```csharp
// 3_Presentation/UI/Screens/GameplayScreen.cs
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using MainraFramework.Application;
using MainraFramework.Application.Managers;
using MainraFramework.Core.Enums;

namespace MainraFramework.Presentation.UI.Screens
{
    public class GameplayScreen : MonoBehaviour
    {
        [SerializeField] private Button _pauseButton;
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _mainMenuButton;

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

        private void Start()
        {
            _pauseButton.onClick.AddListener(OnPauseClicked);
            _resumeButton.onClick.AddListener(OnResumeClicked);
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            _gameStateService.OnStateChanged += OnGameStateChanged;
            
            UpdatePausePanel();
        }

        private void OnDestroy()
        {
            _gameStateService.OnStateChanged -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            UpdatePausePanel();
            
            // ✅ Menggunakan enum dari Core untuk display logic
            if (newState == GameState.Paused)
            {
                Time.timeScale = 0f;
            }
            else if (newState == GameState.Playing)
            {
                Time.timeScale = 1f;
            }
        }

        private void UpdatePausePanel()
        {
            // ✅ Menggunakan enum dari Core
            _pausePanel.SetActive(_gameStateService.CurrentState == GameState.Paused);
        }

        private void OnPauseClicked()
        {
            _gameManager.PauseGame();
        }

        private void OnResumeClicked()
        {
            _gameManager.ResumeGame();
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f;
            // Navigate to main menu
        }
    }
}
```

### Player Input Handler:
```csharp
// 3_Presentation/Input/PlayerInputHandler.cs
using UnityEngine;
using VContainer;
using MainraFramework.Application.Services;
using MainraFramework.Application;
using MainraFramework.Core.Enums;

namespace MainraFramework.Presentation.Input
{
    public class PlayerInputHandler : MonoBehaviour
    {
        private PlayerService _playerService;
        private GameStateService _gameStateService;

        [Inject]
        public void Construct(
            PlayerService playerService,
            GameStateService gameStateService)
        {
            _playerService = playerService;
            _gameStateService = gameStateService;
        }

        private void Update()
        {
            // ✅ Cek state dari Core enum
            if (_gameStateService.CurrentState != GameState.Playing)
                return;

            HandleMovementInput();
            HandleActionInput();
        }

        private void HandleMovementInput()
        {
            float horizontal = UnityEngine.Input.GetAxisRaw("Horizontal");
            float vertical = UnityEngine.Input.GetAxisRaw("Vertical");
            
            // Process movement...
        }

        private void HandleActionInput()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                // Attack action
            }

            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                // Interact action
            }
        }
    }
}
```

### Level Up Notification:
```csharp
// 3_Presentation/UI/Components/LevelUpNotification.cs
using UnityEngine;
using TMPro;
using VContainer;
using PrimeTween;
using MainraFramework.Application;
using MainraFramework.Core.Events;

namespace MainraFramework.Presentation.UI.Components
{
    public class LevelUpNotification : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private EventAggregator _eventAggregator;

        [Inject]
        public void Construct(EventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        private void OnEnable()
        {
            _eventAggregator.Subscribe<PlayerLevelUpEvent>(OnLevelUp);
        }

        private void OnDisable()
        {
            _eventAggregator.Unsubscribe<PlayerLevelUpEvent>(OnLevelUp);
        }

        private void OnLevelUp(PlayerLevelUpEvent evt)
        {
            // ✅ Menggunakan data dari Core event
            _levelText.text = $"Level {evt.NewLevel}!";
            ShowNotification();
        }

        private void ShowNotification()
        {
            _panel.SetActive(true);
            _canvasGroup.alpha = 0f;

            Tween.Alpha(_canvasGroup, 1f, 0.3f)
                .Chain(Tween.Delay(2f))
                .Chain(Tween.Alpha(_canvasGroup, 0f, 0.3f))
                .OnComplete(() => _panel.SetActive(false));
        }
    }
}
```

---

## 🚫 Contoh yang SALAH

```csharp
// ❌ SALAH - Business logic di Presentation
public class GameplayScreen : MonoBehaviour
{
    private void OnEnemyKilled()
    {
        // ❌ Business logic tidak boleh di sini!
        int expGained = enemy.Level * 10 + Random.Range(5, 15);
        player.Experience += expGained;
        
        if (player.Experience >= player.Level * 100)
        {
            player.Level++;
            player.MaxHealth += 10;
        }
    }
}

// ❌ SALAH - Langsung akses Infrastructure
public class SaveButton : MonoBehaviour
{
    private void OnClick()
    {
        // ❌ Tidak boleh langsung akses Infrastructure!
        PlayerPrefs.SetString("PlayerData", JsonUtility.ToJson(player));
        PlayerPrefs.Save();
    }
}

// ❌ SALAH - Memodifikasi domain model langsung
public class DamageHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision col)
    {
        // ❌ Harus melalui Application service!
        _playerData.Health -= 10;
    }
}
```

---

## ✅ Cara yang BENAR

```csharp
// ✅ BENAR - Memanggil Application service
public class GameplayScreen : MonoBehaviour
{
    private void OnEnemyKilled()
    {
        // ✅ Delegate ke Application layer
        _playerService.AddExperience(enemy.ExperienceReward);
    }
}

// ✅ BENAR - Melalui Application
public class SaveButton : MonoBehaviour
{
    private void OnClick()
    {
        // ✅ Melalui Application service
        _playerService.SavePlayer();
    }
}

// ✅ BENAR - Melalui Application service
public class DamageHandler : MonoBehaviour
{
    private void OnCollisionEnter(Collision col)
    {
        // ✅ Melalui Application service
        _playerService.TakeDamage(10);
    }
}
```

---

## ⚠️ Catatan Penting

1. **UI hanya untuk display** - Jangan implement business logic
2. **Event-driven** - Subscribe ke events dari Application/Core
3. **Thin Views** - View harus "bodoh", hanya menampilkan data
4. **Dependency Injection** - Inject services melalui VContainer

---

> 📖 Kembali ke [Architecture Overview](../ARCHITECTURE.md)
