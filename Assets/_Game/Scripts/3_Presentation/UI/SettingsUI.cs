using _Game.Scripts._2_Application.Services;
using _Game.Scripts.GameConfiguration;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using Sirenix.OdinInspector;

namespace _Game.Scripts.Presentation.UI
{
    /// <summary>
    /// Flexible UI component for displaying and modifying game settings.
    /// All UI elements are optional - assign only the settings you need in the Inspector.
    /// Automatically syncs with GameSettingsService events.
    /// </summary>
    public class SettingsUI : BaseUI
    {
        #region Sound Settings
        
        [TabGroup("Settings", "Sound")]
        [InfoBox("All sound settings are optional. Assign only the controls you need.")]
        [LabelText("Master Volume")]
        [SerializeField] private Slider _volumeMaster;
        
        [TabGroup("Settings", "Sound")]
        [LabelText("Music Volume")]
        [SerializeField] private Slider _volumeMusic;
        
        [TabGroup("Settings", "Sound")]
        [LabelText("SFX Volume")]
        [SerializeField] private Slider _volumeSFX;
        
        [TabGroup("Settings", "Sound")]
        [LabelText("Voice Volume")]
        [SerializeField] private Slider _volumeVoice;
        
        [TabGroup("Settings", "Sound")]
        [LabelText("Ambient Volume")]
        [SerializeField] private Slider _volumeAmbient;
        
        [TabGroup("Settings", "Sound")]
        [LabelText("Mute Toggle")]
        [SerializeField] private Toggle _muteAudioToggle;
        
        #endregion
        
        #region Graphics Settings
        
        [TabGroup("Settings", "Graphics")]
        [InfoBox("All graphics settings are optional. Assign only the controls you need.")]
        [LabelText("Quality Level")]
        [SerializeField] private TMP_Dropdown _qualityDropdown;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Resolution")]
        [SerializeField] private TMP_Dropdown _resolutionDropdown;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Fullscreen")]
        [SerializeField] private Toggle _fullscreenToggle;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("VSync")]
        [SerializeField] private Toggle _vsyncToggle;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Frame Rate")]
        [SerializeField] private TMP_Dropdown _frameRateDropdown;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Anti-Aliasing")]
        [SerializeField] private TMP_Dropdown _antiAliasingDropdown;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Shadow Quality")]
        [SerializeField] private TMP_Dropdown _shadowQualityDropdown;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Texture Quality")]
        [SerializeField] private TMP_Dropdown _textureQualityDropdown;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Brightness")]
        [SerializeField] private Slider _brightnessSlider;
        
        [TabGroup("Settings", "Graphics")]
        [LabelText("Gamma")]
        [SerializeField] private Slider _gammaSlider;
        
        #endregion
        
        #region Value Display Labels
        
        [TabGroup("Settings", "Labels")]
        [InfoBox("Optional labels to display current values.")]
        [LabelText("Master Volume Label")]
        [SerializeField] private TextMeshProUGUI _masterVolumeLabel;
        
        [TabGroup("Settings", "Labels")]
        [LabelText("Music Volume Label")]
        [SerializeField] private TextMeshProUGUI _musicVolumeLabel;
        
        [TabGroup("Settings", "Labels")]
        [LabelText("SFX Volume Label")]
        [SerializeField] private TextMeshProUGUI _sfxVolumeLabel;
        
        [TabGroup("Settings", "Labels")]
        [LabelText("Voice Volume Label")]
        [SerializeField] private TextMeshProUGUI _voiceVolumeLabel;
        
        [TabGroup("Settings", "Labels")]
        [LabelText("Ambient Volume Label")]
        [SerializeField] private TextMeshProUGUI _ambientVolumeLabel;
        
        [TabGroup("Settings", "Labels")]
        [LabelText("Brightness Label")]
        [SerializeField] private TextMeshProUGUI _brightnessLabel;
        
        [TabGroup("Settings", "Labels")]
        [LabelText("Gamma Label")]
        [SerializeField] private TextMeshProUGUI _gammaLabel;
        
        #endregion
        
        #region Reset Buttons
        
        [TabGroup("Settings", "Buttons")]
        [InfoBox("Optional reset buttons for restoring default values.")]
        [LabelText("Reset Sound")]
        [SerializeField] private Button _resetSoundButton;
        
        [TabGroup("Settings", "Buttons")]
        [LabelText("Reset Graphics")]
        [SerializeField] private Button _resetGraphicsButton;
        
        [TabGroup("Settings", "Buttons")]
        [LabelText("Reset All")]
        [SerializeField] private Button _resetAllButton;
        
        #endregion
        
        #region Dependencies (Injected)
        
        [Inject] private GameSettingsService _settingsService;
        
        #endregion
        
        #region Private Fields
        
        /// <summary>
        /// Flag to prevent event triggering during value initialization.
        /// </summary>
        private bool _isInitializing;
        
        /// <summary>
        /// Flag to ensure dropdown options are only initialized once.
        /// </summary>
        private bool _isDropdownInitialized;
        
        #endregion
        
        #region Unity Lifecycle
        
        /// <summary>
        /// Called when the UI is enabled. Re-initializes values and event listeners.
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            
            // Initialize dropdowns only once
            if (!_isDropdownInitialized)
            {
                InitializeDropdownOptions();
                _isDropdownInitialized = true;
            }
            
            InitializeAllValues();
            SetupAllEventListeners();
            SubscribeToServiceEvents();
        }
        
        /// <summary>
        /// Called when the UI is disabled. Saves settings and removes event listeners.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            
            SaveSettings();
            RemoveAllEventListeners();
            UnsubscribeFromServiceEvents();
        }
        
        #endregion
        
        #region Initialization
        
        private void InitializeDropdownOptions()
        {
            // Quality dropdown
            if (_qualityDropdown != null)
            {
                _qualityDropdown.ClearOptions();
                _qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(_settingsService.GetQualityLevelNames()));
            }
            
            // Resolution dropdown
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.ClearOptions();
                var resolutions = _settingsService.GetAvailableResolutions();
                var resolutionOptions = new System.Collections.Generic.List<string>();
                foreach (var res in resolutions)
                {
                    resolutionOptions.Add($"{res.width} x {res.height} @ {res.refreshRateRatio}Hz");
                }
                _resolutionDropdown.AddOptions(resolutionOptions);
            }
            
            // Frame rate dropdown
            if (_frameRateDropdown != null)
            {
                _frameRateDropdown.ClearOptions();
                _frameRateDropdown.AddOptions(new System.Collections.Generic.List<string> 
                { 
                    "30 FPS", "60 FPS", "90 FPS", "120 FPS", "144 FPS", "Unlimited" 
                });
            }
            
            // Anti-aliasing dropdown
            if (_antiAliasingDropdown != null)
            {
                _antiAliasingDropdown.ClearOptions();
                _antiAliasingDropdown.AddOptions(new System.Collections.Generic.List<string> 
                { 
                    "Off", "2x MSAA", "4x MSAA", "8x MSAA" 
                });
            }
            
            // Shadow quality dropdown
            if (_shadowQualityDropdown != null)
            {
                _shadowQualityDropdown.ClearOptions();
                _shadowQualityDropdown.AddOptions(new System.Collections.Generic.List<string> 
                { 
                    "Off", "Low", "Medium", "High", "Ultra" 
                });
            }
            
            // Texture quality dropdown
            if (_textureQualityDropdown != null)
            {
                _textureQualityDropdown.ClearOptions();
                _textureQualityDropdown.AddOptions(new System.Collections.Generic.List<string> 
                { 
                    "Full", "Half", "Quarter", "Eighth" 
                });
            }
        }
        
        private void InitializeAllValues()
        {
            _isInitializing = true;
            
            InitializeSoundValues();
            InitializeGraphicsValues();
            
            _isInitializing = false;
        }
        
        private void InitializeSoundValues()
        {
            // Volume sliders
            if (_volumeMaster != null)
            {
                _volumeMaster.value = _settingsService.MasterVolume;
                UpdateVolumeLabel(_masterVolumeLabel, _settingsService.MasterVolume);
            }
            
            if (_volumeMusic != null)
            {
                _volumeMusic.value = _settingsService.MusicVolume;
                UpdateVolumeLabel(_musicVolumeLabel, _settingsService.MusicVolume);
            }
            
            if (_volumeSFX != null)
            {
                _volumeSFX.value = _settingsService.SfxVolume;
                UpdateVolumeLabel(_sfxVolumeLabel, _settingsService.SfxVolume);
            }
            
            if (_volumeVoice != null)
            {
                _volumeVoice.value = _settingsService.VoiceVolume;
                UpdateVolumeLabel(_voiceVolumeLabel, _settingsService.VoiceVolume);
            }
            
            if (_volumeAmbient != null)
            {
                _volumeAmbient.value = _settingsService.AmbientVolume;
                UpdateVolumeLabel(_ambientVolumeLabel, _settingsService.AmbientVolume);
            }
            
            // Mute toggle
            if (_muteAudioToggle != null)
            {
                _muteAudioToggle.isOn = _settingsService.MuteAudio;
            }
        }
        
        private void InitializeGraphicsValues()
        {
            // Quality
            if (_qualityDropdown != null)
            {
                _qualityDropdown.value = _settingsService.QualityLevel;
            }
            
            // Resolution
            if (_resolutionDropdown != null)
            {
                _resolutionDropdown.value = _settingsService.ResolutionIndex;
            }
            
            // Fullscreen
            if (_fullscreenToggle != null)
            {
                _fullscreenToggle.isOn = _settingsService.FullScreen;
            }
            
            // VSync
            if (_vsyncToggle != null)
            {
                _vsyncToggle.isOn = _settingsService.VSync;
            }
            
            // Frame rate
            if (_frameRateDropdown != null)
            {
                _frameRateDropdown.value = FrameRateToDropdownIndex(_settingsService.TargetFrameRate);
            }
            
            // Anti-aliasing
            if (_antiAliasingDropdown != null)
            {
                _antiAliasingDropdown.value = AntiAliasingToDropdownIndex(_settingsService.AntiAliasing);
            }
            
            // Shadow quality
            if (_shadowQualityDropdown != null)
            {
                _shadowQualityDropdown.value = (int)_settingsService.ShadowQuality;
            }
            
            // Texture quality
            if (_textureQualityDropdown != null)
            {
                _textureQualityDropdown.value = _settingsService.TextureQuality;
            }
            
            // Brightness
            if (_brightnessSlider != null)
            {
                _brightnessSlider.value = _settingsService.Brightness;
                UpdateVolumeLabel(_brightnessLabel, _settingsService.Brightness);
            }
            
            // Gamma
            if (_gammaSlider != null)
            {
                _gammaSlider.value = _settingsService.Gamma;
                UpdateVolumeLabel(_gammaLabel, _settingsService.Gamma);
            }
        }
        
        #endregion
        
        #region Event Listeners Setup
        
        private void SetupAllEventListeners()
        {
            // Sound listeners
            if (_volumeMaster != null) _volumeMaster.onValueChanged.AddListener(OnMasterVolumeChanged);
            if (_volumeMusic != null) _volumeMusic.onValueChanged.AddListener(OnMusicVolumeChanged);
            if (_volumeSFX != null) _volumeSFX.onValueChanged.AddListener(OnSFXVolumeChanged);
            if (_volumeVoice != null) _volumeVoice.onValueChanged.AddListener(OnVoiceVolumeChanged);
            if (_volumeAmbient != null) _volumeAmbient.onValueChanged.AddListener(OnAmbientVolumeChanged);
            if (_muteAudioToggle != null) _muteAudioToggle.onValueChanged.AddListener(OnMuteAudioChanged);
            
            // Graphics listeners
            if (_qualityDropdown != null) _qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            if (_resolutionDropdown != null) _resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);
            if (_fullscreenToggle != null) _fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
            if (_vsyncToggle != null) _vsyncToggle.onValueChanged.AddListener(OnVSyncChanged);
            if (_frameRateDropdown != null) _frameRateDropdown.onValueChanged.AddListener(OnFrameRateChanged);
            if (_antiAliasingDropdown != null) _antiAliasingDropdown.onValueChanged.AddListener(OnAntiAliasingChanged);
            if (_shadowQualityDropdown != null) _shadowQualityDropdown.onValueChanged.AddListener(OnShadowQualityChanged);
            if (_textureQualityDropdown != null) _textureQualityDropdown.onValueChanged.AddListener(OnTextureQualityChanged);
            if (_brightnessSlider != null) _brightnessSlider.onValueChanged.AddListener(OnBrightnessChanged);
            if (_gammaSlider != null) _gammaSlider.onValueChanged.AddListener(OnGammaChanged);
            
            // Reset buttons
            if (_resetSoundButton != null) _resetSoundButton.onClick.AddListener(ResetSoundToDefaults);
            if (_resetGraphicsButton != null) _resetGraphicsButton.onClick.AddListener(ResetGraphicsToDefaults);
            if (_resetAllButton != null) _resetAllButton.onClick.AddListener(ResetAllToDefaults);
        }
        
        private void RemoveAllEventListeners()
        {
            // Sound listeners
            if (_volumeMaster != null) _volumeMaster.onValueChanged.RemoveListener(OnMasterVolumeChanged);
            if (_volumeMusic != null) _volumeMusic.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            if (_volumeSFX != null) _volumeSFX.onValueChanged.RemoveListener(OnSFXVolumeChanged);
            if (_volumeVoice != null) _volumeVoice.onValueChanged.RemoveListener(OnVoiceVolumeChanged);
            if (_volumeAmbient != null) _volumeAmbient.onValueChanged.RemoveListener(OnAmbientVolumeChanged);
            if (_muteAudioToggle != null) _muteAudioToggle.onValueChanged.RemoveListener(OnMuteAudioChanged);
            
            // Graphics listeners
            if (_qualityDropdown != null) _qualityDropdown.onValueChanged.RemoveListener(OnQualityChanged);
            if (_resolutionDropdown != null) _resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
            if (_fullscreenToggle != null) _fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
            if (_vsyncToggle != null) _vsyncToggle.onValueChanged.RemoveListener(OnVSyncChanged);
            if (_frameRateDropdown != null) _frameRateDropdown.onValueChanged.RemoveListener(OnFrameRateChanged);
            if (_antiAliasingDropdown != null) _antiAliasingDropdown.onValueChanged.RemoveListener(OnAntiAliasingChanged);
            if (_shadowQualityDropdown != null) _shadowQualityDropdown.onValueChanged.RemoveListener(OnShadowQualityChanged);
            if (_textureQualityDropdown != null) _textureQualityDropdown.onValueChanged.RemoveListener(OnTextureQualityChanged);
            if (_brightnessSlider != null) _brightnessSlider.onValueChanged.RemoveListener(OnBrightnessChanged);
            if (_gammaSlider != null) _gammaSlider.onValueChanged.RemoveListener(OnGammaChanged);
            
            // Reset buttons
            if (_resetSoundButton != null) _resetSoundButton.onClick.RemoveListener(ResetSoundToDefaults);
            if (_resetGraphicsButton != null) _resetGraphicsButton.onClick.RemoveListener(ResetGraphicsToDefaults);
            if (_resetAllButton != null) _resetAllButton.onClick.RemoveListener(ResetAllToDefaults);
        }
        
        private void SubscribeToServiceEvents()
        {
            _settingsService.OnSoundSettingsChanged += OnServiceSoundSettingsChanged;
            _settingsService.OnGraphicsSettingsChanged += OnServiceGraphicsSettingsChanged;
            _settingsService.OnSettingsReset += OnServiceSettingsReset;
        }
        
        private void UnsubscribeFromServiceEvents()
        {
            _settingsService.OnSoundSettingsChanged -= OnServiceSoundSettingsChanged;
            _settingsService.OnGraphicsSettingsChanged -= OnServiceGraphicsSettingsChanged;
            _settingsService.OnSettingsReset -= OnServiceSettingsReset;
        }
        
        #endregion
        
        #region Sound Event Handlers
        
        private void OnMasterVolumeChanged(float value)
        {
            if (_isInitializing) return;
            _settingsService.MasterVolume = value;
            _settingsService.ApplySoundSettings();
            UpdateVolumeLabel(_masterVolumeLabel, value);
        }
        
        private void OnMusicVolumeChanged(float value)
        {
            if (_isInitializing) return;
            _settingsService.MusicVolume = value;
            UpdateVolumeLabel(_musicVolumeLabel, value);
        }
        
        private void OnSFXVolumeChanged(float value)
        {
            if (_isInitializing) return;
            _settingsService.SfxVolume = value;
            UpdateVolumeLabel(_sfxVolumeLabel, value);
        }
        
        private void OnVoiceVolumeChanged(float value)
        {
            if (_isInitializing) return;
            _settingsService.VoiceVolume = value;
            UpdateVolumeLabel(_voiceVolumeLabel, value);
        }
        
        private void OnAmbientVolumeChanged(float value)
        {
            if (_isInitializing) return;
            _settingsService.AmbientVolume = value;
            UpdateVolumeLabel(_ambientVolumeLabel, value);
        }
        
        private void OnMuteAudioChanged(bool value)
        {
            if (_isInitializing) return;
            _settingsService.MuteAudio = value;
            _settingsService.ApplySoundSettings();
        }
        
        #endregion
        
        #region Graphics Event Handlers
        
        private void OnQualityChanged(int value)
        {
            if (_isInitializing) return;
            _settingsService.QualityLevel = value;
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnResolutionChanged(int value)
        {
            if (_isInitializing) return;
            _settingsService.ResolutionIndex = value;
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnFullscreenChanged(bool value)
        {
            if (_isInitializing) return;
            _settingsService.FullScreen = value;
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnVSyncChanged(bool value)
        {
            if (_isInitializing) return;
            _settingsService.VSync = value;
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnFrameRateChanged(int value)
        {
            if (_isInitializing) return;
            _settingsService.TargetFrameRate = DropdownIndexToFrameRate(value);
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnAntiAliasingChanged(int value)
        {
            if (_isInitializing) return;
            _settingsService.AntiAliasing = DropdownIndexToAntiAliasing(value);
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnShadowQualityChanged(int value)
        {
            if (_isInitializing) return;
            _settingsService.ShadowQuality = (ShadowQualityOption)value;
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnTextureQualityChanged(int value)
        {
            if (_isInitializing) return;
            _settingsService.TextureQuality = value;
            _settingsService.ApplyGraphicsSettings();
        }
        
        private void OnBrightnessChanged(float value)
        {
            if (_isInitializing) return;
            _settingsService.Brightness = value;
            UpdateVolumeLabel(_brightnessLabel, value);
        }
        
        private void OnGammaChanged(float value)
        {
            if (_isInitializing) return;
            _settingsService.Gamma = value;
            UpdateVolumeLabel(_gammaLabel, value);
        }
        
        #endregion
        
        #region Service Event Handlers (Sync from external changes)
        
        private void OnServiceSoundSettingsChanged()
        {
            if (!_isInitializing)
            {
                _isInitializing = true;
                InitializeSoundValues();
                _isInitializing = false;
            }
        }
        
        private void OnServiceGraphicsSettingsChanged()
        {
            if (!_isInitializing)
            {
                _isInitializing = true;
                InitializeGraphicsValues();
                _isInitializing = false;
            }
        }
        
        private void OnServiceSettingsReset()
        {
            InitializeAllValues();
        }
        
        #endregion
        
        #region Reset Methods
        
        /// <summary>
        /// Resets sound settings to defaults.
        /// </summary>
        public void ResetSoundToDefaults()
        {
            _settingsService.ResetSoundToDefaults();
            InitializeSoundValues();
        }
        
        /// <summary>
        /// Resets graphics settings to defaults.
        /// </summary>
        public void ResetGraphicsToDefaults()
        {
            _settingsService.ResetGraphicsToDefaults();
            InitializeGraphicsValues();
        }
        
        /// <summary>
        /// Resets all settings to defaults.
        /// </summary>
        public void ResetAllToDefaults()
        {
            _settingsService.ResetToDefaults();
            InitializeAllValues();
        }
        
        #endregion
        
        #region Save
        
        private void SaveSettings()
        {
            _settingsService.SaveSettings();
        }
        
        #endregion
        
        #region Utility Methods
        
        private void UpdateVolumeLabel(TextMeshProUGUI label, float value)
        {
            if (label != null)
            {
                label.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
        }
        
        private int FrameRateToDropdownIndex(int frameRate)
        {
            return frameRate switch
            {
                30 => 0,
                60 => 1,
                90 => 2,
                120 => 3,
                144 => 4,
                _ => 5 // Unlimited (-1 or other)
            };
        }
        
        private int DropdownIndexToFrameRate(int index)
        {
            return index switch
            {
                0 => 30,
                1 => 60,
                2 => 90,
                3 => 120,
                4 => 144,
                _ => -1 // Unlimited
            };
        }
        
        private int AntiAliasingToDropdownIndex(int antiAliasing)
        {
            return antiAliasing switch
            {
                0 => 0,
                2 => 1,
                4 => 2,
                8 => 3,
                _ => 0
            };
        }
        
        private int DropdownIndexToAntiAliasing(int index)
        {
            return index switch
            {
                0 => 0,
                1 => 2,
                2 => 4,
                3 => 8,
                _ => 0
            };
        }
        
        #endregion
    }
}
