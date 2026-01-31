namespace _Game.Scripts.Core.Interfaces
{
    /// <summary>
    /// Interface for comprehensive scene management with loading screen support.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface is designed to be Unity-agnostic in the Core layer.
    /// Unity-specific types (AsyncOperation, LoadSceneMode) are abstracted away
    /// using primitive types and custom enums.
    /// </para>
    /// <para>
    /// The implementation in Application layer will handle the Unity-specific conversions.
    /// </para>
    /// </remarks>
    public interface ISceneLoader
    {
        #region Memory Management

        /// <summary>
        /// Enables or disables automatic memory management.
        /// </summary>
        /// <param name="enable">Whether to enable automatic memory management.</param>
        void SetAutoMemoryManagement(bool enable);

        /// <summary>
        /// Manually triggers memory cleanup.
        /// </summary>
        void ForceMemoryCleanup();

        #endregion

        #region Basic Scene Loading

        /// <summary>
        /// Loads a scene by name with loading mode options.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="loadMode">Scene loading mode (Single or Additive).</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void LoadScene(string sceneName, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false);

        /// <summary>
        /// Loads a scene asynchronously with loading mode options.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load.</param>
        /// <param name="loadMode">Scene loading mode (Single or Additive).</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void LoadSceneAsync(string sceneName, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false);

        /// <summary>
        /// Loads the next scene in build settings.
        /// </summary>
        /// <param name="loadMode">Scene loading mode (Single or Additive).</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void LoadNextScene(SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false);

        /// <summary>
        /// Loads the previous scene in build settings.
        /// </summary>
        /// <param name="loadMode">Scene loading mode (Single or Additive).</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void LoadPreviousScene(SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false);

        /// <summary>
        /// Reloads the current scene.
        /// </summary>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void ReloadScene(bool unloadUnusedAssets = false);

        #endregion

        #region Build Index Loading

        /// <summary>
        /// Loads a scene by build index.
        /// </summary>
        /// <param name="buildIndex">Build index of the scene to load.</param>
        /// <param name="loadMode">Scene loading mode (Single or Additive).</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void LoadSceneByBuildIndex(int buildIndex, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false);

        /// <summary>
        /// Loads a scene asynchronously by build index.
        /// </summary>
        /// <param name="buildIndex">Build index of the scene to load.</param>
        /// <param name="loadMode">Scene loading mode (Single or Additive).</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void LoadSceneByBuildIndexAsync(int buildIndex, SceneLoadMode loadMode = SceneLoadMode.Single, bool unloadUnusedAssets = false);

        #endregion

        #region Scene Unloading

        /// <summary>
        /// Unloads a scene by name.
        /// </summary>
        /// <param name="sceneName">Name of the scene to unload.</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void UnloadScene(string sceneName, bool unloadUnusedAssets = false);

        /// <summary>
        /// Unloads a scene asynchronously by name.
        /// </summary>
        /// <param name="sceneName">Name of the scene to unload.</param>
        /// <param name="unloadUnusedAssets">Whether to clean up unused assets.</param>
        void UnloadSceneAsync(string sceneName, bool unloadUnusedAssets = false);

        #endregion

        #region Scene Information

        /// <summary>
        /// Gets the name of the active scene.
        /// </summary>
        /// <returns>Name of the active scene.</returns>
        string GetActiveSceneName();

        /// <summary>
        /// Gets the build index of the active scene.
        /// </summary>
        /// <returns>Build index of the active scene.</returns>
        int GetActiveSceneBuildIndex();

        /// <summary>
        /// Checks if a scene is loaded.
        /// </summary>
        /// <param name="sceneName">Name of the scene to check.</param>
        /// <returns>True if the scene is loaded, false otherwise.</returns>
        bool IsSceneLoaded(string sceneName);

        #endregion

        #region Loading Progress

        /// <summary>
        /// Gets the current loading progress (0-1).
        /// </summary>
        /// <returns>Loading progress value between 0 and 1.</returns>
        float GetLoadingProgress();

        /// <summary>
        /// Checks if a scene is currently loading.
        /// </summary>
        /// <returns>True if a scene is loading, false otherwise.</returns>
        bool IsLoading();

        /// <summary>
        /// Gets the name of the scene being loaded.
        /// </summary>
        /// <returns>Name of the scene being loaded, or empty string if not loading.</returns>
        string GetLoadingSceneName();

        #endregion

        #region Loading Screen

        /// <summary>
        /// Loads a target scene through a loading screen.
        /// </summary>
        /// <param name="targetSceneName">Name of the scene to load.</param>
        /// <param name="loadingSceneName">Name of the loading screen scene.</param>
        void LoadSceneThroughLoading(string targetSceneName, string loadingSceneName = "Loading");

        /// <summary>
        /// Loads a target scene asynchronously through a loading screen.
        /// </summary>
        /// <param name="targetSceneName">Name of the scene to load.</param>
        /// <param name="loadingSceneName">Name of the loading screen scene.</param>
        void LoadSceneThroughLoadingAsync(string targetSceneName, string loadingSceneName = "Loading");

        /// <summary>
        /// Loads a target scene through a loading screen by build index.
        /// </summary>
        /// <param name="targetBuildIndex">Build index of the scene to load.</param>
        /// <param name="loadingSceneName">Name of the loading screen scene.</param>
        void LoadSceneThroughLoadingByIndex(int targetBuildIndex, string loadingSceneName = "Loading");

        /// <summary>
        /// Loads a target scene asynchronously through a loading screen by build index.
        /// </summary>
        /// <param name="targetBuildIndex">Build index of the scene to load.</param>
        /// <param name="loadingSceneName">Name of the loading screen scene.</param>
        void LoadSceneThroughLoadingByIndexAsync(int targetBuildIndex, string loadingSceneName = "Loading");

        /// <summary>
        /// Reloads the current scene through a loading screen.
        /// </summary>
        /// <param name="loadingSceneName">Name of the loading screen scene.</param>
        void ReloadSceneThroughLoading(string loadingSceneName = "Loading");

        /// <summary>
        /// Reloads the current scene asynchronously through a loading screen.
        /// </summary>
        /// <param name="loadingSceneName">Name of the loading screen scene.</param>
        void ReloadSceneThroughLoadingAsync(string loadingSceneName = "Loading");

        /// <summary>
        /// Loads the target scene asynchronously (for use with loading screens).
        /// </summary>
        void LoadTargetSceneAsync();

        /// <summary>
        /// Activates the loaded scene.
        /// </summary>
        void ActivateLoadedScene();

        #endregion
    }

    /// <summary>
    /// Scene loading mode - mirrors Unity's LoadSceneMode without Unity dependency.
    /// </summary>
    public enum SceneLoadMode
    {
        /// <summary>
        /// Closes all current loaded scenes and loads a scene.
        /// </summary>
        Single = 0,
        
        /// <summary>
        /// Adds the scene to the current loaded scenes.
        /// </summary>
        Additive = 1
    }
}
