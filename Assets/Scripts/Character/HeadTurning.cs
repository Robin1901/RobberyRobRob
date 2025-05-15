using UnityEngine;

public class HeadTurning : MonoBehaviour, IHeadTurning
{
    [Header("References")]
    public Transform bodyTransform;
    public Transform chestTransform;
    public Transform neckAnchor;
    public Movement movement;

    [Header("Settings")]
    public float mouseSensitivity = 100f;
    private float headRotationSpeed = 6f;
    private float leanSpeed = 5f;

    private float headMaxZRotation = 22.5f;
    private float headMaxXOffset = 0.225f;
    private float chestMaxZRotation = 10f;
    private float chestMaxXOffset = 0.115f;

    private float xRotation = 0f;
    private float yRotation = 0f;

    private float targetHeadZRot = 0f;
    private float targetHeadXOffset = 0f;
    private float targetChestZRot = 0f;
    private float targetChestXOffset = 0f;

    private Vector3 initialLocalPosition;
    private Vector3 chestInitialLocalPosition;

    private float bobTimer = 0f;
    private Vector3 bobOffset = Vector3.zero;

    public float LookDirectionY { get; private set; }

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
        chestInitialLocalPosition = chestTransform.localPosition;

        transform.position = neckAnchor.position;

        Vector3 euler = transform.localEulerAngles;
        yRotation = euler.y;
        xRotation = euler.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLook();

        HandleLeaning();

        HandleHeadbob();

        ApplyRotations();

        ApplyPositions();

        UpdateLookDirectionY();
    }

    private void HandleMouseLook()
    {
        bool isLeaning = IsLeaning();
        if (isLeaning) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yRotation += mouseX;
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 75f);
    }

    private bool IsLeaning()
    {
        float pitch = xRotation > 180f ? xRotation - 360f : xRotation;
        return pitch >= -5f && pitch <= 10f && (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E));
    }

    private void HandleLeaning()
    {
        float headTargetZ = 0f, headTargetX = 0f;
        float chestTargetZ = 0f, chestTargetX = 0f;

        if (IsLeaning())
        {
            if (Input.GetKey(KeyCode.Q))
            {
                headTargetZ = headMaxZRotation;
                headTargetX = -headMaxXOffset;
                chestTargetZ = chestMaxZRotation;
                chestTargetX = -chestMaxXOffset;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                headTargetZ = -headMaxZRotation;
                headTargetX = headMaxXOffset;
                chestTargetZ = -chestMaxZRotation;
                chestTargetX = chestMaxXOffset;
            }
        }

        targetHeadZRot = Mathf.Lerp(targetHeadZRot, headTargetZ, Time.deltaTime * headRotationSpeed);
        targetHeadXOffset = Mathf.Lerp(targetHeadXOffset, headTargetX, Time.deltaTime * leanSpeed);
        targetChestZRot = Mathf.Lerp(targetChestZRot, chestTargetZ, Time.deltaTime * headRotationSpeed);
        targetChestXOffset = Mathf.Lerp(targetChestXOffset, chestTargetX, Time.deltaTime * leanSpeed);
    }

    private void HandleHeadbob()
    {
        float speedFactor, bobAmountY, bobAmountX;

        if (movement.isSprinting)
        {
            speedFactor = 25f;
            bobAmountY = 0.015f;
            bobAmountX = 0.02f;
        }
        else if (movement.isMoving && !movement.isCrouching)
        {
            speedFactor = 15f;
            bobAmountY = 0.015f;
            bobAmountX = 0.01f;
        }
        else
        {
            speedFactor = 4f;
            bobAmountY = 0.005f;
            bobAmountX = 0f;
        }

        bobTimer += Time.deltaTime * speedFactor;
        bobOffset.y = Mathf.Sin(bobTimer) * bobAmountY;
        bobOffset.x = Mathf.Cos(bobTimer * 0.5f) * bobAmountX;
    }

    private void ApplyRotations()
    {
        Quaternion headRotation = Quaternion.Euler(xRotation, yRotation, targetHeadZRot);
        Quaternion chestRotation = Quaternion.Euler(0f, chestTransform.localEulerAngles.y, targetChestZRot);

        transform.localRotation = headRotation;
        chestTransform.localRotation = chestRotation;
    }

    private void ApplyPositions()
    {
        Vector3 leanOffset = bodyTransform.right * targetHeadXOffset + new Vector3(bobOffset.x, bobOffset.y, 0f);
        transform.localPosition = initialLocalPosition + leanOffset;

        Vector3 chestPos = chestInitialLocalPosition;
        chestPos.x += targetChestXOffset;
        chestTransform.localPosition = chestPos;
    }

    private void UpdateLookDirectionY()
    {
        float bodyYaw = bodyTransform.eulerAngles.y;
        LookDirectionY = bodyYaw + Mathf.DeltaAngle(bodyYaw, yRotation);
    }
}
