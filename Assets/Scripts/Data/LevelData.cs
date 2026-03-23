using UnityEngine;
using System;

[Serializable]
public class LevelData
{
    public int level_number;
    public int grid_width;
    public int grid_height;
    public int move_count;
    public string[] grid;

    public static LevelData Load(int levelNumber)
    {
        string path = string.Format("Levels/level_{0:D2}", levelNumber);
        TextAsset json = Resources.Load<TextAsset>(path);
        if (json == null) return null;
        return JsonUtility.FromJson<LevelData>(json.text);
    }

    public string GetCell(int x, int y)
    {
        int row = grid_height - 1 - y;
        int index = row * grid_width + x;
        if (index < 0 || index >= grid.Length) return "rand";
        return grid[index];
    }
}
