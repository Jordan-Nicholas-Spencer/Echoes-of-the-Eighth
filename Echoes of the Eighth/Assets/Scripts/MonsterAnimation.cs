using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterAnimation : MonoBehaviour
{
    public Animator animator;

    private MonsterNavigation _monsterNavigationScript;

    private void Awake()
    {
        _monsterNavigationScript = GetComponent<MonsterNavigation>();
    }

    // Update is called once per frame
    void Update()
    {
        animator.SetFloat("Speed", _monsterNavigationScript.monsterAgent.velocity.magnitude);
    }
}
