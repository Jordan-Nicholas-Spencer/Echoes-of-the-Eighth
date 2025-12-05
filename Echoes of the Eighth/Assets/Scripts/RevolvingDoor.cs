using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolvingDoor : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 25f;
    
    //TODO add as a listener for when the fireball hits the collider, door will open when both are hit
    void Rotate()
    {
        while (transform.rotation.y < 90)
        {
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        }
    }
}
