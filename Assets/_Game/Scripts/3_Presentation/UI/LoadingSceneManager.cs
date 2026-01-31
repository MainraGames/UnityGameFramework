using _Game.Scripts.Core.Interfaces;
using _Game.Scripts.Core.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace _Game.Scripts.Presentation.UI
{
    /// <summary>
    /// Manages the loading screen UI and progress display.
    /// </summary>
    public class LoadingSceneManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private float _minLoadingTime = 1f;

        private ISceneLoader _sceneLoader;
        private IGameStateService _gameStateService;
        private float _loadingStartTime;
        private bool _isLoading;

        [Inject]
        private void Construct(ISceneLoader sceneLoader, IGameStateService gameStateService)
        {
            _sceneLoader = sceneLoader;
            _gameStateService = gameStateService;
        }

        private void Start()
        {
            StartLoading();
        }

        private void Update()
        {
            if (!_isLoading) return;

            UpdateLoadingProgress();
        }

        private void StartLoading()
        {
            _isLoading = true;
            _gameStateService.SetState(GameState.Loading);
            _loadingStartTime = Time.time;
            _loadingText.text = "Loading...";
            
            // Mulai loading scene target
            _sceneLoader.LoadTargetSceneAsync();
        }

        private void UpdateLoadingProgress()
        {
            float progress = _sceneLoader.GetLoadingProgress();
            float elapsedTime = Time.time - _loadingStartTime;
            
            // Pastikan loading screen ditampilkan minimal _minLoadingTime
            if (elapsedTime < _minLoadingTime)
            {
                progress = Mathf.Min(progress, elapsedTime / _minLoadingTime);
            }

            _progressBar.value = progress;
            _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";

            // Aktifkan scene jika loading sudah selesai dan waktu minimum sudah terpenuhi
            if (progress >= 1f && elapsedTime >= _minLoadingTime)
            {
                _sceneLoader.ActivateLoadedScene();
            }
        }
    }
} 