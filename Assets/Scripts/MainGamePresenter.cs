using System;
using R3;
using VContainer.Unity;

public class MainGamePresenter : IStartable, IDisposable
{   
    private readonly MainGameModel _mainGameModel;
    private readonly MainGameView _mainGameView;
    private readonly ClickView _clickView;
        
    private DisposableBag _disposableBag;
    
    public MainGamePresenter(MainGameView view,MainGameModel model,ClickView clickView)
    {
        _mainGameModel = model;
        _mainGameView = view;
        _clickView = clickView;
    }
    
    public void Start()
    {
        Bind();
    }

    private void Bind()
    {   
        Observable.FromEvent(
                h => _clickView.OnClick += h,
                h => _clickView.OnClick -= h
            )
            .Subscribe(_ =>
            {
                _mainGameModel.AddClick(1); 
            })
            .AddTo(ref _disposableBag);
        _mainGameModel.ClickCount
            .Subscribe(v =>
            {
                _mainGameView.SetClickCountUI(v);
            }).AddTo(ref _disposableBag);
    }

    public void AddClick(int clickCount)
    {
        _mainGameModel.AddClick(clickCount);
    }


    public void Dispose()
    {
        _disposableBag.Dispose();
    }

    
}