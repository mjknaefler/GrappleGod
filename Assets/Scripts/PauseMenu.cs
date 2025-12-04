using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenu : MonoBehaviour
{
    [Header("Menu Settings")]
    [SerializeField] private string mainMenuSceneName = "Level Select";

    [Header("UI References")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private Image fadeImage;
    [SerializeField] private Button quitButton;

    [Header("Fade Settings")]
    [SerializeField] private float menuFadeDuration = 0.3f;
    [SerializeField] private float quitFadeDuration = 1f;

    private bool isPaused = false;
    private bool isQuitting = false;

    void Start()
    {
        // Make sure menu starts hidden
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Make fade image transparent
        if (fadeImage != null)
            fadeImage.color = new Color(0, 0, 0, 0);

        // Setup quit button
        if (quitButton != null)
            quitButton.onClick.AddListener(QuitToMainMenu);
    }

    void Update()
    {
        // Toggle pause with Escape key
        if (Input.GetKeyDown(KeyCode.Escape) && !isQuitting)
        {
            if (isPaused)
                Resume();
            else
                Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        StartCoroutine(FadeInMenu());
    }

    public void Resume()
    {
        isPaused = false;
        StartCoroutine(FadeOutMenu());
    }

    public void QuitToMainMenu()
    {
        if (!isQuitting)
        {
            isQuitting = true;
            StartCoroutine(FadeAndQuit());
        }
    }

    IEnumerator FadeInMenu()
    {
        // Show menu
        pauseMenuUI.SetActive(true);

        // Fade in dark overlay
        float elapsed = 0f;
        while (elapsed < menuFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled time so fade works while paused
            float alpha = Mathf.Lerp(0f, 0.7f, elapsed / menuFadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0.7f);

        // Pause the game
        Time.timeScale = 0f;
    }

    IEnumerator FadeOutMenu()
    {
        // Unpause the game first
        Time.timeScale = 1f;

        // Fade out dark overlay
        float elapsed = 0f;
        while (elapsed < menuFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0.7f, 0f, elapsed / menuFadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 0);

        // Hide menu
        pauseMenuUI.SetActive(false);
    }

    IEnumerator FadeAndQuit()
    {
        // Unpause before transitioning
        Time.timeScale = 1f;

        // Fade to full black
        float elapsed = 0f;
        while (elapsed < quitFadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0.7f, 1f, elapsed / quitFadeDuration);
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        fadeImage.color = new Color(0, 0, 0, 1);

        // Load main menu
        SceneManager.LoadScene(mainMenuSceneName);
    }
}