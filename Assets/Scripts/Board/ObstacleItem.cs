using UnityEngine;

public class ObstacleItem : ItemBase
{
    public ObstacleType Type { get; private set; }
    public int Health { get; private set; }
    public bool IsDestroyed => Health <= 0;

    public override bool CanFall => Type == ObstacleType.Vase;
    public override bool IsObstacle => true;

    public void InitObstacle(int x, int y, ObstacleType type)
    {
        Init(x, y);
        Type = type;
        Health = (type == ObstacleType.Vase) ? 2 : 1;
        UpdateSprite();
    }

    public bool TakeDamage()
    {
        Health--;
        if (Health <= 0) return true;
        UpdateSprite();
        return false;
    }

    private void UpdateSprite()
    {
        if (SpriteRenderer == null) return;
        SpriteRenderer.sprite = SpriteLoader.GetObstacle(Type, Health);
    }

    public bool CanBeDamagedByBlast()
    {
        return Type == ObstacleType.Box || Type == ObstacleType.Vase;
    }

    public bool CanBeDamagedByRocket()
    {
        return true;
    }
}
