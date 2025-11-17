using UnityEngine;
using UnityEngine.UI;

public class GraphicsSettingsManager : MonoBehaviour
{
    private string[] qualityLevels = { "Very Low", "Low", "Medium", "High", "Very High", "Ultra" };

    public Button[] qualityButtons;

    private void Start()
    {
        InitializeButtons();
    }

    private void InitializeButtons()
    {
        for (int i = 0; i < qualityButtons.Length; i++)
        {
            int index = i;
            qualityButtons[i].onClick.AddListener(() => SetQualityLevel(index));
        }
        SetQualityLevel(QualitySettings.GetQualityLevel());
    }

    public void SetQualityLevel(int level)
    {
        QualitySettings.SetQualityLevel(level);
        UpdateButtonColors(level);
    }

    private void UpdateButtonColors(int currentLevel)
    {
        for (int i = 0; i < qualityButtons.Length; i++)
        {
            ColorBlock colors = qualityButtons[i].colors;
            colors.normalColor = (i == currentLevel) ? Color.green : Color.white;
            qualityButtons[i].colors = colors;
        }
    }
}
