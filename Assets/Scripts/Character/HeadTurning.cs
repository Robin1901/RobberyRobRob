using UnityEngine;

public class HeadTurning : MonoBehaviour, IHeadTurning
{
    public Transform bodyTransform;
    public Transform chestTransform;
    public Transform neckAnchor;

    public float mouseSensitivity = 100f;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private float headRotationSpeed = 6f;
    private float leanSpeed = 5f;

    private float headMaxZRotation = 25f;
    private float headMaxXOffset = 0.225f;
    private float chestMaxZRotation = 10f;
    private float chestMaxXOffset = 0.115f;

    private float targetHeadZRot = 0f;
    private float targetHeadXOffset = 0f;
    private float targetChestZRot = 0f;
    private float targetChestXOffset = 0f;

    private Vector3 initialLocalPosition;
    private Vector3 chestInitialLocalPosition;

    public float LookDirectionY { get; private set; }

    private void Start()
    {
        initialLocalPosition = transform.localPosition;
        chestInitialLocalPosition = chestTransform.localPosition;

        Vector3 euler = transform.localEulerAngles;
        yRotation = euler.y;
        xRotation = euler.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void LateUpdate()
    {
        transform.position = neckAnchor.position;
    }

    private void Update()
    {
        bool qHeld = Input.GetKey(KeyCode.Q);
        bool eHeld = Input.GetKey(KeyCode.E);

        float camPitch = xRotation;
        if (camPitch > 180f) camPitch -= 360f;

        bool canLean = camPitch >= -5f && camPitch <= 10f;
        bool isLeaning = canLean && (qHeld || eHeld);

        if (!isLeaning)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            yRotation += mouseX;
            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -60f, 75f);
        }

        float headTargetZ = 0f;
        float headTargetX = 0f;
        float chestTargetZ = 0f;
        float chestTargetX = 0f;

        if (isLeaning)
        {
            if (qHeld)
            {
                headTargetZ = headMaxZRotation;
                headTargetX = -headMaxXOffset;
                chestTargetZ = chestMaxZRotation;
                chestTargetX = -chestMaxXOffset;
            }
            else if (eHeld)
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

        Quaternion headRotation = Quaternion.Euler(xRotation, yRotation, targetHeadZRot);
        Quaternion chestRotation = Quaternion.Euler(0f, chestTransform.localEulerAngles.y, targetChestZRot);

        transform.localRotation = headRotation;
        chestTransform.localRotation = chestRotation;

        Vector3 headLocalPos = initialLocalPosition;
        headLocalPos.x += targetHeadXOffset;
        transform.localPosition = headLocalPos;

        Vector3 chestLocalPos = chestInitialLocalPosition;
        chestLocalPos.x += targetChestXOffset;
        chestTransform.localPosition = chestLocalPos;

        LookDirectionY = bodyTransform.eulerAngles.y + Mathf.DeltaAngle(bodyTransform.eulerAngles.y, yRotation);
    }
}
