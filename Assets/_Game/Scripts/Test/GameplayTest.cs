using _Game.Scripts.Core.Interfaces;
using UnityEngine;
using VContainer;

namespace _Game.Scripts.Test
{
    /// <summary>
    /// Test class for gameplay functionality.
    /// </summary>
    public class GameplayTest : MonoBehaviour
    {
        [Inject] private ISceneLoader _sceneLoader;

        void Start()
        {
            //_sceneLoader.ReloadSceneThroughLoading();
        }

        void Update()
        {
        }
    }
}
