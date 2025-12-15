using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] public GameObject pausePanel;

    private bool isPaused = false;

    // Reference player's movement script.
    private MonoBehaviour humanGameplayController;

    // Reference Character Controller.
    private CharacterController characterController;

    void Awake()
    {
        // Ensures we never enter a gameplay scene paused
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Find the player once
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player)
        {
            // Get the Human Gameplay Controller.
            humanGameplayController = player.GetComponent("Human Gameplay Controller") as MonoBehaviour;

            // Get the character controller component.
            characterController = player.GetComponent<CharacterController>();
        }

        if (pausePanel)
        {
            pausePanel.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }                
        }
    }

    public void Pause()
    {
        if (isPaused)
        {
            return;
        }
        isPaused = true;

        // Freeze time-driven gameplay
        Time.timeScale = 0f;

        // Pause all audio
        AudioListener.pause = true;

        // Stop player from changing any state while paused.
        if (humanGameplayController)
        {
            humanGameplayController.enabled = false;
        }
        
        // Show UI and free cursor
        if (pausePanel)
        {
            pausePanel.SetActive(true);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Resume()
    {
        if (!isPaused)
        {
            return;
        }
        isPaused = false;

        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (humanGameplayController)
        {
            humanGameplayController.enabled = true;
        }
        if (pausePanel)
        {
            pausePanel.SetActive(false);
            // Hide and lock the cursor again.
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        // Clears the selection on the button used.
        if (EventSystem.current != null)
        {
            StartCoroutine(ClearSelection());
        }
    }

    public void MainMenu()
    {
        // Always restore time/audio before leaving the scene
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene("Main_Menu");
    }

    public void OnApplicationQuit()
    {
        Application.Quit();
    }

    // Helper function to clear the highlight from the prior used button in the pause menu.
    private IEnumerator ClearSelection()
    {
        yield return null;
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
}
