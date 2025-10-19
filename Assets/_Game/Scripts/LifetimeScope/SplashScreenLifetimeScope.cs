using VContainer;
using VContainer.Unity;
using _Game.Scripts.Presentation.UI;

public class SplashScreenLifetimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponentInHierarchy<SplashScreenUI>();
    }
}
