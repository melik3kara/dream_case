using UnityEngine;
using System.Collections.Generic;

public static class SpriteLoader
{
    private static Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Get(string path)
    {
        if (cache.TryGetValue(path, out Sprite cached))
            return cached;

        Sprite sprite = Resources.Load<Sprite>(path);
        if (sprite != null)
            cache[path] = sprite;
        else
            Debug.LogWarning("Sprite not found: " + path);
        return sprite;
    }

    public static Sprite GetCube(CubeColor color)
    {
        return Get("Sprites/Cubes/" + ColorToName(color));
    }

    public static Sprite GetCubeRocketState(CubeColor color)
    {
        return Get("Sprites/Cubes/" + ColorToName(color) + "_rocket");
    }

    public static Sprite GetObstacle(ObstacleType type, int health)
    {
        switch (type)
        {
            case ObstacleType.Box: return Get("Sprites/Obstacles/box");
            case ObstacleType.Stone: return Get("Sprites/Obstacles/stone");
            case ObstacleType.Vase:
                return health >= 2 ? Get("Sprites/Obstacles/vase_01") : Get("Sprites/Obstacles/vase_02");
            default: return null;
        }
    }

    public static Sprite GetRocket(RocketDirection direction)
    {
        return direction == RocketDirection.Horizontal
            ? Get("Sprites/Rockets/horizontal_rocket")
            : Get("Sprites/Rockets/vertical_rocket");
    }

    public static Sprite GetRocketPart(RocketDirection direction, bool isFirst)
    {
        if (direction == RocketDirection.Horizontal)
            return isFirst ? Get("Sprites/Rockets/horizontal_rocket_part_left") : Get("Sprites/Rockets/horizontal_rocket_part_right");
        else
            return isFirst ? Get("Sprites/Rockets/vertical_rocket_part_bottom") : Get("Sprites/Rockets/vertical_rocket_part_top");
    }

    private static string ColorToName(CubeColor color)
    {
        switch (color)
        {
            case CubeColor.Red: return "red";
            case CubeColor.Green: return "green";
            case CubeColor.Blue: return "blue";
            case CubeColor.Yellow: return "yellow";
            default: return "red";
        }
    }
}

public enum CubeColor { Red, Green, Blue, Yellow }
public enum ObstacleType { Box, Stone, Vase }
public enum RocketDirection { Horizontal, Vertical }
