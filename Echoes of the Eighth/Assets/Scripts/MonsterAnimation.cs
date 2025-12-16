using System;
using System.Collections;
using System.Collections.Generic;
using Unity_Store_Imports.Ilumisoft.Health_System.Scripts;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterAnimation : MonoBehaviour
{
    public Animator animator;
    private MonsterNavigation _monsterNavigationScript;
    
    //Monster NavMesh Agent
    private NavMeshAgent monsterAgent;
    
    //Cooldown Vars
    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 5f;

    //Audio
    private AudioSource monsterAudio;
    [SerializeField] private AudioClip punch;
    [SerializeField] private AudioClip slam;
    [SerializeField] private AudioClip doublePunch;
    [SerializeField] private AudioClip roar;
    [SerializeField] private AudioClip death;
    private bool deathAudioPlayedOnce = false;

    //Health Components
    private GameObject playerHealthGO;
    private Health playerHealthComponent;
    private Health monsterHealthComponent;
    [SerializeField] private GameObject monsterHealthBar;
    
    //Assignments when script is loaded (monster is spawned)
    private void Awake()
    {
        _monsterNavigationScript = GetComponent<MonsterNavigation>();
        monsterAgent = _monsterNavigationScript.monsterAgent;
        monsterAudio = GetComponent<AudioSource>();
        playerHealthGO = GameObject.FindGameObjectWithTag("Player Health bar");
        playerHealthComponent = playerHealthGO.GetComponent<Health>();
        monsterHealthComponent = gameObject.GetComponent<Health>();
    }
    
    void Update()
    {
        //Speed parameter used for running animation
        animator.SetFloat("Speed", monsterAgent.velocity.magnitude);
        
        //"Attack" the player when the monster is close enough and there isn't a cooldown active and while alive
        if (_monsterNavigationScript.distanceToPlayer < 5 && canAttack && monsterHealthComponent.IsAlive)
        {
            PlayAttackAnimation();
            canAttack = false;
            StartCoroutine(StartCooldown());
        }
        
        //Monster death
        if (!monsterHealthComponent.IsAlive)
        {
            animator.SetBool("Death", true);
            monsterHealthBar.SetActive(false);
            PlayDeathSoundOnce(deathAudioPlayedOnce);
        }
    }
    
    //Play different attack animations
    void PlayAttackAnimation()
    {
        bool applyDamage = _monsterNavigationScript.distanceToPlayer < 2; //Apply damage if close enough
        int attackIndex = Random.Range(0, 3);
        switch (attackIndex)
        {
            case 0:
                if (applyDamage)
                {
                    playerHealthComponent.ApplyDamage(5f);
                }
                animator.SetTrigger("Attack1");
                monsterAudio.PlayOneShot(punch);
                break;
            case 1:
                if (applyDamage)
                {
                    playerHealthComponent.ApplyDamage(10f);
                }
                animator.SetTrigger("Attack2");
                monsterAudio.PlayOneShot(doublePunch);
                break;
            case 2:
                if (applyDamage)
                {
                    playerHealthComponent.ApplyDamage(15f);
                }
                animator.SetTrigger("Attack3");
                monsterAudio.PlayOneShot(slam);
                break;
            case 3:
                monsterAudio.PlayOneShot(roar);
                break;
        }
        
    }

    IEnumerator StartCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    void PlayDeathSoundOnce(bool hasBeenPlayed)
    {
        if (!hasBeenPlayed)
        {
            monsterAudio.PlayOneShot(death, 2f);
        }

        deathAudioPlayedOnce = true;
    }
}
