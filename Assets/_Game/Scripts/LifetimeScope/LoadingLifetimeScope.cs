using _Game.Scripts.Presentation.UI;
using VContainer;
using VContainer.Unity;

namespace _Game.Scripts.LifetimeScope
{
    /// <summary>
    /// Dependency injection container for the Loading scene.
    /// </summary>
    public class LoadingLifetimeScope : VContainer.Unity.LifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterComponentInHierarchy<LoadingSceneManager>();
        }
    }
}
