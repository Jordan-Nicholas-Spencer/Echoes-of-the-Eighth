using System;
using System.Collections;
using System.Collections.Generic;
using Unity_Store_Imports.Ilumisoft.Health_System.Scripts;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private Button respawnButton;
    
    [Header("Player Health")]
    [SerializeField] private GameObject playerHealthComponent;
    private Health playerHealthScript;
    
    [Header("Audio")]
    [SerializeField] private AudioListener playerAudioListener;
    
    //Add a listener to the respawn button to restart the game when game is over
    private void Start()
    {
        playerHealthScript = playerHealthComponent.GetComponent<Health>();
        respawnButton.onClick.AddListener(RestartGame);
    }
    
    void Update()
    {
        //Check the playerHealth.IsAlive bool to see if the game is still going
        if (!playerHealthScript.IsAlive)
        {
            gameOverScreen.SetActive(true);
            
            //Unlock cursor for button click
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerAudioListener.enabled = false; //Mute audio
        }
    }

    //Reload TheGlade scene
    void RestartGame()
    {
        SceneManager.LoadScene(1);
    }
}
