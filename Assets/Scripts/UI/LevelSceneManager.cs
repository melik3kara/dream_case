using UnityEngine;
using UnityEngine.UI;

public class LevelSceneManager : MonoBehaviour
{
    private Board board;
    private Text movesText;
    private Text goalsText;
    private FailPopup failPopup;
    private CelebrationManager celebration;
    private Canvas uiCanvas;

    void Start()
    {
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        int levelNum = GameManager.Instance.CurrentLevel;
        LevelData data = LevelData.Load(levelNum);

        if (data == null)
        {
            Debug.LogError("Failed to load level " + levelNum);
            GameManager.Instance.LoadMainMenu();
            return;
        }

        SetupCamera(data);
        CreateBoard(data);
        CreateUI(data);
    }

    private void SetupCamera(LevelData data)
    {
        Camera cam = Camera.main;
        cam.orthographic = true;

        float verticalSize = data.grid_height / 2f + 2.5f;
        float horizontalSize = (data.grid_width / 2f + 1f) / cam.aspect;
        cam.orthographicSize = Mathf.Max(verticalSize, horizontalSize);

        cam.transform.position = new Vector3(0, 0.5f, -10);
        cam.backgroundColor = new Color(0.15f, 0.15f, 0.25f);
    }

    private void CreateBoard(LevelData data)
    {
        GameObject boardObj = new GameObject("Board");
        board = boardObj.AddComponent<Board>();
        board.Initialize(data);

        board.OnMovesChanged += OnMovesChanged;
        board.OnWin += OnWin;
        board.OnLose += OnLose;
    }

    private void CreateUI(LevelData data)
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("UICanvas");
        uiCanvas = canvasObj.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        uiCanvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Top bar background
        Sprite topSprite = SpriteLoader.Get("Sprites/UI/top_ui");
        if (topSprite != null)
        {
            GameObject topBar = new GameObject("TopBar");
            topBar.transform.SetParent(canvasObj.transform, false);
            RectTransform topRect = topBar.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.pivot = new Vector2(0.5f, 1);
            topRect.anchoredPosition = Vector2.zero;
            topRect.sizeDelta = new Vector2(0, 200);
            Image topImage = topBar.AddComponent<Image>();
            topImage.sprite = topSprite;
            topImage.type = Image.Type.Sliced;
            topImage.raycastTarget = false;
        }

        // Moves counter
        GameObject movesObj = CreateUIText(canvasObj.transform, "MovesText",
            new Vector2(-200, -50), new Vector2(300, 100));
        movesText = movesObj.GetComponent<Text>();
        movesText.text = "Moves: " + data.move_count;
        movesText.fontSize = 48;
        movesText.color = Color.white;
        movesText.fontStyle = FontStyle.Bold;
        movesText.alignment = TextAnchor.MiddleCenter;
        RectTransform movesRect = movesObj.GetComponent<RectTransform>();
        movesRect.anchorMin = new Vector2(0.5f, 1f);
        movesRect.anchorMax = new Vector2(0.5f, 1f);
        movesRect.pivot = new Vector2(0.5f, 1f);

        // Goals counter
        GameObject goalsObj = CreateUIText(canvasObj.transform, "GoalsText",
            new Vector2(200, -50), new Vector2(300, 100));
        goalsText = goalsObj.GetComponent<Text>();
        int obstacleCount = board.GetRemainingObstacles();
        goalsText.text = "Goals: " + obstacleCount;
        goalsText.fontSize = 48;
        goalsText.color = Color.white;
        goalsText.fontStyle = FontStyle.Bold;
        goalsText.alignment = TextAnchor.MiddleCenter;
        RectTransform goalsRect = goalsObj.GetComponent<RectTransform>();
        goalsRect.anchorMin = new Vector2(0.5f, 1f);
        goalsRect.anchorMax = new Vector2(0.5f, 1f);
        goalsRect.pivot = new Vector2(0.5f, 1f);

        // Level title
        GameObject titleObj = CreateUIText(canvasObj.transform, "LevelTitle",
            new Vector2(0, -10), new Vector2(400, 60));
        Text titleText = titleObj.GetComponent<Text>();
        titleText.text = "Level " + data.level_number;
        titleText.fontSize = 36;
        titleText.color = Color.white;
        titleText.fontStyle = FontStyle.Bold;
        titleText.alignment = TextAnchor.MiddleCenter;
        RectTransform titleRect = titleObj.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
    }

    private GameObject CreateUIText(Transform parent, string name, Vector2 position, Vector2 size)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rect = obj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Text text = obj.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.raycastTarget = false;

        return obj;
    }

    private void OnMovesChanged(int moves)
    {
        if (movesText != null)
            movesText.text = "Moves: " + moves;

        if (goalsText != null)
            goalsText.text = "Goals: " + board.GetRemainingObstacles();
    }

    private void OnWin()
    {
        GameManager.Instance.OnLevelWon();

        // Show celebration
        GameObject celebObj = new GameObject("Celebration");
        celebration = celebObj.AddComponent<CelebrationManager>();
        celebration.Play(() =>
        {
            GameManager.Instance.LoadMainMenu();
        });
    }

    private void OnLose()
    {
        // Show fail popup
        if (failPopup == null)
        {
            GameObject popupObj = new GameObject("FailPopup");
            failPopup = popupObj.AddComponent<FailPopup>();
        }
        failPopup.Show(uiCanvas.transform);
    }
}
