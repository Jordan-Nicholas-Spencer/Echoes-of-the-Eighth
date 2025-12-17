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
    [SerializeField] private AudioSource[] monsterSources;

    [SerializeField] private AudioSource playerAudio;

    [SerializeField] private AudioClip playerDeath;

    private bool ranOnce = false;
    //Add a listener to the respawn button to restart the game when game is over
    private void Start()
    {
        playerHealthScript = playerHealthComponent.GetComponent<Health>();
        respawnButton.onClick.AddListener(RestartGame);
    }
    
    void Update()
    {
        //Check the playerHealth.IsAlive bool to see if the game is still going
        if (!playerHealthScript.IsAlive && !ranOnce)
        {
            gameOverScreen.SetActive(true);
            ranOnce = true;
            //Unlock cursor for button click
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            StartCoroutine(MuteMonsterAudio()); //Mute all monsters and play death sound
        }
    }

    //Reload TheGlade scene
    void RestartGame()
    {
        SceneManager.LoadScene(1);
    }

    IEnumerator MuteMonsterAudio()
    {
        foreach (var source in monsterSources)
        {
            source.enabled = false;
        }

        yield return new WaitForSeconds(1f);
        
        playerAudio.PlayOneShot(playerDeath, 0.5f);
    }
}
