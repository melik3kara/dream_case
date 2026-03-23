using UnityEngine;

public abstract class ItemBase : MonoBehaviour
{
    public int X { get; set; }
    public int Y { get; set; }
    public SpriteRenderer SpriteRenderer { get; private set; }

    public abstract bool CanFall { get; }
    public abstract bool IsObstacle { get; }

    public virtual void Init(int x, int y)
    {
        X = x;
        Y = y;
        SpriteRenderer = GetComponent<SpriteRenderer>();
        if (SpriteRenderer == null)
            SpriteRenderer = gameObject.AddComponent<SpriteRenderer>();
    }

    public void SetSortingOrder(int order)
    {
        if (SpriteRenderer != null)
            SpriteRenderer.sortingOrder = order;
    }

    public void FitToCell(float cellSize)
    {
        if (SpriteRenderer == null || SpriteRenderer.sprite == null) return;
        Vector2 spriteSize = SpriteRenderer.sprite.bounds.size;
        float scaleX = cellSize / spriteSize.x;
        float scaleY = cellSize / spriteSize.y;
        float scale = Mathf.Min(scaleX, scaleY) * 0.9f;
        transform.localScale = new Vector3(scale, scale, 1f);
    }
}
