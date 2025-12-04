using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFadeIn : MonoBehaviour
{
    [SerializeField] private float fadeInDuration = 1f;

    void Start()
    {
        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        // Find the fade canvas created by LevelTransition
        GameObject fadeCanvasObj = GameObject.Find("FadeCanvas");
        if (fadeCanvasObj == null)
        {
            Debug.Log("No fade canvas found - scene loaded normally");
            yield break;
        }

        Image fadeImage = fadeCanvasObj.GetComponentInChildren<Image>();
        if (fadeImage == null) yield break;

        // Small delay at black screen
        yield return new WaitForSeconds(0.2f);

        // Fade from black to transparent
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeInDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Ensure fully transparent
        fadeImage.color = new Color(0, 0, 0, 0);

        // Destroy the fade canvas
        Destroy(fadeCanvasObj);
    }
}