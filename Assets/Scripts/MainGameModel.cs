using R3;
using UnityEngine;

public class MainGameModel
{
        private ReactiveProperty<int> _money = new ReactiveProperty<int>(0);
        public ReadOnlyReactiveProperty<int> Money => _money;
        public void AddMoney(int money)
        {
                _money.Value += money;
        }
}