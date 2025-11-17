using UnityEngine;
using TMPro;

public class FPS : MonoBehaviour
{
    public float updateInterval = 0.5f;
    private float accum = 0f;
    private int frames = 0;
    private float timeLeft;

    private TextMeshProUGUI fpsText;

    private void Start()
    {

        fpsText = GetComponent<TextMeshProUGUI>();

        if (!fpsText)
        {
            Debug.LogError("TextMeshProUGUI component not found! Make sure it is attached to the same GameObject as this script.");
            enabled = false;
            return;
        }

        timeLeft = updateInterval;
    }

    private void Update()
    {
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        frames++;

        if (timeLeft <= 0f)
        {
            float fps = accum / frames;
            fpsText.text = $"FPS: {fps:F1}";

            timeLeft = updateInterval;
            accum = 0f;
            frames = 0;
        }
    }
}
