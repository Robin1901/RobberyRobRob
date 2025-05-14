using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    [Header("Speeds")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 7.5f;
    public float crouchSpeed = 2f;

    private float currentSpeed = 0f;


    [Header("Mouse Look")]
    public float mouseSensitivity = 100f; // sens
    public Transform cameraTransform;    // char cam
    private float xRotation = 0f;        // camera pitch

    [Header("FOV Settings")]
    private float normalFOV = 60f;        // standard fov
    private float sprintFOV = 67f;        // FOV sprinten
    private float crouchFOV = 53f;      // FOV crouchen
    private float fovLerpSpeed = 5f;      // FOV Anpassungsgeschwindigkeit

    private CharacterController controller;
    private Camera cam;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        cam = cameraTransform.GetComponent<Camera>();


        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked; // cursor weg
        Cursor.visible = false;
    }

    void Update()
    {

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -60f, 75f); // cam max rotation
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // cam drehen

        transform.Rotate(Vector3.up * mouseX); // player drehen

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 rawMove = transform.right * x + transform.forward * z; // bewegen in cam richtung
        if (rawMove.sqrMagnitude > 1f)
            rawMove = rawMove.normalized; // diagonal nicht schneller

        bool shiftPressed = Input.GetKey(KeyCode.LeftShift); // shift gedrückt?
        bool strgPressed = Input.GetKey(KeyCode.LeftControl); // strg gedrückt?

        float targetSpeed; // einmal definieren zum Benutzen
        float targetFOV;

        if (shiftPressed && !strgPressed)
        {
            targetSpeed = sprintSpeed;
            targetFOV = sprintFOV;
        }
        else if (strgPressed && !shiftPressed)
        {
            targetSpeed = crouchSpeed;
            targetFOV = crouchFOV;
        }
        else
        {
            targetSpeed = walkSpeed;
            targetFOV = normalFOV;
        }

        if (rawMove == Vector3.zero)
        {
            currentSpeed = 0f;
        }
        else
        {
            currentSpeed = targetSpeed;
        }
        // Speedänderung smoothen
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed); // FOV änderung smoothen

        controller.Move(rawMove * currentSpeed * Time.deltaTime); // char endlich bewegen
    }
}