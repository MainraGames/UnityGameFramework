using System;
using _Game.Scripts.GameConfiguration;
using CarterGames.Assets.SaveManager;
using UnityEngine;
using VContainer.Unity;

namespace _Game.Scripts._2_Application.Services
{
    /// <summary>
    /// Application service responsible for managing game settings.
    /// Handles initialization, applying, saving, and loading settings.
    /// </summary>
    /// <remarks>
    /// This service follows the Clean Architecture pattern:
    /// - GameSettings (GameConfiguration): Default values configured in Unity Editor
    /// - GameSettingsSaveObject: Runtime values persisted via Carter Games Save Manager
    /// - GameSettingsService (Application): Business logic for managing settings
    /// </remarks>
    public class GameSettingsService : IInitializable, IDisposable
    {
        #region Dependencies
        
        private readonly GameSettings _defaultSettings;
        private readonly GameSettingsSaveObject _saveObject;
        
        #endregion

        #region Events
        
        /// <summary>Raised when any sound setting changes.</summary>
        public event Action OnSoundSettingsChanged;
        
        /// <summary>Raised when any graphics setting changes.</summary>
        public event Action OnGraphicsSettingsChanged;
        
        /// <summary>Raised when settings are reset to defaults.</summary>
        public event Action OnSettingsReset;
        
        #endregion

        #region Constructor
        
        /// <summary>
        /// Creates a new GameSettingsService instance.
        /// </summary>
        /// <param name="defaultSettings">The default settings ScriptableObject.</param>
        /// <param name="saveObject">The save object for persisting settings.</param>
        public GameSettingsService(GameSettings defaultSettings, GameSettingsSaveObject saveObject)
        {
            _defaultSettings = defaultSettings ?? throw new ArgumentNullException(nameof(defaultSettings));
            _saveObject = saveObject ?? throw new ArgumentNullException(nameof(saveObject));
        }
        
        #endregion

        #region Properties - Sound (Read from SaveObject)
        
        /// <summary>Gets or sets the master volume (0-1).</summary>
        public float MasterVolume
        {
            get => _saveObject.MasterVolume;
            set
            {
                _saveObject.MasterVolume = value;
                OnSoundSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the music volume (0-1).</summary>
        public float MusicVolume
        {
            get => _saveObject.MusicVolume;
            set
            {
                _saveObject.MusicVolume = value;
                OnSoundSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the SFX volume (0-1).</summary>
        public float SfxVolume
        {
            get => _saveObject.SfxVolume;
            set
            {
                _saveObject.SfxVolume = value;
                OnSoundSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the voice volume (0-1).</summary>
        public float VoiceVolume
        {
            get => _saveObject.VoiceVolume;
            set
            {
                _saveObject.VoiceVolume = value;
                OnSoundSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the ambient volume (0-1).</summary>
        public float AmbientVolume
        {
            get => _saveObject.AmbientVolume;
            set
            {
                _saveObject.AmbientVolume = value;
                OnSoundSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets whether audio is muted.</summary>
        public bool MuteAudio
        {
            get => _saveObject.MuteAudio;
            set
            {
                _saveObject.MuteAudio = value;
                OnSoundSettingsChanged?.Invoke();
            }
        }
        
        #endregion

        #region Properties - Graphics (Read from SaveObject)
        
        /// <summary>Gets or sets the quality level.</summary>
        public int QualityLevel
        {
            get => _saveObject.QualityLevel;
            set
            {
                _saveObject.QualityLevel = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets whether fullscreen is enabled.</summary>
        public bool FullScreen
        {
            get => _saveObject.FullScreen;
            set
            {
                _saveObject.FullScreen = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the resolution index.</summary>
        public int ResolutionIndex
        {
            get => _saveObject.ResolutionIndex;
            set
            {
                _saveObject.ResolutionIndex = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the target frame rate.</summary>
        public int TargetFrameRate
        {
            get => _saveObject.TargetFrameRate;
            set
            {
                _saveObject.TargetFrameRate = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets whether VSync is enabled.</summary>
        public bool VSync
        {
            get => _saveObject.VSync;
            set
            {
                _saveObject.VSync = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the anti-aliasing level.</summary>
        public int AntiAliasing
        {
            get => _saveObject.AntiAliasing;
            set
            {
                _saveObject.AntiAliasing = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the shadow quality.</summary>
        public ShadowQualityOption ShadowQuality
        {
            get => _saveObject.ShadowQuality;
            set
            {
                _saveObject.ShadowQuality = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the texture quality.</summary>
        public int TextureQuality
        {
            get => _saveObject.TextureQuality;
            set
            {
                _saveObject.TextureQuality = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the brightness level.</summary>
        public float Brightness
        {
            get => _saveObject.Brightness;
            set
            {
                _saveObject.Brightness = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        /// <summary>Gets or sets the gamma level.</summary>
        public float Gamma
        {
            get => _saveObject.Gamma;
            set
            {
                _saveObject.Gamma = value;
                OnGraphicsSettingsChanged?.Invoke();
            }
        }
        
        #endregion

        #region IInitializable Implementation
        
        /// <summary>
        /// Initializes the settings service.
        /// Called automatically by VContainer after dependency injection.
        /// </summary>
        public void Initialize()
        {
            // Load saved settings and apply them
            SaveManager.Load();
            ApplyAllSettings();
            
            Debug.Log("[GameSettingsService] Initialized and settings applied.");
        }
        
        #endregion

        #region IDisposable Implementation
        
        /// <summary>
        /// Disposes the service and saves settings.
        /// </summary>
        public void Dispose()
        {
            SaveSettings();
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Applies all current settings to the game.
        /// </summary>
        public void ApplyAllSettings()
        {
            ApplySoundSettings();
            ApplyGraphicsSettings();
        }
        
        /// <summary>
        /// Applies current sound settings to the game.
        /// </summary>
        public void ApplySoundSettings()
        {
            // Apply master volume to AudioListener
            AudioListener.volume = _saveObject.MuteAudio ? 0f : _saveObject.MasterVolume;
            
            // Note: Individual audio channels (Music, SFX, Voice, Ambient) 
            // should be applied through your audio system (e.g., Audio Mixer, BroAudio)
        }
        
        /// <summary>
        /// Applies current graphics settings to the game.
        /// </summary>
        public void ApplyGraphicsSettings()
        {
            // Quality Level
            QualitySettings.SetQualityLevel(_saveObject.QualityLevel, true);
            
            // Frame Rate and VSync
            UnityEngine.Application.targetFrameRate = _saveObject.TargetFrameRate;
            QualitySettings.vSyncCount = _saveObject.VSync ? 1 : 0;
            
            // Anti-Aliasing
            QualitySettings.antiAliasing = _saveObject.AntiAliasing;
            
            // Texture Quality
            QualitySettings.globalTextureMipmapLimit = _saveObject.TextureQuality;
            
            // Shadow Quality
            ApplyShadowQuality(_saveObject.ShadowQuality);
            
            // Resolution and Fullscreen
            ApplyResolution();
        }
        
        /// <summary>
        /// Saves current settings to persistent storage.
        /// </summary>
        public void SaveSettings()
        {
            SaveManager.Save();
            Debug.Log("[GameSettingsService] Settings saved.");
        }
        
        /// <summary>
        /// Resets all settings to default values and applies them.
        /// </summary>
        public void ResetToDefaults()
        {
            _saveObject.LoadDefaults(_defaultSettings);
            ApplyAllSettings();
            SaveSettings();
            
            OnSettingsReset?.Invoke();
            Debug.Log("[GameSettingsService] Settings reset to defaults.");
        }
        
        /// <summary>
        /// Resets sound settings to default values.
        /// </summary>
        public void ResetSoundToDefaults()
        {
            _saveObject.MasterVolume = _defaultSettings.DefaultMasterVolume;
            _saveObject.MusicVolume = _defaultSettings.DefaultMusicVolume;
            _saveObject.SfxVolume = _defaultSettings.DefaultSfxVolume;
            _saveObject.VoiceVolume = _defaultSettings.DefaultVoiceVolume;
            _saveObject.AmbientVolume = _defaultSettings.DefaultAmbientVolume;
            _saveObject.MuteAudio = _defaultSettings.DefaultMuteAudio;
            
            ApplySoundSettings();
            OnSoundSettingsChanged?.Invoke();
        }
        
        /// <summary>
        /// Resets graphics settings to default values.
        /// </summary>
        public void ResetGraphicsToDefaults()
        {
            _saveObject.QualityLevel = _defaultSettings.DefaultQualityLevel;
            _saveObject.FullScreen = _defaultSettings.DefaultFullScreen;
            _saveObject.ResolutionIndex = _defaultSettings.DefaultResolutionIndex;
            _saveObject.TargetFrameRate = _defaultSettings.DefaultTargetFrameRate;
            _saveObject.VSync = _defaultSettings.DefaultVSync;
            _saveObject.AntiAliasing = _defaultSettings.DefaultAntiAliasing;
            _saveObject.ShadowQuality = _defaultSettings.DefaultShadowQuality;
            _saveObject.TextureQuality = _defaultSettings.DefaultTextureQuality;
            _saveObject.Brightness = _defaultSettings.DefaultBrightness;
            _saveObject.Gamma = _defaultSettings.DefaultGamma;
            
            ApplyGraphicsSettings();
            OnGraphicsSettingsChanged?.Invoke();
        }
        
        /// <summary>
        /// Gets available screen resolutions.
        /// </summary>
        /// <returns>Array of available resolutions.</returns>
        public Resolution[] GetAvailableResolutions()
        {
            return Screen.resolutions;
        }
        
        /// <summary>
        /// Gets available quality level names.
        /// </summary>
        /// <returns>Array of quality level names.</returns>
        public string[] GetQualityLevelNames()
        {
            return QualitySettings.names;
        }
        
        #endregion

        #region Private Methods
        
        private void ApplyResolution()
        {
            Resolution[] resolutions = Screen.resolutions;
            
            if (resolutions.Length > 0 && _saveObject.ResolutionIndex < resolutions.Length)
            {
                Resolution resolution = resolutions[_saveObject.ResolutionIndex];
                Screen.SetResolution(resolution.width, resolution.height, _saveObject.FullScreen);
            }
        }
        
        private void ApplyShadowQuality(ShadowQualityOption quality)
        {
            switch (quality)
            {
                case ShadowQualityOption.Off:
                    QualitySettings.shadows = UnityEngine.ShadowQuality.Disable;
                    break;
                case ShadowQualityOption.Low:
                    QualitySettings.shadows = UnityEngine.ShadowQuality.HardOnly;
                    QualitySettings.shadowResolution = ShadowResolution.Low;
                    break;
                case ShadowQualityOption.Medium:
                    QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.Medium;
                    break;
                case ShadowQualityOption.High:
                    QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.High;
                    break;
                case ShadowQualityOption.Ultra:
                    QualitySettings.shadows = UnityEngine.ShadowQuality.All;
                    QualitySettings.shadowResolution = ShadowResolution.VeryHigh;
                    break;
            }
        }
        
        #endregion
    }
}
