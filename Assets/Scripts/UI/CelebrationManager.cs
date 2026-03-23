using UnityEngine;
using System;
using System.Collections;

public class CelebrationManager : MonoBehaviour
{
    private Action onComplete;

    public void Play(Action onCompleteCallback)
    {
        onComplete = onCompleteCallback;
        StartCoroutine(PlayCelebration());
    }

    private IEnumerator PlayCelebration()
    {
        // Create multiple particle bursts
        for (int i = 0; i < 5; i++)
        {
            SpawnCelebrationParticles(
                new Vector3(
                    UnityEngine.Random.Range(-3f, 3f),
                    UnityEngine.Random.Range(-2f, 3f),
                    0f
                )
            );
            yield return new WaitForSeconds(0.2f);
        }

        // Show "Level Complete!" text via world-space canvas
        GameObject textCanvas = new GameObject("CelebrationCanvas");
        Canvas canvas = textCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 200;
        textCanvas.AddComponent<UnityEngine.UI.CanvasScaler>().uiScaleMode =
            UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        textCanvas.GetComponent<UnityEngine.UI.CanvasScaler>().referenceResolution =
            new Vector2(1080, 1920);

        GameObject textObj = new GameObject("CelebrationText");
        textObj.transform.SetParent(textCanvas.transform, false);
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.sizeDelta = new Vector2(800, 200);

        UnityEngine.UI.Text text = textObj.AddComponent<UnityEngine.UI.Text>();
        text.text = "Level Complete!";
        text.alignment = TextAnchor.MiddleCenter;
        text.fontSize = 64;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.color = Color.white;
        text.fontStyle = FontStyle.Bold;

        // Animate text scale
        float elapsed = 0f;
        float duration = 0.5f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Sin(t * Mathf.PI * 0.5f) * 1.2f;
            textObj.transform.localScale = Vector3.one * Mathf.Max(scale, 0.01f);
            yield return null;
        }
        textObj.transform.localScale = Vector3.one;

        // Spawn star particles
        SpawnStars();

        // Wait then transition
        yield return new WaitForSeconds(2f);

        onComplete?.Invoke();
    }

    private void SpawnCelebrationParticles(Vector3 position)
    {
        GameObject go = new GameObject("CelebrationParticle");
        go.transform.position = position;
        ParticleSystem ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime = 1f;
        main.startSpeed = 5f;
        main.startSize = 0.2f;
        main.duration = 0.5f;
        main.loop = false;
        main.maxParticles = 30;
        main.startColor = new ParticleSystem.MinMaxGradient(
            new Color(1f, 0.8f, 0f),
            new Color(1f, 0.2f, 0.5f)
        );
        main.gravityModifier = 0.5f;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 30)
        });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        var renderer = go.GetComponent<ParticleSystemRenderer>();
        renderer.sortingOrder = 150;

        Destroy(go, 2f);
    }

    private void SpawnStars()
    {
        for (int i = 0; i < 3; i++)
        {
            GameObject starGo = new GameObject("Star_" + i);
            SpriteRenderer sr = starGo.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteLoader.Get("Sprites/UI/star");
            sr.sortingOrder = 160;

            float xPos = (i - 1) * 2f;
            starGo.transform.position = new Vector3(xPos, 1f, 0f);

            if (sr.sprite != null)
            {
                float scale = 1.5f / sr.sprite.bounds.size.x;
                starGo.transform.localScale = new Vector3(scale, scale, 1f);
            }

            Destroy(starGo, 3f);
        }
    }
}
