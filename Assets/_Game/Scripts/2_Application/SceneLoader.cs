using System;
using _Game.Scripts.Core.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using _Game.Scripts.GameConfiguration;

namespace _Game.Scripts.Application
{
    /// <summary>
    /// Service for managing scene loading and unloading with comprehensive features.
    /// </summary>
    /// <remarks>
    /// Implements ISceneLoader and provides memory management, loading screen support,
    /// and async loading capabilities.
    /// </remarks>
    public class SceneLoader : ISceneLoader
    {
        private const string LOG_PREFIX = "[SceneLoader]";
        private const float MEMORY_CHECK_INTERVAL = 30f; // Check memory every 30 seconds
        private const float MEMORY_THRESHOLD = 0.8f; // 80% memory usage threshold
        
        private AsyncOperation _currentLoadingOperation;
        private string _currentLoadingSceneName;
        private bool _isLoading;
        private string _targetSceneName;
        private int _targetBuildIndex = -1;
        private float _lastMemoryCheckTime;
        private List<string> _loadedScenes;
        private bool _autoMemoryManagement;
        private readonly GameConfig _gameConfig;

        public SceneLoader(GameConfig gameConfig)
        {
            _loadedScenes = new List<string>();
            _autoMemoryManagement = true;
            _lastMemoryCheckTime = Time.time;
            _gameConfig = gameConfig;
            
            Debug.Log($"{LOG_PREFIX} Initialized with game config: {_gameConfig.ProductName} v{_gameConfig.ProductVersion}");
        }

        #region Basic Scene Loading

        /// <inheritdoc />
        public void LoadScene(string sceneName, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false)
        {
            ValidateSceneName(sceneName);
            var unityLoadMode = ConvertToUnityLoadMode(loadMode);
            LogLoading(sceneName, unityLoadMode);

            SceneManager.LoadScene(sceneName, unityLoadMode);
            TrackLoadedScene(sceneName);
            HandleUnloadUnusedAssets(unloadUnusedAssets);
            CheckAndManageMemory();
        }

        /// <inheritdoc />
        public void LoadSceneAsync(string sceneName, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false)
        {
            ValidateSceneName(sceneName);
            var unityLoadMode = ConvertToUnityLoadMode(loadMode);
            LogLoading(sceneName, unityLoadMode, true);

            _isLoading = true;
            _currentLoadingSceneName = sceneName;
            _currentLoadingOperation = SceneManager.LoadSceneAsync(sceneName, unityLoadMode);
            _currentLoadingOperation.allowSceneActivation = false;
            
            SetupAsyncOperation(_currentLoadingOperation, unloadUnusedAssets);
            _currentLoadingOperation.completed += (op) => {
                OnLoadingCompleted(op);
                TrackLoadedScene(sceneName);
                CheckAndManageMemory();
            };
        }

        /// <inheritdoc />
        public void LoadNextScene(SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false)
        {
            int nextSceneIndex = GetNextSceneIndex();
            LoadSceneByBuildIndex(nextSceneIndex, loadMode, unloadUnusedAssets);
        }

        /// <inheritdoc />
        public void LoadPreviousScene(SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false)
        {
            int previousSceneIndex = GetPreviousSceneIndex();
            LoadSceneByBuildIndex(previousSceneIndex, loadMode, unloadUnusedAssets);
        }

        /// <inheritdoc />
        public void ReloadScene(bool unloadUnusedAssets = false)
        {
            string currentSceneName = GetActiveSceneName();
            LoadScene(currentSceneName, SceneLoadMode.Single, unloadUnusedAssets);
        }

        #endregion

        #region Build Index Loading

        /// <inheritdoc />
        public void LoadSceneByBuildIndex(int buildIndex, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false)
        {
            ValidateBuildIndex(buildIndex);
            var unityLoadMode = ConvertToUnityLoadMode(loadMode);
            LogLoadingByIndex(buildIndex, unityLoadMode);

            SceneManager.LoadScene(buildIndex, unityLoadMode);
            HandleUnloadUnusedAssets(unloadUnusedAssets);
        }

        /// <inheritdoc />
        public void LoadSceneByBuildIndexAsync(int buildIndex, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false)
        {
            ValidateBuildIndex(buildIndex);
            var unityLoadMode = ConvertToUnityLoadMode(loadMode);
            LogLoadingByIndex(buildIndex, unityLoadMode, true);

            _isLoading = true;
            _currentLoadingSceneName = SceneManager.GetSceneByBuildIndex(buildIndex).name;
            _currentLoadingOperation = SceneManager.LoadSceneAsync(buildIndex, unityLoadMode);
            _currentLoadingOperation.allowSceneActivation = false;
            
            SetupAsyncOperation(_currentLoadingOperation, unloadUnusedAssets);
            _currentLoadingOperation.completed += OnLoadingCompleted;
        }

        #endregion

        #region Scene Unloading

        /// <inheritdoc />
        public void UnloadScene(string sceneName, bool unloadUnusedAssets = false)
        {
            ValidateSceneName(sceneName);
            var scene = GetSceneByName(sceneName);

            LogUnloading(sceneName);
            SceneManager.UnloadSceneAsync(scene);
            HandleUnloadUnusedAssets(unloadUnusedAssets);
        }

        /// <inheritdoc />
        public void UnloadSceneAsync(string sceneName, bool unloadUnusedAssets = false)
        {
            ValidateSceneName(sceneName);
            var scene = GetSceneByName(sceneName);

            LogUnloading(sceneName, true);
            var operation = SceneManager.UnloadSceneAsync(scene);
            SetupAsyncOperation(operation, unloadUnusedAssets);
        }

        #endregion

        #region Scene Information

        /// <inheritdoc />
        public string GetActiveSceneName()
        {
            return SceneManager.GetActiveScene().name;
        }

        /// <inheritdoc />
        public int GetActiveSceneBuildIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }

        /// <inheritdoc />
        public bool IsSceneLoaded(string sceneName)
        {
            ValidateSceneName(sceneName);
            var scene = SceneManager.GetSceneByName(sceneName);
            return scene.IsValid() && scene.isLoaded;
        }

        #endregion

        #region Loading Progress

        /// <inheritdoc />
        public float GetLoadingProgress()
        {
            if (!_isLoading || _currentLoadingOperation == null)
            {
                return 0f;
            }

            // Progress will stop at 0.9 until allowSceneActivation = true
            return Mathf.Clamp01(_currentLoadingOperation.progress / 0.9f);
        }

        /// <inheritdoc />
        public bool IsLoading()
        {
            return _isLoading;
        }

        /// <inheritdoc />
        public string GetLoadingSceneName()
        {
            return _isLoading ? _currentLoadingSceneName : string.Empty;
        }

        #endregion

        #region Loading Screen

        /// <inheritdoc />
        public void LoadSceneThroughLoading(string targetSceneName, string loadingSceneName = "Loading")
        {
            ValidateSceneName(targetSceneName);
            ValidateSceneName(loadingSceneName);

            _targetSceneName = targetSceneName;
            _targetBuildIndex = -1;
            LoadScene(loadingSceneName);
        }

        /// <inheritdoc />
        public void LoadSceneThroughLoadingAsync(string targetSceneName, string loadingSceneName = "Loading")
        {
            ValidateSceneName(targetSceneName);
            ValidateSceneName(loadingSceneName);

            _targetSceneName = targetSceneName;
            _targetBuildIndex = -1;
            LoadSceneAsync(loadingSceneName);
        }

        /// <inheritdoc />
        public void LoadSceneThroughLoadingByIndex(int targetBuildIndex, string loadingSceneName = "Loading")
        {
            ValidateBuildIndex(targetBuildIndex);
            ValidateSceneName(loadingSceneName);

            _targetBuildIndex = targetBuildIndex;
            _targetSceneName = string.Empty;
            LoadScene(loadingSceneName);
        }

        /// <inheritdoc />
        public void LoadSceneThroughLoadingByIndexAsync(int targetBuildIndex, string loadingSceneName = "Loading")
        {
            ValidateBuildIndex(targetBuildIndex);
            ValidateSceneName(loadingSceneName);

            _targetBuildIndex = targetBuildIndex;
            _targetSceneName = string.Empty;
            LoadSceneAsync(loadingSceneName);
        }

        /// <inheritdoc />
        public void ReloadSceneThroughLoading(string loadingSceneName = "Loading")
        {
            ValidateSceneName(loadingSceneName);
            
            // Save current scene as target
            _targetSceneName = GetActiveSceneName();
            _targetBuildIndex = -1;
            
            LogReloading(_targetSceneName);
            LoadScene(loadingSceneName);
        }

        /// <inheritdoc />
        public void ReloadSceneThroughLoadingAsync(string loadingSceneName = "Loading")
        {
            ValidateSceneName(loadingSceneName);
            
            // Save current scene as target
            _targetSceneName = GetActiveSceneName();
            _targetBuildIndex = -1;
            
            LogReloading(_targetSceneName, true);
            LoadSceneAsync(loadingSceneName);
        }

        /// <inheritdoc />
        public void LoadTargetSceneAsync()
        {
            if (!string.IsNullOrEmpty(_targetSceneName))
            {
                LoadSceneAsync(_targetSceneName);
            }
            else if (_targetBuildIndex >= 0)
            {
                LoadSceneByBuildIndexAsync(_targetBuildIndex);
            }
            else
            {
                Debug.LogError($"{LOG_PREFIX} No target scene to load");
            }
        }

        /// <inheritdoc />
        public void ActivateLoadedScene()
        {
            if (_isLoading && _currentLoadingOperation != null)
            {
                _currentLoadingOperation.allowSceneActivation = true;
            }
        }

        #endregion

        #region Memory Management

        /// <inheritdoc />
        public void SetAutoMemoryManagement(bool enable)
        {
            _autoMemoryManagement = enable;
            Debug.Log($"{LOG_PREFIX} Automatic memory management {(enable ? "enabled" : "disabled")}");
        }

        /// <inheritdoc />
        public void ForceMemoryCleanup()
        {
            Debug.Log($"{LOG_PREFIX} Forcing memory cleanup...");
            PerformMemoryCleanup();
        }

        /// <inheritdoc />
        public void CheckAndManageMemory()
        {
            if (!_autoMemoryManagement) return;

            float currentTime = Time.time;
            if (currentTime - _lastMemoryCheckTime < MEMORY_CHECK_INTERVAL) return;

            _lastMemoryCheckTime = currentTime;
            float memoryUsage = GetMemoryUsage();

            if (memoryUsage > MEMORY_THRESHOLD)
            {
                Debug.Log($"{LOG_PREFIX} High memory usage detected ({memoryUsage:P0}). Performing cleanup...");
                PerformMemoryCleanup();
            }
        }

        /// <summary>
        /// Gets the current memory usage as a percentage.
        /// </summary>
        private float GetMemoryUsage()
        {
            // Note: This is a simplified example. In a real implementation,
            // you might want to use more sophisticated memory tracking
            return (float)GC.GetTotalMemory(false) / GC.GetTotalMemory(true);
        }

        /// <summary>
        /// Performs memory cleanup operations.
        /// </summary>
        private void PerformMemoryCleanup()
        {
            // 1. Unload unused assets
            Resources.UnloadUnusedAssets();

            // 2. Force garbage collection
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 3. Unload scenes that haven't been used recently
            UnloadUnusedScenes();

            Debug.Log($"{LOG_PREFIX} Memory cleanup completed");
        }

        /// <summary>
        /// Unloads scenes that haven't been used recently.
        /// </summary>
        private void UnloadUnusedScenes()
        {
            // Implementation depends on your game's requirements
            // This is just an example
            for (int i = _loadedScenes.Count - 1; i >= 0; i--)
            {
                string sceneName = _loadedScenes[i];
                if (sceneName != _currentLoadingSceneName && 
                    sceneName != _targetSceneName &&
                    !IsSceneActive(sceneName))
                {
                    UnloadScene(sceneName, false);
                    _loadedScenes.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Checks if a scene is currently active.
        /// </summary>
        private bool IsSceneActive(string sceneName)
        {
            return SceneManager.GetActiveScene().name == sceneName;
        }

        /// <summary>
        /// Tracks a newly loaded scene.
        /// </summary>
        private void TrackLoadedScene(string sceneName)
        {
            if (!_loadedScenes.Contains(sceneName))
            {
                _loadedScenes.Add(sceneName);
            }
        }

        #endregion

        #region Private Methods

        private void OnLoadingCompleted(AsyncOperation operation)
        {
            _isLoading = false;
            _currentLoadingSceneName = string.Empty;
            _currentLoadingOperation = null;
        }

        private void ValidateSceneName(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName))
            {
                throw new ArgumentException($"{LOG_PREFIX} Scene name cannot be null or empty.");
            }
        }

        private void ValidateBuildIndex(int buildIndex)
        {
            if (buildIndex < 0 || buildIndex >= SceneManager.sceneCountInBuildSettings)
            {
                throw new ArgumentException($"{LOG_PREFIX} Invalid build index: {buildIndex}");
            }
        }

        private Scene GetSceneByName(string sceneName)
        {
            var scene = SceneManager.GetSceneByName(sceneName);
            if (!scene.IsValid())
            {
                throw new ArgumentException($"{LOG_PREFIX} Scene {sceneName} is not loaded.");
            }
            return scene;
        }

        private int GetNextSceneIndex()
        {
            int nextSceneIndex = GetActiveSceneBuildIndex() + 1;
            if (nextSceneIndex >= SceneManager.sceneCountInBuildSettings)
            {
                throw new InvalidOperationException($"{LOG_PREFIX} No more scenes to load.");
            }
            return nextSceneIndex;
        }

        private int GetPreviousSceneIndex()
        {
            int previousSceneIndex = GetActiveSceneBuildIndex() - 1;
            if (previousSceneIndex < 0)
            {
                throw new InvalidOperationException($"{LOG_PREFIX} Already at the first scene.");
            }
            return previousSceneIndex;
        }

        private void HandleUnloadUnusedAssets(bool shouldUnload)
        {
            if (shouldUnload)
            {
                Resources.UnloadUnusedAssets();
            }
        }

        private void SetupAsyncOperation(AsyncOperation operation, bool unloadUnusedAssets)
        {
            if (unloadUnusedAssets)
            {
                operation.completed += (op) => Resources.UnloadUnusedAssets();
            }
        }

        private void LogLoading(string sceneName, LoadSceneMode loadMode, bool isAsync = false)
        {
            Debug.Log($"{LOG_PREFIX} Loading scene {(isAsync ? "asynchronously" : "")}: {sceneName} in {loadMode} mode");
        }

        private void LogLoadingByIndex(int buildIndex, LoadSceneMode loadMode, bool isAsync = false)
        {
            Debug.Log($"{LOG_PREFIX} Loading scene {(isAsync ? "asynchronously" : "")} by build index: {buildIndex} in {loadMode} mode");
        }

        private void LogUnloading(string sceneName, bool isAsync = false)
        {
            Debug.Log($"{LOG_PREFIX} Unloading scene {(isAsync ? "asynchronously" : "")}: {sceneName}");
        }

        private void LogReloading(string sceneName, bool isAsync = false)
        {
            Debug.Log($"{LOG_PREFIX} Reloading scene {(isAsync ? "asynchronously" : "")}: {sceneName}");
        }

        /// <summary>
        /// Converts Core SceneLoadMode to Unity LoadSceneMode.
        /// </summary>
        private LoadSceneMode ConvertToUnityLoadMode(SceneLoadMode loadMode)
        {
            return loadMode == SceneLoadMode.Single ? LoadSceneMode.Single : LoadSceneMode.Additive;
        }

        #endregion
    }
}