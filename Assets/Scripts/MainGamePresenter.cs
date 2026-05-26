using System;
using R3;
using VContainer.Unity;

public class MainGamePresenter : IStartable, IDisposable
{   
    private readonly MainGameView _mainGameView;
    private readonly BallSimulationManager _ballSimulationManager;
        
    private DisposableBag _disposableBag;
    
    public MainGamePresenter(MainGameView view,
        BallSimulationManager ballSimulationManager)
    {
        _mainGameView = view;
        _ballSimulationManager = ballSimulationManager;
    }
    
    public void Start()
    {
        Bind();
    }

    private void Bind()
    {   
        _ballSimulationManager.Money
            .Subscribe(v =>
            {
                _mainGameView.SetMoneyText(v);
            }).AddTo(ref _disposableBag);
    }
    
    public void Dispose()
    {
        _disposableBag.Dispose();
    }
}