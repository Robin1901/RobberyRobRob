using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSettings : MonoBehaviour
{
    [Header("FOV Settings")]
    public float normalFOV = 60f;
    public float sprintFOV = 65f;
    public float crouchFOV = 55f;
    public float fovLerpSpeed = 7f;

    private Camera cam;
    public Movement movementScript;

    void Start()
    {
        cam = GetComponent<Camera>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        bool sprint = movementScript.isSprinting;
        bool crouch = movementScript.isCrouching;
        float targetFOV = sprint ? sprintFOV : crouch ? crouchFOV : normalFOV;
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, Time.deltaTime * fovLerpSpeed);
    }

}

