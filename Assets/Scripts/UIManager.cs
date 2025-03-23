using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image[] colorButtons;
    [SerializeField] private Image[] spriteButtons;

    [Header("References")]
    [SerializeField] private SpriteObjectsManager spriteObjectsManager;

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
            btn.onClick.AddListener(() => spriteObjectsManager.SetActiveColor(colorButtons[index].color));
        }
    }

    private void InitializeSpriteButtons()
    {
        for (int i = 0; i < spriteButtons.Length; ++i)
        {
            int index = i;
            Button btn = spriteButtons[index].GetComponent<Button>();
            btn.onClick.AddListener(() => spriteObjectsManager.SetActiveSprite(spriteButtons[index].sprite));
        }
    }
}