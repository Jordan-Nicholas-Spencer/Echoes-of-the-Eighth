using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class HumanLocomotionController : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 2.0f;
    public float runSpeed  = 5.0f;
    public float rotationSpeed = 720f; // deg/sec

    [Header("Gravity / Jump")]
    public float gravity = -9.81f;
    public float groundedGravity = -2f;
    public float jumpHeight = 1.2f;

    [Header("Animator")]
    public string locomotionParam = "locomotion"; // matches your Blend Tree

    [Header("Camera-relative movement")]
    public bool cameraRelative = true;
    public Transform cameraTransform; // leave empty to auto-use Camera.main

    CharacterController cc;
    Animator anim;
    float yVel;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        // --- INPUT (old Input Manager) ---
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool running = Input.GetKey(KeyCode.LeftShift);
        bool jump    = Input.GetButtonDown("Jump");

        // --- MOVE DIRECTION ---
        Vector3 moveDir;
        if (cameraRelative && cameraTransform)
        {
            Vector3 camFwd = Vector3.Scale(cameraTransform.forward, new Vector3(1, 0, 1)).normalized;
            Vector3 camRight = Vector3.Scale(cameraTransform.right, new Vector3(1, 0, 1)).normalized;
            moveDir = (camFwd * v + camRight * h).normalized;
        }
        else
        {
            moveDir = new Vector3(h, 0, v).normalized;
        }

        float inputMag = Mathf.Clamp01(new Vector2(h, v).magnitude);

        // --- FACE THE MOVE DIRECTION ---
        if (inputMag > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        // --- SPEED & TRANSLATION ---
        float speed = (running ? runSpeed : walkSpeed) * inputMag;

        // gravity & jump (CharacterController needs this done manually)
        bool grounded = cc.isGrounded;
        if (grounded && yVel < 0) yVel = groundedGravity;
        if (grounded && jump) yVel = Mathf.Sqrt(-2f * gravity * jumpHeight);
        yVel += gravity * Time.deltaTime;

        Vector3 velocity = moveDir * speed + Vector3.up * yVel;
        cc.Move(velocity * Time.deltaTime);

        // --- DRIVE YOUR BLEND TREE (0=back, 1=idle, 2=walk, 3=run) ---
        float targetLocomotion = 1f; // idle by default

        if (inputMag > 0.01f)
        {
            // forward vs backward relative to character facing
            float forwardDot = Vector3.Dot(moveDir, transform.forward);

            if (forwardDot < -0.2f)
            {
                // moving backward → slide toward 0 (WalkBack)
                // stronger back input pushes closer to 0
                float t = Mathf.InverseLerp(-1f, -0.2f, forwardDot); // 1 at -1, 0 at -0.2
                targetLocomotion = Mathf.Lerp(1f, 0f, t);
            }
            else
            {
                // moving forward → between 2 (Walk) and 3 (Run)
                targetLocomotion = running ? 3f : 2f;
            }
        }

        // smooth the parameter for nicer blending
        float current = anim.GetFloat(locomotionParam);
        float smoothed = Mathf.MoveTowards(current, targetLocomotion, Time.deltaTime * 6f);
        anim.SetFloat(locomotionParam, smoothed);
    }
}