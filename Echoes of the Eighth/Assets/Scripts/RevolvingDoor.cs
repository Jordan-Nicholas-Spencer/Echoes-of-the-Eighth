using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RevolvingDoor : MonoBehaviour
{
    [SerializeField] private float rotateSpeed = 25f;
    
    //TODO add as a listener for when the fireball hits the collider, door will open when both are hit
    public void Rotate()
    {
        StartCoroutine(RotateDoor());
        while (transform.rotation.y < 90)
        {
            transform.Rotate(0f, rotateSpeed * Time.deltaTime, 0f);
        }
    }

    private IEnumerator RotateDoor()
    {
        float targetAngle = 90f;
        float currentAngle = transform.eulerAngles.y;

        while (currentAngle < targetAngle)
        {
            float step = rotateSpeed * Time.deltaTime;
            currentAngle += step;
            
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            yield return null;
        }
    }
}
