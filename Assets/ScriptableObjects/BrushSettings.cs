using UnityEngine;

[CreateAssetMenu(fileName = "BrushSettings", menuName = "ScriptableObjects/BrushSettings", order = 1)]
public class BrushSettings : ScriptableObject
{
    [SerializeField] private Sprite currentSprite;
    [SerializeField] private Color currentColor;
    [SerializeField] private TrailRenderer swipeTrail;

    public Sprite CurrentSprite
    {
        get
        {
            return currentSprite;
        }
        set
        {
            currentSprite = value;
        }
    }

    public Color CurrentColor
    {
        get
        {
            return currentColor;
        }
        set
        {
            currentColor = value;
        }
    }

    public TrailRenderer SwipeTrail
    {
        get
        {
            return swipeTrail;
        }
        set
        {
            swipeTrail = value;
        }
    }

    public void UpdateTrailColor()
    {
        if (SwipeTrail != null)
        {
            SwipeTrail.startColor = CurrentColor;
            SwipeTrail.endColor = new Color(CurrentColor.r, CurrentColor.g, CurrentColor.b, 0);
        }
    }
}