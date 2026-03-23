using UnityEngine;
using UnityEngine.UI;

public class FailPopup : MonoBehaviour
{
    private GameObject popupRoot;

    public void Show(Transform canvasTransform)
    {
        if (popupRoot != null) return;

        // Dark overlay
        popupRoot = new GameObject("FailPopupRoot");
        popupRoot.transform.SetParent(canvasTransform, false);
        RectTransform overlayRect = popupRoot.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;
        Image overlayImage = popupRoot.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.6f);

        // Popup base
        GameObject popup = new GameObject("PopupBase");
        popup.transform.SetParent(popupRoot.transform, false);
        RectTransform popupRect = popup.AddComponent<RectTransform>();
        popupRect.anchoredPosition = Vector2.zero;
        popupRect.sizeDelta = new Vector2(700, 500);

        Image popupImage = popup.AddComponent<Image>();
        Sprite popupSprite = SpriteLoader.Get("Sprites/UI/popup_base");
        if (popupSprite != null)
        {
            popupImage.sprite = popupSprite;
            popupImage.type = Image.Type.Sliced;
        }
        else
        {
            popupImage.color = new Color(0.9f, 0.85f, 0.75f);
        }

        // Ribbon
        Sprite ribbonSprite = SpriteLoader.Get("Sprites/UI/popup_ribbon");
        if (ribbonSprite != null)
        {
            GameObject ribbon = new GameObject("Ribbon");
            ribbon.transform.SetParent(popup.transform, false);
            RectTransform ribbonRect = ribbon.AddComponent<RectTransform>();
            ribbonRect.anchorMin = new Vector2(0.5f, 1f);
            ribbonRect.anchorMax = new Vector2(0.5f, 1f);
            ribbonRect.pivot = new Vector2(0.5f, 0.5f);
            ribbonRect.anchoredPosition = new Vector2(0, -20);
            ribbonRect.sizeDelta = new Vector2(500, 120);
            Image ribbonImage = ribbon.AddComponent<Image>();
            ribbonImage.sprite = ribbonSprite;
            ribbonImage.raycastTarget = false;

            // "Level Failed" text on ribbon
            GameObject ribbonTextObj = new GameObject("RibbonText");
            ribbonTextObj.transform.SetParent(ribbon.transform, false);
            RectTransform rtRect = ribbonTextObj.AddComponent<RectTransform>();
            rtRect.anchorMin = Vector2.zero;
            rtRect.anchorMax = Vector2.one;
            rtRect.sizeDelta = Vector2.zero;
            Text ribbonText = ribbonTextObj.AddComponent<Text>();
            ribbonText.text = "Level Failed!";
            ribbonText.alignment = TextAnchor.MiddleCenter;
            ribbonText.fontSize = 42;
            ribbonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (ribbonText.font == null)
                ribbonText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            ribbonText.color = Color.white;
            ribbonText.fontStyle = FontStyle.Bold;
            ribbonText.raycastTarget = false;
        }

        // Close button (return to main menu)
        CreatePopupButton(popup.transform, "CloseButton", "Close",
            new Vector2(-130, -120), new Vector2(200, 80),
            SpriteLoader.Get("Sprites/UI/close_button"),
            () => { GameManager.Instance.LoadMainMenu(); });

        // Try Again button
        CreatePopupButton(popup.transform, "TryAgainButton", "Try Again",
            new Vector2(130, -120), new Vector2(200, 80),
            null,
            () => { GameManager.Instance.ReloadLevel(); });
    }

    private void CreatePopupButton(Transform parent, string name, string label,
        Vector2 position, Vector2 size, Sprite sprite, System.Action onClick)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;

        Image image = buttonObj.AddComponent<Image>();
        if (sprite != null)
        {
            image.sprite = sprite;
            image.type = Image.Type.Sliced;
        }
        else
        {
            image.color = new Color(0.3f, 0.7f, 1f);
        }

        Button button = buttonObj.AddComponent<Button>();
        button.onClick.AddListener(() => onClick());

        // Text
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;

        Text text = textObj.AddComponent<Text>();
        text.text = label;
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 30;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;
        text.raycastTarget = false;
    }
}
