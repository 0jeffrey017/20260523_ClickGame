using VContainer;
using VContainer.Unity;

public class GameLifeTimeScope : LifetimeScope
{
    protected override void Configure(IContainerBuilder builder)
    {
        builder.Register<MainGameModel>(Lifetime.Singleton);
        builder.RegisterComponentInHierarchy<BallSimulationManager>();
        builder.RegisterComponentInHierarchy<MainGameView>();
        builder.RegisterComponentInHierarchy<ClickView>();
        
        builder.RegisterEntryPoint<MainGamePresenter>(Lifetime.Scoped);
    }
}
