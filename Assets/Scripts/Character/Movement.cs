using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("References")]
    public MonoBehaviour headTurningBehaviour;
    public Transform bodyTransform;

    [Header("Settings")]
    private float turnThreshold = 90f;
    private float turnDuration = 0.325f;

    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public bool isSprinting = false;
    [HideInInspector] public bool isCrouching = false;

    private IHeadTurning headTurning;
    private CharacterController controller;

    private Vector3 rawMove;

    private float referenceYaw;
    private bool isTurningInPlace;
    private float turnStartYaw;
    private float turnTargetYaw;
    private float turnElapsed;

    private float walkSpeed = 4.5f;
    private float sprintSpeed = 7f;
    private float crouchSpeed = 2f;

    private void Awake()
    {
        headTurning = headTurningBehaviour as IHeadTurning;
        controller = GetComponent<CharacterController>();
    }

    private void Start()
    {
        referenceYaw = bodyTransform.eulerAngles.y;
    }

    private void Update()
    {
        HandleInput();

        UpdateMovementState();

        HandleTurning();

        ApplyMovement();
    }

    private void HandleInput()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputZ = Input.GetAxisRaw("Vertical");

        float headYaw = headTurning.LookDirectionY;
        Quaternion headRotation = Quaternion.Euler(0f, headYaw, 0f);

        Vector3 forward = headRotation * Vector3.forward;
        Vector3 right = headRotation * Vector3.right;

        rawMove = (right * inputX + forward * inputZ);
        if (rawMove.sqrMagnitude > 1f) rawMove.Normalize();
    }

    private void UpdateMovementState()
    {
        isMoving = rawMove.sqrMagnitude > 0.001f;
        isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);
        isCrouching = isMoving && Input.GetKey(KeyCode.LeftControl);
    }

    private void HandleTurning()
    {
        float headYaw = headTurning.LookDirectionY;
        float yawDifference = Mathf.DeltaAngle(referenceYaw, headYaw);

        if (isMoving)
        {
            isTurningInPlace = false;
            referenceYaw = headYaw;
            SetBodyYaw(headYaw);
            return;
        }

        if (isTurningInPlace)
        {
            if (Mathf.Abs(yawDifference) <= turnThreshold)
            {
                isTurningInPlace = false;
                referenceYaw = bodyTransform.eulerAngles.y;
            }
            else
            {
                turnElapsed += Time.deltaTime;
                float t = Mathf.Clamp01(turnElapsed / turnDuration);
                float smoothT = t * t * (3f - 2f * t); // Smoothstep
                float newYaw = Mathf.LerpAngle(turnStartYaw, turnTargetYaw, smoothT);
                SetBodyYaw(newYaw);

                if (t >= 1f)
                {
                    isTurningInPlace = false;
                    referenceYaw = turnTargetYaw;
                }
            }
        }
        else if (Mathf.Abs(yawDifference) > turnThreshold)
        {
            isTurningInPlace = true;
            turnStartYaw = bodyTransform.eulerAngles.y;
            turnTargetYaw = headYaw;
            turnElapsed = 0f;
        }
    }

    private void ApplyMovement()
    {
        float speed = walkSpeed;

        if (isSprinting) speed = sprintSpeed;
        else if (isCrouching) speed = crouchSpeed;

        controller.Move(rawMove * speed * Time.deltaTime);
    }

    private void SetBodyYaw(float yaw)
    {
        bodyTransform.rotation = Quaternion.Euler(0f, yaw, 0f);
    }
}

public interface IHeadTurning
{
    float LookDirectionY { get; }
}
