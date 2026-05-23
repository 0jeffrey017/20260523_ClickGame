using R3;
using UnityEngine;

public class MainGameModel
{
        private ReactiveProperty<int> _clickCount = new ReactiveProperty<int>(0);
        public ReadOnlyReactiveProperty<int> ClickCount => _clickCount;

        public void AddClick(int clickCount)
        {     
                Debug.Log(clickCount);
                _clickCount.Value += clickCount;
        }
}