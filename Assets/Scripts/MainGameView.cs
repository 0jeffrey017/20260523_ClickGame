using System;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainGameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    [SerializeField] private TextMeshProUGUI ballCostText;
    [SerializeField] private TextMeshProUGUI ballCountText;
    [SerializeField] private TextMeshProUGUI pinCostText;
    [SerializeField] private TextMeshProUGUI pinCountText;
    [SerializeField] private GameObject noMoneyText;
    [SerializeField] private Button ballButton;
    [SerializeField] private Button pinButton;
    
    public Action OnBallButtonPressed = delegate { };
    public Action OnPinButtonPressed = delegate { };

    private void OnEnable()
    {
        ballButton.onClick.AddListener(() => OnBallButtonPressed.Invoke());
        pinButton.onClick.AddListener(() => OnPinButtonPressed.Invoke());
    }

    private void OnDisable()
    {
        ballButton.onClick.RemoveAllListeners();
        pinButton.onClick.RemoveAllListeners();
    }

    public async UniTaskVoid ShowNoMoneyText()
    {
        noMoneyText.SetActive(true);
        await UniTask.Delay(TimeSpan.FromSeconds(1));
        noMoneyText.SetActive(false);
    }
    
    public void SetBallText(ulong cost,uint ballCount)
    {
        ballCostText.text = $"Cost : {cost}";
        ballCountText.text = $"Ball Count : {ballCount}";
    }
    
    public void SetPinText(ulong cost, uint pinCount)
    {
        pinCostText.text = $"Cost : {cost}";
        pinCountText.text = $"Per Pin Money: {pinCount}";
    }

    public void SetMoneyText(ulong moneyCount)
    {
        moneyText.text = $"money: {moneyCount}";
    }
}