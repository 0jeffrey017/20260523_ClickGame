using R3;


public class MainGameModel
{
    public ReactiveProperty<uint> Money = new ReactiveProperty<uint>(0);

    private ReactiveProperty<uint> _nextBallCost = new ReactiveProperty<uint>(10);
    public ReadOnlyReactiveProperty<uint> NextBallCost => _nextBallCost;
    private ReactiveProperty<uint> _nextPinCost = new ReactiveProperty<uint>(10);
    public ReadOnlyReactiveProperty<uint> NextPinCost => _nextPinCost;
    
    private ReactiveProperty<uint> _ballsPerClick = new ReactiveProperty<uint>(1);
    public ReadOnlyReactiveProperty<uint> BallsPerClick => _ballsPerClick;
    
    private ReactiveProperty<uint> _pinPerHit = new ReactiveProperty<uint>(1);
    public ReadOnlyReactiveProperty<uint> PinPerHit => _pinPerHit;

    public bool IsBallBuyable()
    {
        if(Money.Value < _nextBallCost.Value)return false;

        Money.Value -= _nextBallCost.Value;
        _nextBallCost.Value *= 5;
        _ballsPerClick.Value++;
        return true;
    }
    
    public bool IsPinBuyable()
    {
        if(Money.Value < _nextPinCost.Value)return false;

        Money.Value -= _nextPinCost.Value;
        _pinPerHit.Value++;
        _nextPinCost.Value *= 5;
        return true;
    }
}