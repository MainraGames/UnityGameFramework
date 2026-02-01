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
        
        [Inject] private GameSettingsService _settingsService;
        
        private void Start()
        {
            InitializeSliderValues();
            SetupEventListeners();
        }
        
        private void InitializeSliderValues()
        {
            if (_volumeMaster != null) _volumeMaster.value = _settingsService.MasterVolume;
            if (_volumeMusic != null) _volumeMusic.value = _settingsService.MusicVolume;
            if (_volumeSFX != null) _volumeSFX.value = _settingsService.SfxVolume;
            if (_volumeVoice != null) _volumeVoice.value = _settingsService.VoiceVolume;
            if (_volumeAmbient != null) _volumeAmbient.value = _settingsService.AmbientVolume;
        }
        
        private void SetupEventListeners()
        {
            if (_volumeMaster != null) _volumeMaster.onValueChanged.AddListener(UpdateMasterVolume);
            if (_volumeMusic != null) _volumeMusic.onValueChanged.AddListener(UpdateMusicVolume);
            if (_volumeSFX != null) _volumeSFX.onValueChanged.AddListener(UpdateSFXVolume);
            if (_volumeVoice != null) _volumeVoice.onValueChanged.AddListener(UpdateVoiceVolume);
            if (_volumeAmbient != null) _volumeAmbient.onValueChanged.AddListener(UpdateAmbientVolume);
        }
        
        private void UpdateMasterVolume(float value)
        {
            _settingsService.MasterVolume = value;
            _settingsService.ApplySoundSettings();
        }
        
        private void UpdateMusicVolume(float value)
        {
            _settingsService.MusicVolume = value;
        }
        
        private void UpdateSFXVolume(float value)
        {
            _settingsService.SfxVolume = value;
        }
        
        private void UpdateVoiceVolume(float value)
        {
            _settingsService.VoiceVolume = value;
        }
        
        private void UpdateAmbientVolume(float value)
        {
            _settingsService.AmbientVolume = value;
        }
        
        private void OnDisable()
        {
            SaveSettings();
            RemoveEventListeners();
        }
        
        private void SaveSettings()
        {
            _settingsService.SaveSettings();
        }
        
        private void RemoveEventListeners()
        {
            if (_volumeMaster != null) _volumeMaster.onValueChanged.RemoveListener(UpdateMasterVolume);
            if (_volumeMusic != null) _volumeMusic.onValueChanged.RemoveListener(UpdateMusicVolume);
            if (_volumeSFX != null) _volumeSFX.onValueChanged.RemoveListener(UpdateSFXVolume);
            if (_volumeVoice != null) _volumeVoice.onValueChanged.RemoveListener(UpdateVoiceVolume);
            if (_volumeAmbient != null) _volumeAmbient.onValueChanged.RemoveListener(UpdateAmbientVolume);
        }
        
        /// <summary>
        /// Resets sound settings to defaults.
        /// Call this from a "Reset" button in the UI.
        /// </summary>
        public void ResetSoundToDefaults()
        {
            _settingsService.ResetSoundToDefaults();
            InitializeSliderValues();
        }
    }
}
