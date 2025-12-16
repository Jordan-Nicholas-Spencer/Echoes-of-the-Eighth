using System;
using System.Collections;
using System.Collections.Generic;
using Unity_Store_Imports.Ilumisoft.Health_System.Scripts;
using UnityEngine;

public class MonsterCollisions : MonoBehaviour
{
    private Hitbox hitboxScript;
    private void Awake()
    {
        hitboxScript = GetComponent<Hitbox>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fireball"))
        {
            hitboxScript.ApplyDamage(15f);
        }
    }
}
