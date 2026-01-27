# 📝 Coding Standards & Conventions

## Deskripsi
Dokumen ini berisi standar penulisan kode untuk project ini. Semua developer dan AI harus mengikuti standar ini untuk menjaga konsistensi, readability, dan maintainability kode.

---

## 📚 Table of Contents
1. [Naming Conventions](#-naming-conventions)
2. [File & Folder Structure](#-file--folder-structure)
3. [XML Documentation](#-xml-documentation)
4. [Region Organization](#-region-organization)
5. [Odin Inspector Attributes](#-odin-inspector-attributes)
6. [Unity Attributes](#-unity-attributes)
7. [VContainer (Dependency Injection)](#-vcontainer-dependency-injection)
8. [Code Organization](#-code-organization)
9. [Best Practices](#-best-practices)
10. [Anti-Patterns](#-anti-patterns)

---

## 🏷️ Naming Conventions

### General Rules
| Jenis | Konvensi | Contoh |
|-------|----------|--------|
| **Class** | PascalCase | `PlayerController`, `GameManager` |
| **Interface** | I + PascalCase | `IPlayerRepository`, `IAudioService` |
| **Method** | PascalCase | `GetPlayerData()`, `CalculateDamage()` |
| **Property** | PascalCase | `CurrentHealth`, `IsAlive` |
| **Public Field** | PascalCase | `MaxHealth`, `StartPosition` |
| **Private Field** | _camelCase | `_playerData`, `_isInitialized` |
| **Parameter** | camelCase | `playerName`, `damageAmount` |
| **Local Variable** | camelCase | `tempHealth`, `targetPosition` |
| **Constant** | UPPER_SNAKE_CASE | `MAX_HEALTH`, `DEFAULT_SPEED` |
| **Enum** | PascalCase | `GameState`, `ItemType` |
| **Enum Value** | PascalCase | `Playing`, `MainMenu` |
| **Event** | PascalCase + Event | `OnPlayerDied`, `HealthChangedEvent` |

### Prefixes & Suffixes
```csharp
// Services
public class PlayerService { }
public class AudioService { }

// Managers (Orchestration)
public class GameManager { }
public class UIManager { }

// Repositories (Data Access)
public class PlayerRepository { }
public class SettingsRepository { }

// Factories
public class EnemyFactory { }
public class WeaponFactory { }

// Handlers
public class InputHandler { }
public class DamageHandler { }

// Controllers (Unity-specific)
public class PlayerController { }
public class CameraController { }

// UI Components
public class MainMenuUI { }
public class HealthBarUI { }
public class SettingsPopupUI { }
```

---

## 📁 File & Folder Structure

### Namespace Convention
```csharp
// Format: _Game.Scripts.{Layer}.{SubFolder}
namespace _Game.Scripts.Core.Enums { }
namespace _Game.Scripts.Core.Interfaces { }
namespace _Game.Scripts.Core.DomainModels { }

namespace _Game.Scripts.Application.Services { }
namespace _Game.Scripts.Application.Managers { }

namespace _Game.Scripts.Presentation.UI { }
namespace _Game.Scripts.Presentation.Input { }

namespace _Game.Scripts.Infrastructure.Repositories { }
```

### File Naming
```
// Class per file, nama file = nama class
PlayerService.cs        → class PlayerService
IPlayerRepository.cs    → interface IPlayerRepository
GameState.cs            → enum GameState
PlayerDiedEvent.cs      → class PlayerDiedEvent
```

---

## 📖 XML Documentation

### Class Documentation
```csharp
/// <summary>
/// Manages player data, health, and experience progression.
/// Handles saving/loading player state through repository.
/// </summary>
/// <remarks>
/// This service should be registered as Singleton in DI container.
/// Depends on: IPlayerRepository, IEventAggregator
/// </remarks>
public class PlayerService
{
    // ...
}
```

### Method Documentation
```csharp
/// <summary>
/// Applies damage to the player and triggers health changed event.
/// </summary>
/// <param name="damage">Amount of damage to apply. Must be positive.</param>
/// <returns>True if player is still alive after damage, false if dead.</returns>
/// <exception cref="ArgumentException">Thrown when damage is negative.</exception>
/// <example>
/// <code>
/// bool isAlive = playerService.TakeDamage(25);
/// if (!isAlive) HandlePlayerDeath();
/// </code>
/// </example>
public bool TakeDamage(int damage)
{
    if (damage < 0)
        throw new ArgumentException("Damage cannot be negative", nameof(damage));
    
    // Implementation...
}
```

### Property Documentation
```csharp
/// <summary>
/// Gets the current player health.
/// </summary>
/// <value>Health value between 0 and MaxHealth.</value>
public int CurrentHealth => _playerData.Health;

/// <summary>
/// Gets or sets the master volume level.
/// </summary>
/// <value>Volume level from 0.0 (mute) to 1.0 (max).</value>
public float MasterVolume { get; set; }
```

### Interface Documentation
```csharp
/// <summary>
/// Defines contract for player data persistence operations.
/// </summary>
/// <remarks>
/// Implementations:
/// - <see cref="PlayerPrefsRepository"/> for local storage
/// - <see cref="CloudRepository"/> for cloud saves
/// </remarks>
public interface IPlayerRepository
{
    /// <summary>
    /// Saves player data to persistent storage.
    /// </summary>
    /// <param name="data">Player data to save.</param>
    void Save(PlayerData data);
    
    /// <summary>
    /// Loads player data from persistent storage.
    /// </summary>
    /// <returns>Loaded player data, or default if none exists.</returns>
    PlayerData Load();
}
```

### Event Documentation
```csharp
/// <summary>
/// Raised when player's health value changes.
/// </summary>
/// <remarks>
/// Subscribe to this event for:
/// - UI health bar updates
/// - Sound effects on damage/heal
/// - Achievement tracking
/// </remarks>
public event Action<int, int> OnHealthChanged;
```

---

## 🗂️ Region Organization

### Standard Region Order
```csharp
public class PlayerService : IPlayerService
{
    #region Constants
    
    private const int MAX_LEVEL = 100;
    private const float CRITICAL_MULTIPLIER = 2.0f;
    
    #endregion

    #region Fields
    
    private readonly IPlayerRepository _repository;
    private readonly IEventAggregator _eventAggregator;
    private PlayerData _currentPlayer;
    
    #endregion

    #region Properties
    
    public int CurrentHealth => _currentPlayer.Health;
    public bool IsAlive => _currentPlayer.Health > 0;
    
    #endregion

    #region Events
    
    public event Action<PlayerData> OnPlayerLoaded;
    public event Action<int, int> OnHealthChanged;
    
    #endregion

    #region Constructor
    
    public PlayerService(
        IPlayerRepository repository,
        IEventAggregator eventAggregator)
    {
        _repository = repository;
        _eventAggregator = eventAggregator;
    }
    
    #endregion

    #region Public Methods
    
    public void LoadPlayer()
    {
        _currentPlayer = _repository.Load();
        OnPlayerLoaded?.Invoke(_currentPlayer);
    }
    
    public bool TakeDamage(int damage)
    {
        // Implementation...
    }
    
    #endregion

    #region Private Methods
    
    private void NotifyHealthChanged(int oldHealth, int newHealth)
    {
        OnHealthChanged?.Invoke(oldHealth, newHealth);
    }
    
    #endregion
}
```

### MonoBehaviour Region Order
```csharp
public class PlayerController : MonoBehaviour
{
    #region Serialized Fields
    
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _jumpForce = 10f;
    
    #endregion

    #region Private Fields
    
    private Rigidbody _rigidbody;
    private bool _isGrounded;
    
    #endregion

    #region Dependencies (Injected)
    
    [Inject] private readonly IInputService _inputService;
    [Inject] private readonly IPlayerService _playerService;
    
    #endregion

    #region Properties
    
    public bool IsMoving => _rigidbody.velocity.magnitude > 0.1f;
    
    #endregion

    #region Unity Lifecycle
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    private void Start()
    {
        Initialize();
    }
    
    private void Update()
    {
        HandleInput();
    }
    
    private void FixedUpdate()
    {
        ApplyMovement();
    }
    
    private void OnDestroy()
    {
        Cleanup();
    }
    
    #endregion

    #region Initialization
    
    private void Initialize()
    {
        // Setup code...
    }
    
    #endregion

    #region Input Handling
    
    private void HandleInput()
    {
        // Input processing...
    }
    
    #endregion

    #region Movement
    
    private void ApplyMovement()
    {
        // Movement logic...
    }
    
    #endregion

    #region Cleanup
    
    private void Cleanup()
    {
        // Unsubscribe events, dispose resources...
    }
    
    #endregion
}
```

---

## 🎨 Odin Inspector Attributes

### BoxGroup - Grouping Fields
```csharp
public class EnemyController : MonoBehaviour
{
    [BoxGroup("Stats")]
    [SerializeField] private int _health = 100;
    
    [BoxGroup("Stats")]
    [SerializeField] private int _damage = 10;
    
    [BoxGroup("Stats")]
    [SerializeField] private float _moveSpeed = 3f;

    [BoxGroup("References")]
    [SerializeField] private Transform _target;
    
    [BoxGroup("References")]
    [SerializeField] private Animator _animator;

    [BoxGroup("Audio")]
    [SerializeField] private AudioClip _attackSound;
    
    [BoxGroup("Audio")]
    [SerializeField] private AudioClip _deathSound;
}
```

### FoldoutGroup - Collapsible Sections
```csharp
public class GameConfig : ScriptableObject
{
    [FoldoutGroup("Player Settings")]
    public int StartingHealth = 100;
    
    [FoldoutGroup("Player Settings")]
    public float MoveSpeed = 5f;
    
    [FoldoutGroup("Player Settings")]
    public float JumpForce = 10f;

    [FoldoutGroup("Enemy Settings")]
    public float SpawnRate = 2f;
    
    [FoldoutGroup("Enemy Settings")]
    public int MaxEnemies = 10;

    [FoldoutGroup("Audio Settings")]
    [Range(0f, 1f)]
    public float MasterVolume = 1f;
    
    [FoldoutGroup("Audio Settings")]
    [Range(0f, 1f)]
    public float MusicVolume = 0.7f;
}
```

### TabGroup - Tabbed Interface
```csharp
public class CharacterData : ScriptableObject
{
    [TabGroup("Basic Info")]
    public string CharacterName;
    
    [TabGroup("Basic Info")]
    public Sprite Portrait;
    
    [TabGroup("Basic Info")]
    [TextArea(3, 5)]
    public string Description;

    [TabGroup("Combat Stats")]
    public int BaseHealth = 100;
    
    [TabGroup("Combat Stats")]
    public int BaseAttack = 10;
    
    [TabGroup("Combat Stats")]
    public int BaseDefense = 5;

    [TabGroup("Abilities")]
    public List<AbilityData> Abilities;
}
```

### Validation Attributes
```csharp
public class WeaponData : ScriptableObject
{
    [Required("Weapon name is required!")]
    public string WeaponName;

    [Required]
    [PreviewField(75)]
    [AssetsOnly]
    public Sprite Icon;

    [MinValue(1)]
    [MaxValue(100)]
    public int Damage = 10;

    [MinValue(0.1f)]
    public float AttackSpeed = 1f;

    [ValidateInput(nameof(ValidateRarity), "Rarity must be between 1-5")]
    public int Rarity = 1;

    private bool ValidateRarity(int value) => value >= 1 && value <= 5;
}
```

### Button Attributes
```csharp
public class DebugController : MonoBehaviour
{
    [Button("Add 100 Gold", ButtonSizes.Large)]
    [GUIColor(0.4f, 0.8f, 0.4f)]
    private void AddGold()
    {
        // Debug method
    }

    [Button("Kill Player")]
    [GUIColor(1f, 0.4f, 0.4f)]
    private void KillPlayer()
    {
        // Debug method
    }

    [ButtonGroup("Health")]
    [Button("Heal 50")]
    private void Heal50() { }

    [ButtonGroup("Health")]
    [Button("Damage 50")]
    private void Damage50() { }
}
```

### ShowIf / HideIf - Conditional Display
```csharp
public class SpawnSettings : MonoBehaviour
{
    public bool UseRandomSpawn;

    [ShowIf(nameof(UseRandomSpawn))]
    public float SpawnRadius = 5f;

    [ShowIf(nameof(UseRandomSpawn))]
    public int MaxSpawnAttempts = 10;

    [HideIf(nameof(UseRandomSpawn))]
    public Transform[] SpawnPoints;

    public bool EnableBossSpawn;

    [ShowIf(nameof(EnableBossSpawn))]
    [BoxGroup("Boss Settings")]
    public GameObject BossPrefab;

    [ShowIf(nameof(EnableBossSpawn))]
    [BoxGroup("Boss Settings")]
    public float BossSpawnDelay = 30f;
}
```

### ReadOnly & DisableInPlayMode
```csharp
public class RuntimeStats : MonoBehaviour
{
    [ReadOnly]
    [SerializeField] private int _currentScore;

    [ReadOnly]
    [SerializeField] private float _playTime;

    [DisableInPlayMode]
    [SerializeField] private GameConfig _config;

    [EnableIf(nameof(IsDebugMode))]
    [SerializeField] private bool _godMode;

    private bool IsDebugMode => Debug.isDebugBuild;
}
```

### InfoBox - Help Text
```csharp
public class AudioSettings : ScriptableObject
{
    [InfoBox("Master volume affects all audio in the game.")]
    [Range(0f, 1f)]
    public float MasterVolume = 1f;

    [InfoBox("Music will fade out during gameplay events.", InfoMessageType.Info)]
    [Range(0f, 1f)]
    public float MusicVolume = 0.7f;

    [InfoBox("High SFX volume may cause audio clipping!", InfoMessageType.Warning)]
    [Range(0f, 1f)]
    public float SFXVolume = 1f;
}
```

### PropertyOrder - Field Ordering
```csharp
public class OrderedComponent : MonoBehaviour
{
    [PropertyOrder(1)]
    public string Name;

    [PropertyOrder(2)]
    public int Health;

    [PropertyOrder(3)]
    public float Speed;

    [PropertyOrder(0)] // Appears first
    [Title("Configuration")]
    public GameConfig Config;
}
```

---

## 🎮 Unity Attributes

### SerializeField & Header
```csharp
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] 
    [Tooltip("Player movement speed in units per second")]
    private float _moveSpeed = 5f;

    [SerializeField]
    [Tooltip("Force applied when jumping")]
    private float _jumpForce = 10f;

    [Header("Ground Check")]
    [SerializeField]
    [Tooltip("Layer mask for ground detection")]
    private LayerMask _groundLayer;

    [SerializeField]
    [Tooltip("Radius for ground check sphere")]
    private float _groundCheckRadius = 0.2f;

    [Header("References")]
    [SerializeField] private Transform _groundCheck;
    [SerializeField] private Animator _animator;
}
```

### Tooltip - Field Descriptions
```csharp
public class WeaponController : MonoBehaviour
{
    [Tooltip("Damage dealt per hit. Affected by player stats.")]
    [SerializeField] private int _baseDamage = 10;

    [Tooltip("Time between attacks in seconds. Lower = faster attacks.")]
    [SerializeField] private float _attackCooldown = 0.5f;

    [Tooltip("Maximum distance for melee attacks.")]
    [SerializeField] private float _attackRange = 2f;
}
```

### Range & Min/Max
```csharp
public class AudioController : MonoBehaviour
{
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;

    [Range(-3f, 3f)]
    [SerializeField] private float _pitch = 1f;

    [Min(0)]
    [SerializeField] private int _priority = 128;
}
```

### TextArea & Multiline
```csharp
public class DialogueData : ScriptableObject
{
    [TextArea(3, 10)]
    [Tooltip("Main dialogue text. Supports rich text.")]
    public string DialogueText;

    [Multiline(5)]
    public string Notes;
}
```

### Space & Separator
```csharp
public class UIController : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _settingsButton;
    
    [Space(20)]
    
    [SerializeField] private GameObject _mainPanel;
    [SerializeField] private GameObject _settingsPanel;
}
```

### RequireComponent & DisallowMultipleComponent
```csharp
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[DisallowMultipleComponent]
public class PhysicsController : MonoBehaviour
{
    private Rigidbody _rigidbody;
    
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }
}
```

### AddComponentMenu & CreateAssetMenu
```csharp
[AddComponentMenu("Game/Controllers/Player Controller")]
public class PlayerController : MonoBehaviour { }

[CreateAssetMenu(fileName = "NewWeaponData", menuName = "Game/Data/Weapon Data")]
public class WeaponData : ScriptableObject { }
```

---

## 💉 VContainer (Dependency Injection)

### Registration Pattern
```csharp
public class GameLifetimeScope : LifetimeScope
{
    [SerializeField] private GameConfig _gameConfig;

    protected override void Configure(IContainerBuilder builder)
    {
        #region Infrastructure Layer
        
        builder.Register<PlayerPrefsRepository>(Lifetime.Singleton)
               .As<IPlayerRepository>();
        
        #endregion

        #region Application Layer - Services
        
        builder.Register<EventAggregator>(Lifetime.Singleton)
               .As<IEventAggregator>();
        
        builder.Register<GameStateService>(Lifetime.Singleton)
               .As<IGameStateService>();
        
        builder.Register<SceneLoader>(Lifetime.Singleton)
               .As<ISceneLoader>();
        
        #endregion

        #region Application Layer - Managers
        
        builder.Register<GameManager>(Lifetime.Singleton);
        
        #endregion

        #region Configuration
        
        builder.RegisterInstance(_gameConfig);
        
        #endregion
    }
}
```

### Injection Patterns
```csharp
// Constructor Injection (Preferred for non-MonoBehaviour)
public class PlayerService
{
    private readonly IPlayerRepository _repository;
    private readonly IEventAggregator _eventAggregator;

    public PlayerService(
        IPlayerRepository repository,
        IEventAggregator eventAggregator)
    {
        _repository = repository;
        _eventAggregator = eventAggregator;
    }
}

// Field Injection (For MonoBehaviour)
public class MainMenuUI : MonoBehaviour
{
    [Inject] private readonly IGameStateService _gameStateService;
    [Inject] private readonly ISceneLoader _sceneLoader;
}

// Method Injection (Alternative for MonoBehaviour)
public class GameplayUI : MonoBehaviour
{
    private IPlayerService _playerService;

    [Inject]
    public void Construct(IPlayerService playerService)
    {
        _playerService = playerService;
    }
}
```

---

## 📐 Code Organization

### Class Structure Template
```csharp
using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using VContainer;
using _Game.Scripts.Core.Interfaces;

namespace _Game.Scripts.Application.Services
{
    /// <summary>
    /// Brief description of what this class does.
    /// </summary>
    /// <remarks>
    /// Additional details, dependencies, usage notes.
    /// </remarks>
    public class ExampleService : IExampleService, IDisposable
    {
        #region Constants
        
        private const int MAX_VALUE = 100;
        
        #endregion

        #region Fields
        
        private readonly IDependency _dependency;
        private int _currentValue;
        
        #endregion

        #region Properties
        
        /// <summary>
        /// Gets the current value.
        /// </summary>
        public int CurrentValue => _currentValue;
        
        #endregion

        #region Events
        
        /// <summary>
        /// Raised when value changes.
        /// </summary>
        public event Action<int> OnValueChanged;
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// Initializes a new instance of ExampleService.
        /// </summary>
        /// <param name="dependency">Required dependency.</param>
        public ExampleService(IDependency dependency)
        {
            _dependency = dependency ?? throw new ArgumentNullException(nameof(dependency));
        }
        
        #endregion

        #region IExampleService Implementation
        
        /// <inheritdoc/>
        public void DoSomething()
        {
            // Implementation
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Updates the current value.
        /// </summary>
        /// <param name="newValue">New value to set.</param>
        public void SetValue(int newValue)
        {
            if (newValue == _currentValue) return;
            
            _currentValue = Math.Clamp(newValue, 0, MAX_VALUE);
            OnValueChanged?.Invoke(_currentValue);
        }
        
        #endregion

        #region Private Methods
        
        private void InternalMethod()
        {
            // Internal logic
        }
        
        #endregion

        #region IDisposable
        
        /// <summary>
        /// Releases resources.
        /// </summary>
        public void Dispose()
        {
            // Cleanup
        }
        
        #endregion
    }
}
```

### MonoBehaviour Template
```csharp
using System;
using UnityEngine;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using VContainer;
using _Game.Scripts.Core.Interfaces;

namespace _Game.Scripts.Presentation.UI
{
    /// <summary>
    /// Handles the main menu user interface.
    /// </summary>
    [AddComponentMenu("Game/UI/Main Menu UI")]
    public class MainMenuUI : MonoBehaviour
    {
        #region Serialized Fields
        
        [BoxGroup("Buttons")]
        [SerializeField]
        [Tooltip("Button to start the game")]
        private Button _playButton;

        [BoxGroup("Buttons")]
        [SerializeField]
        [Tooltip("Button to open settings")]
        private Button _settingsButton;

        [BoxGroup("Panels")]
        [SerializeField]
        [Required("Settings panel is required")]
        private GameObject _settingsPanel;
        
        #endregion

        #region Dependencies
        
        [Inject] private readonly IGameStateService _gameStateService;
        [Inject] private readonly ISceneLoader _sceneLoader;
        
        #endregion

        #region Private Fields
        
        private bool _isInitialized;
        
        #endregion

        #region Unity Lifecycle
        
        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            Initialize();
        }

        private void OnDestroy()
        {
            Cleanup();
        }
        
        #endregion

        #region Initialization
        
        private void ValidateReferences()
        {
            Debug.Assert(_playButton != null, "Play button is not assigned!", this);
            Debug.Assert(_settingsButton != null, "Settings button is not assigned!", this);
        }

        private void Initialize()
        {
            if (_isInitialized) return;
            
            SubscribeToEvents();
            _isInitialized = true;
        }
        
        #endregion

        #region Event Subscriptions
        
        private void SubscribeToEvents()
        {
            _playButton.onClick.AddListener(OnPlayClicked);
            _settingsButton.onClick.AddListener(OnSettingsClicked);
        }

        private void UnsubscribeFromEvents()
        {
            _playButton.onClick.RemoveListener(OnPlayClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsClicked);
        }
        
        #endregion

        #region Event Handlers
        
        private void OnPlayClicked()
        {
            _sceneLoader.LoadSceneThroughLoading("Gameplay");
        }

        private void OnSettingsClicked()
        {
            _settingsPanel.SetActive(true);
        }
        
        #endregion

        #region Cleanup
        
        private void Cleanup()
        {
            UnsubscribeFromEvents();
        }
        
        #endregion

        #region Editor Only
        
        #if UNITY_EDITOR
        [Button("Test Play Click")]
        private void EditorTestPlayClick()
        {
            Debug.Log("Play button would be clicked");
        }
        #endif
        
        #endregion
    }
}
```

---

## ✅ Best Practices

### 1. Null Checking
```csharp
// Use null-conditional operator
OnHealthChanged?.Invoke(oldHealth, newHealth);

// Use null-coalescing operator
var player = _repository.Load() ?? new PlayerData();

// Use ArgumentNullException in constructors
public PlayerService(IPlayerRepository repository)
{
    _repository = repository ?? throw new ArgumentNullException(nameof(repository));
}
```

### 2. String Constants
```csharp
// ❌ Bad - Magic strings
SceneManager.LoadScene("MainMenu");
animator.SetTrigger("Attack");

// ✅ Good - Use constants
public static class SceneNames
{
    public const string MAIN_MENU = "MainMenu";
    public const string GAMEPLAY = "Gameplay";
}

public static class AnimatorParams
{
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int IsRunning = Animator.StringToHash("IsRunning");
}

// Usage
SceneManager.LoadScene(SceneNames.MAIN_MENU);
animator.SetTrigger(AnimatorParams.Attack);
```

### 3. Event Subscription Pattern
```csharp
private void OnEnable()
{
    _eventAggregator.Subscribe<PlayerDiedEvent>(OnPlayerDied);
}

private void OnDisable()
{
    _eventAggregator.Unsubscribe<PlayerDiedEvent>(OnPlayerDied);
}

// Or for Unity events
private void Start()
{
    _button.onClick.AddListener(OnButtonClicked);
}

private void OnDestroy()
{
    _button.onClick.RemoveListener(OnButtonClicked);
}
```

### 4. Async/Await with UniTask
```csharp
using Cysharp.Threading.Tasks;

public async UniTask LoadGameAsync()
{
    await LoadPlayerDataAsync();
    await LoadLevelAsync();
    await InitializeSystemsAsync();
}

// With cancellation
public async UniTask LoadWithCancellation(CancellationToken cancellationToken)
{
    await UniTask.Delay(1000, cancellationToken: cancellationToken);
}
```

### 5. Defensive Programming
```csharp
public void TakeDamage(int damage)
{
    // Guard clauses
    if (damage < 0)
    {
        Debug.LogWarning($"Negative damage value: {damage}. Using 0 instead.");
        damage = 0;
    }

    if (!IsAlive)
    {
        Debug.LogWarning("Cannot damage dead player.");
        return;
    }

    // Main logic
    _health = Mathf.Max(0, _health - damage);
}
```

---

## ❌ Anti-Patterns (Things to Avoid)

### 1. God Classes
```csharp
// ❌ Bad - One class doing everything
public class GameManager
{
    public void LoadPlayer() { }
    public void SavePlayer() { }
    public void PlaySound() { }
    public void UpdateUI() { }
    public void SpawnEnemy() { }
    public void CalculateDamage() { }
    // ... 50 more methods
}

// ✅ Good - Separated responsibilities
public class PlayerService { }
public class AudioService { }
public class UIManager { }
public class EnemySpawner { }
public class CombatService { }
```

### 2. Magic Numbers/Strings
```csharp
// ❌ Bad
if (health < 20) { }
SceneManager.LoadScene("Level1");

// ✅ Good
private const int LOW_HEALTH_THRESHOLD = 20;
if (health < LOW_HEALTH_THRESHOLD) { }
SceneManager.LoadScene(SceneNames.LEVEL_1);
```

### 3. Tight Coupling
```csharp
// ❌ Bad - Direct dependency
public class PlayerUI
{
    private PlayerService _playerService = new PlayerService();
}

// ✅ Good - Dependency Injection
public class PlayerUI
{
    [Inject] private readonly IPlayerService _playerService;
}
```

### 4. Exposing Internal State
```csharp
// ❌ Bad
public List<Item> Items;

// ✅ Good
private readonly List<Item> _items = new();
public IReadOnlyList<Item> Items => _items;
```

### 5. Empty Catch Blocks
```csharp
// ❌ Bad
try { DoSomething(); }
catch { }

// ✅ Good
try
{
    DoSomething();
}
catch (SpecificException ex)
{
    Debug.LogError($"Failed to do something: {ex.Message}");
    // Handle or rethrow
}
```

---

## 📋 Checklist for Code Review

Before submitting code, verify:

- [ ] Follows naming conventions
- [ ] Has XML documentation for public members
- [ ] Uses regions for organization
- [ ] Uses appropriate Odin Inspector attributes
- [ ] Has [Tooltip] for serialized fields
- [ ] No magic numbers/strings
- [ ] Proper null checking
- [ ] Events are unsubscribed in OnDestroy/OnDisable
- [ ] No tight coupling (uses interfaces)
- [ ] Follows layer dependency rules
- [ ] No business logic in Presentation layer
- [ ] No Unity dependencies in Core layer

---

> 📖 Kembali ke [Architecture Overview](./ARCHITECTURE.md)
