using UnityEngine;

public class RocketItem : ItemBase
{
    public RocketDirection Direction { get; private set; }

    public override bool CanFall => true;
    public override bool IsObstacle => false;

    public void InitRocket(int x, int y, RocketDirection direction)
    {
        Init(x, y);
        Direction = direction;
        UpdateSprite();
    }

    private void UpdateSprite()
    {
        if (SpriteRenderer == null) return;
        SpriteRenderer.sprite = SpriteLoader.GetRocket(Direction);
    }
}
