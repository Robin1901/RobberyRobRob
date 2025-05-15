using UnityEngine;

public class Movement : MonoBehaviour
{
    public MonoBehaviour headTurningBehaviour;
    public Transform bodyTransform;
    private float turnThreshold = 90f;
    private float turnDuration = 0.325f;
    [HideInInspector] public bool isMoving = false;
    [HideInInspector] public bool isSprinting = false;
    [HideInInspector] public bool isCrouching = false;

    private IHeadTurning headTurning;
    private float referenceYaw;
    private bool isTurningInPlace;
    private float turnStartYaw;
    private float turnTargetYaw;
    private float turnElapsed;

    private CharacterController controller;
    private Vector3 rawMove;

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
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        float headYaw = headTurning.LookDirectionY;
        Quaternion yawRot = Quaternion.Euler(0f, headYaw, 0f);
        Vector3 forward = yawRot * Vector3.forward;
        Vector3 right = yawRot * Vector3.right;

        rawMove = right * x + forward * z;
        if (rawMove.sqrMagnitude > 1f) rawMove.Normalize();

        isMoving = rawMove.sqrMagnitude > 0.001f;
        isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);
        isCrouching = isMoving && Input.GetKey(KeyCode.LeftControl);

        if (isMoving)
        {
            isTurningInPlace = false;
            referenceYaw = headYaw;
            SetBodyYaw(headYaw);
        }
        else
        {
            float yawDiff = Mathf.DeltaAngle(referenceYaw, headYaw);

            if (isTurningInPlace)
            {
                if (Mathf.Abs(yawDiff) <= turnThreshold)
                {
                    isTurningInPlace = false;
                    referenceYaw = bodyTransform.eulerAngles.y;
                }
                else
                {
                    turnElapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(turnElapsed / turnDuration);
                    float smoothT = t * t * (3f - 2f * t);
                    float newYaw = Mathf.LerpAngle(turnStartYaw, turnTargetYaw, smoothT);
                    SetBodyYaw(newYaw);

                    if (t >= 1f)
                    {
                        isTurningInPlace = false;
                        referenceYaw = turnTargetYaw;
                    }
                }
            }
            else if (Mathf.Abs(yawDiff) > turnThreshold)
            {
                isTurningInPlace = true;
                turnStartYaw = bodyTransform.eulerAngles.y;
                turnTargetYaw = headYaw;
                turnElapsed = 0f;
            }
        }

        float speed = isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : walkSpeed;
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
