using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MonsterAnimation : MonoBehaviour
{
    public Animator animator;

    private MonsterNavigation _monsterNavigationScript;

    private NavMeshAgent monsterAgent;

    private bool canAttack = true;
    [SerializeField] private float attackCooldown = 5f;

    private AudioSource monsterAudio;
    [SerializeField] private AudioClip punch;
    [SerializeField] private AudioClip slam;
    [SerializeField] private AudioClip doublePunch;
    [SerializeField] private AudioClip roar;
    private void Awake()
    {
        _monsterNavigationScript = GetComponent<MonsterNavigation>();
        monsterAgent = _monsterNavigationScript.monsterAgent;
        monsterAudio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
       // Debug.Log("distance: " + _monsterNavigationScript.distanceToPlayer);
        //Debug.Log("Attack1 Param: " + animator.GetBool("Attack1"));
        
        animator.SetFloat("Speed", monsterAgent.velocity.magnitude);
        if (_monsterNavigationScript.distanceToPlayer < 5 && canAttack)
        {
            PlayAttackAnimation();
            //monsterAgent.speed = 0f;
            canAttack = false;
            StartCoroutine(StartCooldown());
        }
    }

    void PlayAttackAnimation()
    {
        int attackIndex = Random.Range(0, 3);
        print(attackIndex);
        switch (attackIndex)
        {
            case 0:
                animator.SetTrigger("Attack1");
                monsterAudio.PlayOneShot(punch);
                break;
            case 1:
                animator.SetTrigger("Attack2");
                monsterAudio.PlayOneShot(doublePunch);
                break;
            case 2:
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
}
