using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class HumanGameplayController : MonoBehaviour
{
    // ========= Movement (ground is snappy) =========
    [Header("Movement (ground is snappy)")]
    public float walkSpeed = 2.6f;
    public float runSpeed  = 5.4f;
    public float rotationSpeed = 900f;
    public float groundSnapRate = 100f;
    public float groundBrakeRate = 90f;

    // ========= Air & Gravity =========
    [Header("Air control & gravity")]
    public float airControl = 5f;
    public float gravity = -9.81f;
    [Range(1f, 2.5f)] public float airGravityMultiplier = 1.35f;
    public float groundedGravity = -2f;
    public float jumpHeight = 0.95f;
    public float coyoteTime = 0.12f;
    public float jumpBuffer = 0.12f;
    public float jumpCooldown = 0.12f;

    // ========= Animator states =========
    [Header("Animator states (exact names)")]
    public string locomotionParam = "locomotion"; // 0 back, 1 idle, 2 walk, 3 run
    public string stJump   = "RunningJump02";
    public string stKickR  = "KickR";
    public string stCast01 = "CastSpell01";
    public string stCast02 = "CastSpell02";

    // ========= Action behaviour =========
    [Header("Action behavior")]
    public float preActionBrakeTime = 0.12f;
    public float preActionBrakeRate = 100f;
    public bool  freezeHorizontalDuringAction = true;

    // ========= Feel sliders =========
    [Header("Sliders (tune in Inspector)")]
    [Range(0.0f, 2.0f)] public float kickEarlyUnlockSeconds = 0.5f;
    [Range(0.05f, 1.0f)] public float castToLocomotionFade = 0.5f;
    [Range(0.0f, 0.4f)]  public float castHandoffDelay = 0.12f;

    // ========= Movement frame (camera-relative input only; no camera control) =========
    [Header("Camera-relative movement")]
    public bool cameraRelative = true;
    public Transform cameraTransform; // used only to read forward/right for input

    [Header("Back-walk behavior")]
    public bool faceInputWhenBacking = true; // true = turn to face input (no slide)

    // ========= FX (integrated) =========
    [Header("Barbarian FX (integrated)")]
    public GameObject particleBarbarian1;   // fireball prefab
    public Transform positionBarbarian1;    // spawn
    public float repeatEverySeconds = 1.2f;
    public float fxLifetimeSeconds = 7f;

    // ---- internals ----
    CharacterController cc;
    Animator anim;
    Vector3 planarVel = Vector3.zero;
    float yVel = 0f;
    float lastGroundedTime = -999f;
    float lastJumpPressedTime = -999f;
    float nextJumpAllowedTime = 0f;
    bool actionLocked = false;
    bool brakingForAction = false;
    bool wasGrounded = false;
    Coroutine castRoutine;

    readonly Dictionary<string, float> clipLen = new Dictionary<string, float>();

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;

        if (anim && anim.runtimeAnimatorController)
            foreach (var c in anim.runtimeAnimatorController.animationClips)
                if (!clipLen.ContainsKey(c.name)) clipLen[c.name] = c.length;
    }

    void Update()
    {
        // INPUT
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool runHeld   = Input.GetKey(KeyCode.LeftShift);
        bool jumpDown  = Input.GetButtonDown("Jump");
        bool leftDown  = Input.GetMouseButtonDown(0);
        bool rightDown = Input.GetMouseButtonDown(1);
        if (jumpDown) lastJumpPressedTime = Time.time;

        // Move dir (camera-relative)
        Vector3 inputDir;
        if (cameraRelative && cameraTransform)
        {
            Vector3 f = Vector3.Scale(cameraTransform.forward, new Vector3(1,0,1)).normalized;
            Vector3 r = Vector3.Scale(cameraTransform.right,   new Vector3(1,0,1)).normalized;
            inputDir = (f * v + r * h).normalized;
        }
        else inputDir = new Vector3(h,0f,v).normalized;

        float inputMag = Mathf.Clamp01(new Vector2(h, v).magnitude);
        bool isMoving  = inputMag > 0.01f;
        bool isRunning = runHeld && isMoving;
        Vector3 desiredPlanar = inputDir * (isRunning ? runSpeed : walkSpeed) * inputMag;

        // Rotation (fix slide when backing if requested)
        if (!actionLocked && !brakingForAction && isMoving)
        {
            if (faceInputWhenBacking)
            {
                Quaternion tgt = Quaternion.LookRotation(inputDir, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, tgt, rotationSpeed * Time.deltaTime);
            }
            else
            {
                float forwardDot = Vector3.Dot(inputDir, transform.forward);
                if (forwardDot >= -0.25f)
                {
                    Quaternion tgt = Quaternion.LookRotation(inputDir, Vector3.up);
                    transform.rotation = Quaternion.RotateTowards(transform.rotation, tgt, rotationSpeed * Time.deltaTime);
                }
            }
        }

        // Grounding
        bool grounded = cc.isGrounded;
        if (grounded)
        {
            lastGroundedTime = Time.time;
            if (yVel < 0f) yVel = groundedGravity;
            if (!wasGrounded) planarVel = isMoving ? desiredPlanar : Vector3.zero; // dump on land
        }

        // Planar velocity (momentum only in air)
        if (!actionLocked && !brakingForAction)
        {
            if (grounded)
            {
                planarVel = isMoving
                    ? Vector3.MoveTowards(planarVel, desiredPlanar, groundSnapRate * Time.deltaTime)
                    : Vector3.MoveTowards(planarVel, Vector3.zero,  groundBrakeRate * Time.deltaTime);
            }
            else
            {
                planarVel = Vector3.MoveTowards(planarVel, desiredPlanar, airControl * Time.deltaTime);
            }
        }

        // Jump (locked until grounded)
        TryJumpStayInAirUntilGround(grounded);

        // Apply motion
        float grav = grounded ? gravity : gravity * airGravityMultiplier;
        yVel += grav * Time.deltaTime;
        cc.Move((planarVel + Vector3.up * yVel) * Time.deltaTime);

        // Locomotion param
        float targetLoc = 1f;
        if (!actionLocked && !brakingForAction && isMoving)
        {
            if (!faceInputWhenBacking && transform.InverseTransformDirection(inputDir).z < -0.1f)
                targetLoc = 0f; // WalkBack threshold
            else
                targetLoc = isRunning ? 3f : 2f;
        }
        anim.SetFloat(locomotionParam, Mathf.MoveTowards(anim.GetFloat(locomotionParam), targetLoc, Time.deltaTime * 10f));

        // Actions
        if (!actionLocked && !brakingForAction && rightDown && grounded)
            StartCoroutine(BrakeThenKickEarlyUnlock());

        // Cast is single, locked action (extra clicks ignored)
        if (!actionLocked && !brakingForAction && leftDown && grounded)
            castRoutine = StartCoroutine(BrakeThenCastOnceLocked());

        wasGrounded = grounded;
    }

    // ===== Jump =====
    void TryJumpStayInAirUntilGround(bool grounded)
    {
        if (Time.time < nextJumpAllowedTime || actionLocked || brakingForAction) return;
        bool canCoyote = Time.time - lastGroundedTime <= coyoteTime;
        bool buffered  = Time.time - lastJumpPressedTime <= jumpBuffer;
        if ((grounded || canCoyote) && buffered)
        {
            StartCoroutine(JumpLockUntilGrounded());
            yVel = Mathf.Sqrt(-2f * gravity * jumpHeight);
            lastJumpPressedTime = -999f;
            nextJumpAllowedTime = Time.time + jumpCooldown;
        }
    }
    IEnumerator JumpLockUntilGrounded()
    {
        actionLocked = true;
        anim.CrossFade(stJump, 0.05f, 0, 0f);
        float guard = 0f;
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stJump) && guard < 0.5f) { guard += Time.deltaTime; yield return null; }
        while (!cc.isGrounded) yield return null;
        actionLocked = false;
    }

    // ===== Kick =====
    IEnumerator BrakeThenKickEarlyUnlock()
    {
        brakingForAction = true;
        float t = 0f;
        while (t < preActionBrakeTime && planarVel.sqrMagnitude > 0.0004f)
        { planarVel = Vector3.MoveTowards(planarVel, Vector3.zero, preActionBrakeRate * Time.deltaTime); t += Time.deltaTime; yield return null; }
        planarVel = Vector3.zero; brakingForAction = false;

        actionLocked = true;
        anim.CrossFade(stKickR, 0.06f, 0, 0f);

        float guard = 0f;
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stKickR) && guard < 0.5f)
        { guard += Time.deltaTime; if (freezeHorizontalDuringAction) planarVel = Vector3.zero; yield return null; }

        float len = clipLen.ContainsKey(stKickR) ? clipLen[stKickR] : 1.2f;
        float unlockAt = Mathf.Max(0.05f, len - Mathf.Clamp(kickEarlyUnlockSeconds, 0.0f, len * 0.95f));

        float played = 0f;
        while (played < unlockAt)
        { if (freezeHorizontalDuringAction) planarVel = Vector3.zero; played += Time.deltaTime; yield return null; }

        actionLocked = false; // early unlock
    }

    // ===== Cast ONCE (locked) – particle spawns only after CastSpell02 starts =====
    IEnumerator BrakeThenCastOnceLocked()
    {
        brakingForAction = true;
        float t = 0f;
        while (t < preActionBrakeTime && planarVel.sqrMagnitude > 0.0004f)
        { planarVel = Vector3.MoveTowards(planarVel, Vector3.zero, preActionBrakeRate * Time.deltaTime); t += Time.deltaTime; yield return null; }
        planarVel = Vector3.zero; brakingForAction = false;

        actionLocked = true;

        // Enter Cast 01
        anim.CrossFade(stCast01, 0.06f, 0, 0f);

        // Wait in Cast 01
        float guard = 0f;
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stCast01) && guard < 0.5f)
        { guard += Time.deltaTime; if (freezeHorizontalDuringAction) planarVel = Vector3.zero; yield return null; }

        // Optional small delay then go to Cast 02
        if (castHandoffDelay > 0f) yield return new WaitForSeconds(castHandoffDelay);
        anim.CrossFade(stCast02, 0.06f, 0, 0f);

        // >>> Start FX only after we actually enter Cast 02 <<<
        float enterGuard = 0f;
        while (!anim.GetCurrentAnimatorStateInfo(0).IsName(stCast02) && enterGuard < 0.5f)
        { enterGuard += Time.deltaTime; if (freezeHorizontalDuringAction) planarVel = Vector3.zero; yield return null; }
        StartCastBarbarian1();

        // Wait for Cast 02 to finish (safe timeout)
        float len02 = clipLen.ContainsKey(stCast02) ? clipLen[stCast02] : 1.0f;
        float timeout = Mathf.Max(len02 + 0.25f, 0.6f);
        float t2 = 0f;
        while (t2 < timeout)
        {
            var st = anim.GetCurrentAnimatorStateInfo(0);
            if (st.IsName(stCast02) && st.normalizedTime >= 0.98f && !anim.IsInTransition(0)) break;
            t2 += Time.deltaTime; if (freezeHorizontalDuringAction) planarVel = Vector3.zero; yield return null;
        }

        StopCastBarbarian1();
        actionLocked = false;
        anim.CrossFade("Locomotion", castToLocomotionFade, 0, 0f);
    }

    // ===== FX helpers (animation events may also call these) =====
    public void StartCastBarbarian1()
    {
        CancelInvoke(nameof(CastBarbarian1));
        CastBarbarian1(); // first shot when Cast02 begins
        if (repeatEverySeconds > 0f)
            InvokeRepeating(nameof(CastBarbarian1), repeatEverySeconds, repeatEverySeconds);
    }
    public void StopCastBarbarian1() => CancelInvoke(nameof(CastBarbarian1));

    public void CastBarbarian1()
    {
        if (!particleBarbarian1 || !positionBarbarian1) return;

        // Spawn the prefab
        var go = Instantiate(particleBarbarian1, positionBarbarian1.position, Quaternion.identity);

        // Face the character’s world Y rotation
        Vector3 fwd = transform.forward;
        fwd.y = 0f;
        if (fwd.sqrMagnitude < 1e-6f) fwd = Vector3.forward;
        go.transform.forward = fwd.normalized;

        // If the prefab uses a child for motion, also reorient that
        foreach (Transform child in go.transform)
            child.forward = fwd.normalized;

        // Optional: kick its custom mover script on if present
        var mover = go.GetComponent<SFB_MoveOverTime>();
        if (mover != null)
            mover.SendMessage("StartMoveOverTime", SendMessageOptions.DontRequireReceiver);

        if (fxLifetimeSeconds > 0f)
            Destroy(go, fxLifetimeSeconds);
    }
}