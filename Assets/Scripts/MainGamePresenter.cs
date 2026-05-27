using System;
using R3;
using VContainer.Unity;

public class MainGamePresenter : IStartable, IDisposable
{   
    private readonly MainGameView _mainGameView;
    private readonly MainGameModel _mainGameModel;
    private readonly ClickView _clickView;
    private readonly BallSimulationManager _ballSimulationManager;
        
    private DisposableBag _disposableBag;
    
    public MainGamePresenter(MainGameView view,
        BallSimulationManager ballSimulationManager,
        MainGameModel mainGameModel,
        ClickView clickView)
    {
        _mainGameView = view;
        _mainGameModel = mainGameModel;
        _ballSimulationManager = ballSimulationManager;
        _clickView = clickView;
    }
    
    public void Start()
    {
        Bind();
    }

    private void Bind()
    {
        _ballSimulationManager.SetBallsPerClick(_mainGameModel.BallsPerClick.CurrentValue);
        _ballSimulationManager.SetPinPerHit(_mainGameModel.PinPerHit.CurrentValue);

        _mainGameModel.Money.Subscribe(money =>
        {
            _mainGameView.SetMoneyText(money);
        }).AddTo(ref _disposableBag);
        
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ => _ballSimulationManager.HandleClickSpawn())
            .AddTo(ref _disposableBag);

        _mainGameModel.NextBallCost.CombineLatest(_mainGameModel.BallsPerClick, (cost, count) => (cost, count))
            .Subscribe(x =>
            {
                _mainGameView.SetBallText(x.cost, x.count);
            }).AddTo(ref _disposableBag);

        _mainGameModel.NextPinCost.CombineLatest(_mainGameModel.PinPerHit, (cost, count) => (cost, count))
            .Subscribe(x =>
            {
                _mainGameView.SetPinText(x.cost, x.count);
            }).AddTo(ref _disposableBag);
        
        _ballSimulationManager.OnGetMoney += OnGetMoney;
        _clickView.OnClick += OnClick;
        _mainGameView.OnBallButtonPressed += OnBallButtonPressed;
        _mainGameView.OnPinButtonPressed += OnPinButtonPressed;
    }
    private void OnClick()
    {
        _ballSimulationManager.HandleClickSpawn();
    }
    private void OnGetMoney(uint money)
    {
        _mainGameModel.Money.Value += money;
    }
    private void OnPinButtonPressed()
    {
        if (_mainGameModel.IsPinBuyable())
        {
            _ballSimulationManager
              .SetPinPerHit(_mainGameModel.PinPerHit.CurrentValue);
        }
        else
        {
            _mainGameView.ShowNoMoneyText().Forget();
        }
    }
    private void OnBallButtonPressed()
    {
        if (_mainGameModel.IsBallBuyable())
        {
            _ballSimulationManager
                .SetBallsPerClick(_mainGameModel.BallsPerClick.CurrentValue);
        }
        else
        {
            _mainGameView.ShowNoMoneyText().Forget();
        }
    }

    public void Dispose()
    {
        _disposableBag.Dispose();
    }
}