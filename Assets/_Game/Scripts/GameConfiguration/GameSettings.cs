using Sirenix.OdinInspector;
using UnityEngine;

namespace _Game.Scripts.GameConfiguration
{
    /// <summary>
    /// ScriptableObject containing default game settings values (audio, graphics, etc.).
    /// This is a pure data class - no logic, no dependencies on DI frameworks.
    /// </summary>
    /// <remarks>
    /// This ScriptableObject holds DEFAULT values that can be configured in the Unity Editor.
    /// Runtime values are managed by GameSettingsService and persisted via GameSettingsSaveObject.
    /// Create via: Assets > Create > Game > Game Settings
    /// </remarks>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "Game/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        #region Serialized Fields - Audio
        
        [FoldoutGroup("Sound Settings")]
        [TitleGroup("Sound Settings/Master")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Default master volume level")]
        private float _defaultMasterVolume = 1f;
        
        [FoldoutGroup("Sound Settings")]
        [TitleGroup("Sound Settings/Music")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Default music volume level")]
        private float _defaultMusicVolume = 1f;
        
        [FoldoutGroup("Sound Settings")]
        [TitleGroup("Sound Settings/SFX")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Default sound effects volume level")]
        private float _defaultSfxVolume = 1f;
        
        [FoldoutGroup("Sound Settings")]
        [TitleGroup("Sound Settings/Voice")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Default voice/dialogue volume level")]
        private float _defaultVoiceVolume = 1f;
        
        [FoldoutGroup("Sound Settings")]
        [TitleGroup("Sound Settings/Ambient")]
        [SerializeField]
        [Range(0f, 1f)]
        [Tooltip("Default ambient sound volume level")]
        private float _defaultAmbientVolume = 1f;
        
        [FoldoutGroup("Sound Settings")]
        [SerializeField]
        [Tooltip("Default mute all audio state")]
        private bool _defaultMuteAudio;
        
        #endregion

        #region Serialized Fields - Graphics
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Quality")]
        [SerializeField]
        [Tooltip("Default quality level index from QualitySettings")]
        [PropertyRange(0, 5)]
        private int _defaultQualityLevel = 2;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Display")]
        [SerializeField]
        [Tooltip("Default fullscreen mode")]
        private bool _defaultFullScreen = true;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Display")]
        [SerializeField]
        [Tooltip("Default resolution index from Screen.resolutions")]
        private int _defaultResolutionIndex;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Performance")]
        [SerializeField]
        [Tooltip("Default target frame rate (0 for unlimited, -1 for platform default)")]
        [PropertyRange(-1, 240)]
        private int _defaultTargetFrameRate = 60;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Performance")]
        [SerializeField]
        [Tooltip("Default vertical sync state")]
        private bool _defaultVSync = true;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Rendering")]
        [SerializeField]
        [Tooltip("Default anti-aliasing level (0=Off, 2, 4, 8)")]
        [ValueDropdown("GetAntiAliasingOptions")]
        private int _defaultAntiAliasing = 2;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Rendering")]
        [SerializeField]
        [Tooltip("Default shadow quality")]
        private ShadowQualityOption _defaultShadowQuality = ShadowQualityOption.Medium;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Rendering")]
        [SerializeField]
        [Tooltip("Default texture quality (0=Full, 1=Half, 2=Quarter, 3=Eighth)")]
        [PropertyRange(0, 3)]
        private int _defaultTextureQuality;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Effects")]
        [SerializeField]
        [Tooltip("Default brightness level")]
        [PropertyRange(0f, 2f)]
        private float _defaultBrightness = 1f;
        
        [FoldoutGroup("Graphics Settings")]
        [TitleGroup("Graphics Settings/Effects")]
        [SerializeField]
        [Tooltip("Default gamma level")]
        [PropertyRange(0.5f, 2f)]
        private float _defaultGamma = 1f;
        
        #endregion

        #region Properties - Sound Defaults
        
        /// <summary>Gets the default master volume (0-1).</summary>
        public float DefaultMasterVolume => _defaultMasterVolume;
        
        /// <summary>Gets the default music volume (0-1).</summary>
        public float DefaultMusicVolume => _defaultMusicVolume;
        
        /// <summary>Gets the default SFX volume (0-1).</summary>
        public float DefaultSfxVolume => _defaultSfxVolume;
        
        /// <summary>Gets the default voice volume (0-1).</summary>
        public float DefaultVoiceVolume => _defaultVoiceVolume;
        
        /// <summary>Gets the default ambient volume (0-1).</summary>
        public float DefaultAmbientVolume => _defaultAmbientVolume;
        
        /// <summary>Gets the default mute audio state.</summary>
        public bool DefaultMuteAudio => _defaultMuteAudio;
        
        #endregion

        #region Properties - Graphics Defaults
        
        /// <summary>Gets the default quality level.</summary>
        public int DefaultQualityLevel => _defaultQualityLevel;
        
        /// <summary>Gets the default fullscreen state.</summary>
        public bool DefaultFullScreen => _defaultFullScreen;
        
        /// <summary>Gets the default resolution index.</summary>
        public int DefaultResolutionIndex => _defaultResolutionIndex;
        
        /// <summary>Gets the default target frame rate.</summary>
        public int DefaultTargetFrameRate => _defaultTargetFrameRate;
        
        /// <summary>Gets the default VSync state.</summary>
        public bool DefaultVSync => _defaultVSync;
        
        /// <summary>Gets the default anti-aliasing level.</summary>
        public int DefaultAntiAliasing => _defaultAntiAliasing;
        
        /// <summary>Gets the default shadow quality.</summary>
        public ShadowQualityOption DefaultShadowQuality => _defaultShadowQuality;
        
        /// <summary>Gets the default texture quality.</summary>
        public int DefaultTextureQuality => _defaultTextureQuality;
        
        /// <summary>Gets the default brightness.</summary>
        public float DefaultBrightness => _defaultBrightness;
        
        /// <summary>Gets the default gamma.</summary>
        public float DefaultGamma => _defaultGamma;
        
        #endregion

        #region Editor Helpers
        
#if UNITY_EDITOR
        private static int[] GetAntiAliasingOptions() => new[] { 0, 2, 4, 8 };
#endif
        
        #endregion
    }
    
    /// <summary>
    /// Shadow quality options for graphics settings.
    /// </summary>
    public enum ShadowQualityOption
    {
        Off = 0,
        Low = 1,
        Medium = 2,
        High = 3,
        Ultra = 4
    }
}

