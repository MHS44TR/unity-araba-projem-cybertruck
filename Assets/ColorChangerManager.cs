using UnityEngine;
using UnityEngine.UI;

public class ColorChangerManager : MonoBehaviour
{
    public Material targetMaterial;
    public Button[] colorButtons;

    void Start()
    {
        for (int i = 0; i < colorButtons.Length; i++)
        {
            int index = i;
            colorButtons[i].onClick.AddListener(() => ChangeColor(index));
        }
    }
    void ChangeColor(int colorIndex)
{
    Color newColor = colorButtons[colorIndex].image.color;
    targetMaterial.SetColor("_Color", newColor);
}
}
