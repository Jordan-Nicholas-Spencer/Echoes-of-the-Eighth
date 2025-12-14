
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;


public class MainMenu : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainScene = "TheGlade";

    [Header("Panels")]
    [SerializeField] private GameObject instructionsPanel;

    [Header("Audio Setting")]
    [SerializeField] private AudioSource backgroundMusic;
    [SerializeField] private float fadeOutTime = 1.5f;

    [Header("Button Settings")]
    [SerializeField] private float buttonDeselectDelay = 0.3f;


    void Awake()
    {
        // Make sure time is running normaly if coming back from a Pause Game.
        Time.timeScale = 1f;

        // Lock cursor for using the menu.
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // Button hooks
    public void Play()
    {
        StartCoroutine(DeselectButton());
        StartCoroutine(FadeOutAndLoadNextScene());
    }

    // Fade out music before loading the main scene.
    private IEnumerator FadeOutAndLoadNextScene()
    {
        if (backgroundMusic != null)
        {
            float startVolume = backgroundMusic.volume;
            float elapsedTime = 0f;

            // Gradually reduce volume over fadeOutTime.
            while (elapsedTime < fadeOutTime)
            {
                elapsedTime += Time.deltaTime;
                backgroundMusic.volume = Mathf.Lerp(startVolume, 0f, elapsedTime / fadeOutTime);
                yield return null;
            }
        }

        // Load main scene
        SceneManager.LoadScene(mainScene);
    }

    public void OpenInstructions()
    {
        StartCoroutine(DeselectButton());

        if (instructionsPanel)
        {
            instructionsPanel.SetActive(true);
        }
    }

    public void CloseInstructions()
    {
        StartCoroutine(DeselectButton());

        if (instructionsPanel)
        {
            instructionsPanel.SetActive(false);
        }
    }

    public void OnApplicationQuit()
    {
        StartCoroutine(DeselectButton());
        Application.Quit();
    }

    // Prevents buttons from staying in the "selected" state after being clicked.
    private IEnumerator DeselectButton()
    {
        // Wait for the delay.
        yield return new WaitForSeconds(buttonDeselectDelay);

        EventSystem.current.SetSelectedGameObject(null);
    }
}
