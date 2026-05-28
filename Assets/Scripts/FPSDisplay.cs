using TMPro;
using UnityEngine;

public class FPSDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField] private float pollingTime = 0.5f; // How often the UI updates (in seconds)

    private float timeAccumulator;
    private int frameCount;

    void Update()
    {
        // Track the total elapsed time and frames passed
        timeAccumulator += Time.unscaledDeltaTime;
        frameCount++;

        // Update the display when the interval is reached
        if (timeAccumulator >= pollingTime)
        {
            int frameRate = Mathf.RoundToInt(frameCount / timeAccumulator);
            fpsText.text = $"{frameRate} FPS";

            // Reset trackers for the next polling block
            timeAccumulator = 0f;
            frameCount = 0;
        }
    }
}
