using CarterGames.Assets.SaveManager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.GameConfiguration
{
    /// <summary>
    /// SaveObject for persisting game settings data.
    /// Uses Carter Games Save Manager for persistence.
    /// </summary>
    /// <remarks>
    /// This class holds the runtime values that can be modified by the player.
    /// Create via: Assets > Create > Save Objects > Game Settings Save Object
    /// </remarks>
    [CreateAssetMenu(fileName = "GameSettingsSaveObject", menuName = "Save Objects/Game Settings Save Object")]
    public class GameSettingsSaveObject : SaveObject
    {
        #region Save Keys
        
        private const string KEY_MASTER_VOLUME = "settings_masterVolume";
        private const string KEY_MUSIC_VOLUME = "settings_musicVolume";
        private const string KEY_SFX_VOLUME = "settings_sfxVolume";
        private const string KEY_VOICE_VOLUME = "settings_voiceVolume";
        private const string KEY_AMBIENT_VOLUME = "settings_ambientVolume";
        private const string KEY_MUTE_AUDIO = "settings_muteAudio";
        private const string KEY_QUALITY_LEVEL = "settings_qualityLevel";
        private const string KEY_FULLSCREEN = "settings_fullScreen";
        private const string KEY_RESOLUTION_INDEX = "settings_resolutionIndex";
        private const string KEY_TARGET_FRAMERATE = "settings_targetFrameRate";
        private const string KEY_VSYNC = "settings_vSync";
        private const string KEY_ANTI_ALIASING = "settings_antiAliasing";
        private const string KEY_SHADOW_QUALITY = "settings_shadowQuality";
        private const string KEY_TEXTURE_QUALITY = "settings_textureQuality";
        private const string KEY_BRIGHTNESS = "settings_brightness";
        private const string KEY_GAMMA = "settings_gamma";
        
        #endregion

        #region Save Values - Sound Settings
        
        [FoldoutGroup("Sound Settings")]
        [SerializeField]
        [Tooltip("Master volume level (0-1)")]
        private SaveValue<float> masterVolume = new SaveValue<float>(KEY_MASTER_VOLUME, 1f);
        
        [FoldoutGroup("Sound Settings")]
        [SerializeField]
        [Tooltip("Music volume level (0-1)")]
        private SaveValue<float> musicVolume = new SaveValue<float>(KEY_MUSIC_VOLUME, 1f);
        
        [FoldoutGroup("Sound Settings")]
        [SerializeField]
        [Tooltip("Sound effects volume level (0-1)")]
        private SaveValue<float> sfxVolume = new SaveValue<float>(KEY_SFX_VOLUME, 1f);
        
        [FoldoutGroup("Sound Settings")]
        [SerializeField]
        [Tooltip("Voice/dialogue volume level (0-1)")]
        private SaveValue<float> voiceVolume = new SaveValue<float>(KEY_VOICE_VOLUME, 1f);
        
        [FoldoutGroup("Sound Settings")]
        [SerializeField]
        [Tooltip("Ambient sound volume level (0-1)")]
        private SaveValue<float> ambientVolume = new SaveValue<float>(KEY_AMBIENT_VOLUME, 1f);
        
        [FoldoutGroup("Sound Settings")]
        [SerializeField]
        [Tooltip("Mute all audio")]
        private SaveValue<bool> muteAudio = new SaveValue<bool>(KEY_MUTE_AUDIO, false);
        
        #endregion

        #region Save Values - Graphics Settings
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Quality")]
        [SerializeField]
        [Tooltip("Quality level index from QualitySettings")]
        private SaveValue<int> qualityLevel = new SaveValue<int>(KEY_QUALITY_LEVEL, 2);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Display")]
        [SerializeField]
        [Tooltip("Enable fullscreen mode")]
        private SaveValue<bool> fullScreen = new SaveValue<bool>(KEY_FULLSCREEN, true);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Display")]
        [SerializeField]
        [Tooltip("Resolution index from Screen.resolutions")]
        private SaveValue<int> resolutionIndex = new SaveValue<int>(KEY_RESOLUTION_INDEX, 0);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Performance")]
        [SerializeField]
        [Tooltip("Target frame rate (-1 for platform default, 0 for unlimited)")]
        private SaveValue<int> targetFrameRate = new SaveValue<int>(KEY_TARGET_FRAMERATE, 60);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Performance")]
        [SerializeField]
        [Tooltip("Enable vertical sync")]
        private SaveValue<bool> vSync = new SaveValue<bool>(KEY_VSYNC, true);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Rendering")]
        [SerializeField]
        [Tooltip("Anti-aliasing level (0=Off, 2, 4, 8)")]
        private SaveValue<int> antiAliasing = new SaveValue<int>(KEY_ANTI_ALIASING, 2);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Rendering")]
        [SerializeField]
        [Tooltip("Shadow quality setting")]
        private SaveValue<int> shadowQuality = new SaveValue<int>(KEY_SHADOW_QUALITY, (int)ShadowQualityOption.Medium);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Rendering")]
        [SerializeField]
        [Tooltip("Texture quality (0=Full, 1=Half, 2=Quarter, 3=Eighth)")]
        private SaveValue<int> textureQuality = new SaveValue<int>(KEY_TEXTURE_QUALITY, 0);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Effects")]
        [SerializeField]
        [Tooltip("Brightness level")]
        private SaveValue<float> brightness = new SaveValue<float>(KEY_BRIGHTNESS, 1f);
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Effects")]
        [SerializeField]
        [Tooltip("Gamma level")]
        private SaveValue<float> gamma = new SaveValue<float>(KEY_GAMMA, 1f);
        
        #endregion

        #region Properties - Sound
        
        /// <summary>Gets or sets the master volume (0-1).</summary>
        public float MasterVolume
        {
            get => masterVolume.Value;
            set => masterVolume.Value = Mathf.Clamp01(value);
        }
        
        /// <summary>Gets or sets the music volume (0-1).</summary>
        public float MusicVolume
        {
            get => musicVolume.Value;
            set => musicVolume.Value = Mathf.Clamp01(value);
        }
        
        /// <summary>Gets or sets the SFX volume (0-1).</summary>
        public float SfxVolume
        {
            get => sfxVolume.Value;
            set => sfxVolume.Value = Mathf.Clamp01(value);
        }
        
        /// <summary>Gets or sets the voice volume (0-1).</summary>
        public float VoiceVolume
        {
            get => voiceVolume.Value;
            set => voiceVolume.Value = Mathf.Clamp01(value);
        }
        
        /// <summary>Gets or sets the ambient volume (0-1).</summary>
        public float AmbientVolume
        {
            get => ambientVolume.Value;
            set => ambientVolume.Value = Mathf.Clamp01(value);
        }
        
        /// <summary>Gets or sets whether audio is muted.</summary>
        public bool MuteAudio
        {
            get => muteAudio.Value;
            set => muteAudio.Value = value;
        }
        
        #endregion

        #region Properties - Graphics
        
        /// <summary>Gets or sets the quality level.</summary>
        public int QualityLevel
        {
            get => qualityLevel.Value;
            set => qualityLevel.Value = Mathf.Clamp(value, 0, QualitySettings.names.Length - 1);
        }
        
        /// <summary>Gets or sets whether fullscreen is enabled.</summary>
        public bool FullScreen
        {
            get => fullScreen.Value;
            set => fullScreen.Value = value;
        }
        
        /// <summary>Gets or sets the resolution index.</summary>
        public int ResolutionIndex
        {
            get => resolutionIndex.Value;
            set => resolutionIndex.Value = Mathf.Max(0, value);
        }
        
        /// <summary>Gets or sets the target frame rate.</summary>
        public int TargetFrameRate
        {
            get => targetFrameRate.Value;
            set => targetFrameRate.Value = Mathf.Max(-1, value);
        }
        
        /// <summary>Gets or sets whether VSync is enabled.</summary>
        public bool VSync
        {
            get => vSync.Value;
            set => vSync.Value = value;
        }
        
        /// <summary>Gets or sets the anti-aliasing level.</summary>
        public int AntiAliasing
        {
            get => antiAliasing.Value;
            set => antiAliasing.Value = ValidateAntiAliasing(value);
        }
        
        /// <summary>Gets or sets the shadow quality.</summary>
        public ShadowQualityOption ShadowQuality
        {
            get => (ShadowQualityOption)shadowQuality.Value;
            set => shadowQuality.Value = (int)value;
        }
        
        /// <summary>Gets or sets the texture quality.</summary>
        public int TextureQuality
        {
            get => textureQuality.Value;
            set => textureQuality.Value = Mathf.Clamp(value, 0, 3);
        }
        
        /// <summary>Gets or sets the brightness level.</summary>
        public float Brightness
        {
            get => brightness.Value;
            set => brightness.Value = Mathf.Clamp(value, 0f, 2f);
        }
        
        /// <summary>Gets or sets the gamma level.</summary>
        public float Gamma
        {
            get => gamma.Value;
            set => gamma.Value = Mathf.Clamp(value, 0.5f, 2f);
        }
        
        #endregion

        #region Public Methods
        
        /// <summary>
        /// Loads default values from GameSettings ScriptableObject.
        /// </summary>
        /// <param name="defaults">The default settings to load from.</param>
        public void LoadDefaults(GameSettings defaults)
        {
            if (defaults == null) return;
            
            // Sound defaults
            masterVolume.Value = defaults.DefaultMasterVolume;
            musicVolume.Value = defaults.DefaultMusicVolume;
            sfxVolume.Value = defaults.DefaultSfxVolume;
            voiceVolume.Value = defaults.DefaultVoiceVolume;
            ambientVolume.Value = defaults.DefaultAmbientVolume;
            muteAudio.Value = defaults.DefaultMuteAudio;
            
            // Graphics defaults
            qualityLevel.Value = defaults.DefaultQualityLevel;
            fullScreen.Value = defaults.DefaultFullScreen;
            resolutionIndex.Value = defaults.DefaultResolutionIndex;
            targetFrameRate.Value = defaults.DefaultTargetFrameRate;
            vSync.Value = defaults.DefaultVSync;
            antiAliasing.Value = defaults.DefaultAntiAliasing;
            shadowQuality.Value = (int)defaults.DefaultShadowQuality;
            textureQuality.Value = defaults.DefaultTextureQuality;
            brightness.Value = defaults.DefaultBrightness;
            gamma.Value = defaults.DefaultGamma;
        }
        
        #endregion

        #region Private Methods
        
        private static int ValidateAntiAliasing(int value)
        {
            // Only valid values: 0, 2, 4, 8
            return value switch
            {
                0 => 0,
                <= 2 => 2,
                <= 4 => 4,
                _ => 8
            };
        }
        
        #endregion
    }
}