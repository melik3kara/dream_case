using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public const int TotalLevels = 10;
    private const string LevelKey = "CurrentLevel";

    public int CurrentLevel
    {
        get => PlayerPrefs.GetInt(LevelKey, 1);
        set
        {
            PlayerPrefs.SetInt(LevelKey, value);
            PlayerPrefs.Save();
        }
    }

    public bool AllLevelsFinished => CurrentLevel > TotalLevels;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void LoadLevel()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void ReloadLevel()
    {
        SceneManager.LoadScene("LevelScene");
    }

    public void OnLevelWon()
    {
        if (CurrentLevel <= TotalLevels)
            CurrentLevel++;
    }

    public void SetLevel(int level)
    {
        CurrentLevel = Mathf.Clamp(level, 1, TotalLevels + 1);
    }
}
