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
    public float mouseSensitivity = 100f;
    public Transform cameraTransform;    // char cam
    private float xRotation = 0f;        // camera pitch

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();

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
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f); //cam drehen

        transform.Rotate(Vector3.up * mouseX); //player drehen

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 rawMove = transform.right * x + transform.forward * z; //bewegen in cam richtung
        if (rawMove.sqrMagnitude > 1f)
            rawMove = rawMove.normalized; //diagonal nicht schneller

        bool shiftPressed = Input.GetKey(KeyCode.LeftShift); //shift gedrückt?
        bool strgPressed = Input.GetKey(KeyCode.LeftControl); //strg gedrückt?

        float targetSpeed;
        if (shiftPressed && !strgPressed)
        {
            targetSpeed = sprintSpeed;
        }
        else if (strgPressed && !shiftPressed)
        {
            targetSpeed = crouchSpeed;
        }
        else
        {
            targetSpeed = walkSpeed;
        }

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 10f); //speed unterschiede smoothen

        controller.Move(rawMove * currentSpeed * Time.deltaTime); //char endlich bewegen
    }
}




