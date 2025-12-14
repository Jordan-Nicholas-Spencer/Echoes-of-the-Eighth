using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This will move the object over time
/// </summary>
public class SFB_MoveOverTime : MonoBehaviour {

	public Vector3 travelDirection;		// Direction of travel
	public bool local = false;			// True if yo uwant to move on local axis
	public float speed = 1.0f;			// Speed of movement
	public float delay = 0.0f;			// Initial delay before movement starts
	public bool isMoving = false;		// True if moving

	[Header("Collision handling")]
    [Tooltip("Disable all colliders for a short time after spawn to avoid hitting the owner.")]
    public float enableColliderDelay = 0.000005f;

	// cache colliders to toggle
	Collider[] _colliders;

	void Awake()
    {
        _colliders = GetComponentsInChildren<Collider>(true);
    }

	// Use this for initialization
	void Start()
    {
        // Small grace period so we don't collide with the player at spawn
        if (enableColliderDelay > 0f && _colliders != null)
            StartCoroutine(EnableCollidersAfterDelay(enableColliderDelay));

        if (delay > 0f) Invoke(nameof(StartMoving), delay);
        else StartMoving();
    }
	
	IEnumerator EnableCollidersAfterDelay(float t)
    {
        // turn off all colliders briefly
        foreach (var c in _colliders) if (c) c.enabled = false;
        yield return new WaitForSeconds(t);
        foreach (var c in _colliders) if (c) c.enabled = true;
    }

	public void StartMoving(){
		isMoving = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (isMoving) {
			if (local) {
				Vector3 dir = transform.forward.normalized;
       			transform.position += dir * Time.deltaTime * speed;
			} else {
				transform.position += travelDirection * Time.deltaTime * speed;
			}
		}
	}

	// Destroy when hitting anything
	void OnCollisionEnter(Collision collision)
	{
		Destroy(gameObject);
	}

	// Also catch trigger collisions (if using trigger colliders)
	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Blue Torch"))
		{
			print("hit");
			var torch = other.GetComponent<ActivateBlueTorch>();
			if (torch != null)
			{
				torch.ToggleLight();
			}
		}
		Destroy(gameObject);
	}
}
