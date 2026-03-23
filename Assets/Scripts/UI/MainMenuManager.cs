using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    void Start()
    {
        // Ensure GameManager exists
        if (GameManager.Instance == null)
        {
            GameObject gmObj = new GameObject("GameManager");
            gmObj.AddComponent<GameManager>();
        }

        SetupCamera();
        CreateBackground();
        CreateLevelButton();
    }

    private void SetupCamera()
    {
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 6f;
        Camera.main.backgroundColor = new Color(0.2f, 0.6f, 0.3f);
        Camera.main.transform.position = new Vector3(0, 0, -10);
    }

    private void CreateBackground()
    {
        Sprite bgSprite = SpriteLoader.Get("Sprites/Menu/background");
        if (bgSprite == null) return;

        GameObject bg = new GameObject("Background");
        SpriteRenderer sr = bg.AddComponent<SpriteRenderer>();
        sr.sprite = bgSprite;
        sr.sortingOrder = -100;

        // Scale to fill screen
        float screenHeight = Camera.main.orthographicSize * 2f;
        float screenWidth = screenHeight * Camera.main.aspect;
        Vector2 spriteSize = sr.sprite.bounds.size;
        float scaleX = screenWidth / spriteSize.x;
        float scaleY = screenHeight / spriteSize.y;
        float scale = Mathf.Max(scaleX, scaleY);
        bg.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void CreateLevelButton()
    {
        // Create Canvas
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Button background
        GameObject buttonObj = new GameObject("LevelButton");
        buttonObj.transform.SetParent(canvasObj.transform, false);
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchoredPosition = new Vector2(0, -100);
        buttonRect.sizeDelta = new Vector2(400, 150);

        Image buttonImage = buttonObj.AddComponent<Image>();
        Sprite buttonSprite = SpriteLoader.Get("Sprites/UI/button");
        if (buttonSprite != null)
        {
            buttonImage.sprite = buttonSprite;
            buttonImage.type = Image.Type.Sliced;
        }
        else
        {
            buttonImage.color = new Color(0.3f, 0.7f, 1f);
        }

        Button button = buttonObj.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
        colors.pressedColor = new Color(0.7f, 0.7f, 0.7f);
        button.colors = colors;

        // Button frame overlay
        Sprite frameSprite = SpriteLoader.Get("Sprites/UI/button_frame");
        if (frameSprite != null)
        {
            GameObject frameObj = new GameObject("ButtonFrame");
            frameObj.transform.SetParent(buttonObj.transform, false);
            RectTransform frameRect = frameObj.AddComponent<RectTransform>();
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.sizeDelta = new Vector2(20, 20);
            frameRect.anchoredPosition = Vector2.zero;
            Image frameImage = frameObj.AddComponent<Image>();
            frameImage.sprite = frameSprite;
            frameImage.type = Image.Type.Sliced;
            frameImage.raycastTarget = false;
        }

        // Button text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.anchoredPosition = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 48;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;

        if (GameManager.Instance.AllLevelsFinished)
        {
            text.text = "Finished!";
            button.onClick.AddListener(() => { });
        }
        else
        {
            text.text = "Level " + GameManager.Instance.CurrentLevel;
            button.onClick.AddListener(() =>
            {
                GameManager.Instance.LoadLevel();
            });
        }
    }
}
