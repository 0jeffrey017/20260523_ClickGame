using TMPro;
using UnityEngine;

public class MainGameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI clickCountText;

    public void SetClickCountUI(int clickCount)
    {
        clickCountText.text = $"click count: {clickCount}";
    }
}