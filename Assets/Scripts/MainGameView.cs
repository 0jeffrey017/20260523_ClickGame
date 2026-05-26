using TMPro;
using UnityEngine;

public class MainGameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    public void SetMoneyText(ulong moneyCount)
    {
        moneyText.text = $"money: {moneyCount}";
    }
}