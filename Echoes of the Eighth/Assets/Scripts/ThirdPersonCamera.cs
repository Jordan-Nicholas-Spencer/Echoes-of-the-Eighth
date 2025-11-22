using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;                       // drag your Human root here
    public Vector3 offset = new Vector3(0, 1.6f, -3f);
    public float followSmooth = 10f;
    public float mouseSensitivity = 150f;
    public float minPitch = -30f, maxPitch = 70f;

    float yaw, pitch;

    void Start()
    {
        if (!target)
            target = GameObject.FindWithTag("Player")?.transform; // optional convenience
        var a = transform.eulerAngles; yaw = a.y; pitch = a.x;

        Cursor.lockState = CursorLockMode.Locked;  // optional
        Cursor.visible = false;
    }

    void LateUpdate()
    {
        if (!target) return;

        // mouse orbit
        yaw   += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        var rot = Quaternion.Euler(pitch, yaw, 0f);

        // follow
        var desiredPos = target.position + rot * offset;
        transform.position = Vector3.Lerp(transform.position, desiredPos, 1f - Mathf.Exp(-followSmooth * Time.deltaTime));
        transform.rotation = rot;
    }
}