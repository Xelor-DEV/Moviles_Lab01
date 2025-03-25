using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image[] colorButtons;
    [SerializeField] private Image[] spriteButtons;

    [Header("BrushSettings")]
    [SerializeField] private BrushSettings brushSettings;

    private void Start()
    {
        InitializeColorButtons();
        InitializeSpriteButtons();
    }

    private void InitializeColorButtons()
    {
        for (int i = 0; i < colorButtons.Length; ++i)
        {
            int index = i;
            Button btn = colorButtons[index].gameObject.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                brushSettings.CurrentColor = colorButtons[index].color;
                brushSettings.UpdateTrailColor();
            });
        }
    }

    private void InitializeSpriteButtons()
    {
        for (int i = 0; i < spriteButtons.Length; ++i)
        {
            int index = i;
            Button btn = spriteButtons[index].GetComponent<Button>();
            btn.onClick.AddListener(() => brushSettings.CurrentSprite = spriteButtons[index].sprite);
        }
    }
}