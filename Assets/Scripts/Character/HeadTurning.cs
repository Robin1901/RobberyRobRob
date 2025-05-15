using UnityEngine;

public class HeadTurning : MonoBehaviour, IHeadTurning
{
    public Transform bodyTransform;

    public float mouseSensitivity = 100f;
    private float xRotation = 0f;
    private float yRotation = 0f;

    private float headRotationSpeed = 6f;
    public float maxHeadRotation = 15f; // wichtig: public, damit BodyController zugreifen kann
    private float headLeanOffset = 0.175f;
    private float leanSpeed = 5f;

    private float targetHeadRotation = 0f;
    private float targetHeadOffsetX = 0f;

    private Vector3 initialLocalPosition;

    public float LookDirectionY { get; private set; } // aktuelle Y-Drehung des Kopfes (global)

    private void Start()
    {
        initialLocalPosition = transform.localPosition;

        Vector3 euler = transform.localEulerAngles;
        yRotation = euler.y;
        xRotation = euler.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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

        float desiredRotation = 0f;
        float desiredOffsetX = 0f;

        if (isLeaning)
        {
            if (qHeld)
            {
                desiredRotation = maxHeadRotation;
                desiredOffsetX = -headLeanOffset;
            }
            else if (eHeld)
            {
                desiredRotation = -maxHeadRotation;
                desiredOffsetX = headLeanOffset;
            }
        }

        targetHeadRotation = Mathf.Lerp(targetHeadRotation, desiredRotation, Time.deltaTime * headRotationSpeed);
        targetHeadOffsetX = Mathf.Lerp(targetHeadOffsetX, desiredOffsetX, Time.deltaTime * leanSpeed);

        Quaternion leanRotation = Quaternion.Euler(0f, yRotation, targetHeadRotation);
        Quaternion normalRotation = Quaternion.Euler(xRotation, yRotation, targetHeadRotation);

        transform.localRotation = isLeaning ? leanRotation : normalRotation;

        Vector3 newPos = initialLocalPosition;
        newPos.x += targetHeadOffsetX;
        transform.localPosition = newPos;

        // Update öffentliche LookDirection in Weltkoordinaten (nur Y-Achse)
        LookDirectionY = bodyTransform.eulerAngles.y + Mathf.DeltaAngle(bodyTransform.eulerAngles.y, yRotation);

        // Körperrotation im HeadTurning nicht mehr setzen
    }
}
