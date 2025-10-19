using _Game.Scripts.Core.Interfaces;
using _Game.Scripts.GameConfiguration;
using UnityEngine;
using VContainer.Unity;
using MainraFramework.Parameter;
using VContainer;

public class GameBootstrapper : IInitializable
{
    
    private readonly ISceneLoader _sceneLoader;
    private readonly IGameStateService _gameStateService;
    
    [Inject] private GameConfig gameConfig;

    public GameBootstrapper(ISceneLoader sceneLoader, IGameStateService gameStateService)
    {
        _sceneLoader = sceneLoader;
        _gameStateService = gameStateService;
    }

    public void Initialize()
    {
        Application.targetFrameRate = gameConfig.GameSettings.targetFrameRate;
        QualitySettings.vSyncCount = gameConfig.GameSettings.vSync ? 1 : 0;
        _sceneLoader.LoadScene(Parameter.Scenes.SPLASHSCREEN);
    }
}