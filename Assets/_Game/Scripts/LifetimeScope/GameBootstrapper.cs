using _Game.Scripts.Core.Interfaces;
using _Game.Scripts._2_Application.Services;
using UnityEngine;
using VContainer.Unity;
using MainraFramework.Parameter;

namespace _Game.Scripts.LifetimeScope
{
    /// <summary>
    /// Entry point for the game application.
    /// </summary>
    public class GameBootstrapper : IInitializable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IGameStateService _gameStateService;
        private readonly GameSettingsService _gameSettingsService;

        public GameBootstrapper(
            ISceneLoader sceneLoader, 
            IGameStateService gameStateService,
            GameSettingsService gameSettingsService)
        {
            _sceneLoader = sceneLoader;
            _gameStateService = gameStateService;
            _gameSettingsService = gameSettingsService;
        }

        public void Initialize()
        {
            // Settings are applied by GameSettingsService.Initialize() which runs before this
            // Additional game-specific initialization can be done here if needed
            _sceneLoader.LoadScene(Parameter.Scenes.SPLASHSCREEN);
        }
    }
}