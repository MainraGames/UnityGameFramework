using _Game.Scripts.Application;
using _Game.Scripts.Core.Interfaces;
using _Game.Scripts.GameConfiguration;
using _Game.Scripts._2_Application.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace _Game.Scripts.LifetimeScope
{
    /// <summary>
    /// Main dependency injection container for the game.
    /// </summary>
    public class GameLifetimeScope : VContainer.Unity.LifetimeScope
    {
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private GameSettingsSaveObject _gameSettingsSaveObject;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterEntryPoint<GameBootstrapper>();

            // Application Layer - Services
            builder.Register<EventAggregator>(Lifetime.Singleton).As<IEventAggregator>();
            builder.Register<IGameStateService, GameStateService>(Lifetime.Singleton);
            builder.Register<ISceneLoader, SceneLoader>(Lifetime.Singleton);
            
            // Game Settings Service (with IInitializable for automatic initialization)
            builder.Register<GameSettingsService>(Lifetime.Singleton)
                .WithParameter(_gameConfig.GameSettings)
                .WithParameter(_gameSettingsSaveObject)
                .AsImplementedInterfaces()
                .AsSelf();

            // Configuration
            builder.RegisterInstance(_gameConfig).As<GameConfig>();
            builder.RegisterInstance(_gameSettingsSaveObject);
        }

        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}
