using UnityEngine;

public class HeadTurning : MonoBehaviour, IHeadTurning
{
    public Transform bodyTransform;
    public Transform chestTransform;
    public Transform neckAnchor;
    public Movement movement; // Referenz auf dein Movement-Skript

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

        // Headbob / Atembewegung
        float speedFactor;
        float bobAmountY;
        float bobAmountX;

        if (movement.isSprinting) //Sprinten
        {
            speedFactor = 25f;
            bobAmountY = 0.015f;
            bobAmountX = 0.02f;
        }
        else if (movement.isMoving && !movement.isCrouching) //Laufen
        {
            speedFactor = 15f;
            bobAmountY = 0.015f;
            bobAmountX = 0.01f;
        }
        else //Langsames Atmen beim Crouchen & Stehen
        {
            speedFactor = 4f;
            bobAmountY = 0.005f;
            bobAmountX = 0f;
        }

        bobTimer += Time.deltaTime * speedFactor;
        bobOffset.y = Mathf.Sin(bobTimer) * bobAmountY;
        bobOffset.x = Mathf.Cos(bobTimer * 0.5f) * bobAmountX;

        // Apply Rotations
        Quaternion headRotation = Quaternion.Euler(xRotation, yRotation, targetHeadZRot);
        Quaternion chestRotation = Quaternion.Euler(0f, chestTransform.localEulerAngles.y, targetChestZRot);

        transform.localRotation = headRotation;
        chestTransform.localRotation = chestRotation;

        // Apply Positions (Lean + Bob)
        Vector3 leanOffset = bodyTransform.right * targetHeadXOffset; // relativ zum Körper nach rechts/links
        leanOffset += new Vector3(bobOffset.x, bobOffset.y, 0f);      // Atmen + Kopf bobben (nur x & y)

        Vector3 headPos = initialLocalPosition + leanOffset;

        transform.localPosition = headPos;

        Vector3 chestLocalPos = chestInitialLocalPosition;
        chestLocalPos.x += targetChestXOffset;
        chestTransform.localPosition = chestLocalPos;

        // LookDirectionY für Movement
        LookDirectionY = bodyTransform.eulerAngles.y
                     + Mathf.DeltaAngle(bodyTransform.eulerAngles.y, yRotation);
    }
}
