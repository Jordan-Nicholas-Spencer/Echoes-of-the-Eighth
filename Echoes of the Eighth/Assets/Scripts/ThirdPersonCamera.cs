using UnityEngine;

public class ThirdPersonCamera_FinalSnap : MonoBehaviour
{
    public Transform target;
    public Vector3 offset = new Vector3(0, 1.7f, -3f);
    public float sens = 120f;
    public float minPitch = -20f, maxPitch = 70f;
    public float followLerp = 12f;

    float yaw, pitch;
    bool eatFirstMouseDelta = true;

    void Awake()
    {
        if (!target) target = GameObject.FindWithTag("Player")?.transform;
        if (target)
        {
            yaw = target.eulerAngles.y;
            pitch = 10f;
            var rot = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = target.position + rot * offset;
            transform.rotation = rot;
        }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");
        if (eatFirstMouseDelta) { mx = 0f; my = 0f; eatFirstMouseDelta = false; }

        yaw   += mx * sens * Time.deltaTime;
        pitch -= my * sens * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        var rot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desired = target.position + rot * offset;
        transform.position = Vector3.Lerp(transform.position, desired, 1f - Mathf.Exp(-followLerp * Time.deltaTime));
        transform.rotation = rot;
    }
}