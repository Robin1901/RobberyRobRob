using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 4.5f;
    public float sprintSpeed = 7f;
    public float crouchSpeed = 2f;

    // Öffentlich, damit andere Skripte sie abfragen können:
    public bool isSprinting { get; private set; }
    public bool isCrouching { get; private set; }

    private float currentSpeed = 0f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;
    private float xRotation = 0f;

    [Header("FOV Settings")]
    private float normalFOV = 60f;
    private float sprintFOV = 65f;
    private float crouchFOV = 55f;
    private float fovLerpSpeed = 7f;

    private CharacterController controller;
    private Camera cam;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        cam = cameraTransform.GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // --- Mouse Look ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRotation = Mathf.Clamp(xRotation - mouseY, -60f, 75f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);

        // --- Movement Input ---
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        Vector3 rawMove = transform.right * x + transform.forward * z;
        if (rawMove.sqrMagnitude > 1f) rawMove = rawMove.normalized;

        // --- Zustand erkennen ---
        isSprinting = Input.GetKey(KeyCode.LeftShift);
        isCrouching = Input.GetKey(KeyCode.LeftControl);

        float targetSpeed;
        float targetFOV;
        if (isSprinting && !isCrouching)
        {
            targetSpeed = sprintSpeed;
            targetFOV = sprintFOV;
        }
        else if (isCrouching && !isSprinting)
        {
            targetSpeed = crouchSpeed;
            targetFOV = crouchFOV;
        }
        else
        {
            targetSpeed = walkSpeed;
            targetFOV = normalFOV;
        }

        // --- Sofort starten / stoppen ---
        if (rawMove == Vector3.zero)
            currentSpeed = 0f;
        else
            currentSpeed = targetSpeed;

        // --- Anwenden ---
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
        controller.Move(rawMove * currentSpeed * Time.deltaTime);
    }
}
