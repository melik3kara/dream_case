using UnityEngine;
using UnityEditor;

public class LevelEditorMenu : EditorWindow
{
    private int levelNumber = 1;

    [MenuItem("Dream Games/Set Level Number")]
    public static void ShowWindow()
    {
        GetWindow<LevelEditorMenu>("Set Level Number");
    }

    void OnGUI()
    {
        GUILayout.Label("Set Current Level", EditorStyles.boldLabel);
        GUILayout.Space(10);

        int currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
        GUILayout.Label("Current saved level: " + currentLevel);
        GUILayout.Space(10);

        levelNumber = EditorGUILayout.IntField("Level Number:", levelNumber);
        levelNumber = Mathf.Clamp(levelNumber, 1, GameManager.TotalLevels + 1);

        GUILayout.Space(10);

        if (GUILayout.Button("Set Level"))
        {
            PlayerPrefs.SetInt("CurrentLevel", levelNumber);
            PlayerPrefs.Save();
            Debug.Log("Level set to: " + levelNumber);
        }

        GUILayout.Space(5);

        if (GUILayout.Button("Reset to Level 1"))
        {
            PlayerPrefs.SetInt("CurrentLevel", 1);
            PlayerPrefs.Save();
            levelNumber = 1;
            Debug.Log("Level reset to 1");
        }
    }
}
