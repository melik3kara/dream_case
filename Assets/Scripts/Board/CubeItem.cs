using UnityEngine;

public class CubeItem : ItemBase
{
    public CubeColor Color { get; private set; }
    private bool showRocketIndicator;

    public override bool CanFall => true;
    public override bool IsObstacle => false;

    public void InitCube(int x, int y, CubeColor color)
    {
        Init(x, y);
        Color = color;
        UpdateSprite();
    }

    public void SetRocketIndicator(bool show)
    {
        if (showRocketIndicator == show) return;
        showRocketIndicator = show;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (SpriteRenderer == null) return;
        SpriteRenderer.sprite = showRocketIndicator
            ? SpriteLoader.GetCubeRocketState(Color)
            : SpriteLoader.GetCube(Color);
    }

    public static CubeColor RandomColor()
    {
        return (CubeColor)Random.Range(0, 4);
    }

    public static CubeColor StringToColor(string s)
    {
        switch (s)
        {
            case "r": return CubeColor.Red;
            case "g": return CubeColor.Green;
            case "b": return CubeColor.Blue;
            case "y": return CubeColor.Yellow;
            default: return RandomColor();
        }
    }
}
