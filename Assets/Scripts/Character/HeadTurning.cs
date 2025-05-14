using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadTurning : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Transform headPivot;
    public Transform bodyTransform;

    private float headRotationSpeed = 6f;
    private float maxHeadRotation = 15f;
    private float headLeanOffset = 0.15f;
    private float leanSpeed = 7f;

    private float targetHeadRotation = 0f;
    private float targetHeadOffsetX = 0f;
    private Vector3 initialLocalPosition;

    void Start()
    {
        initialLocalPosition = headPivot.localPosition;
    }

    void Update()
    {
        float camPitch = cameraTransform.localEulerAngles.x;
        if (camPitch > 180f) camPitch -= 360f;

        if (Input.GetKey(KeyCode.Q)) //links
        {
            targetHeadRotation = Mathf.Lerp(targetHeadRotation, maxHeadRotation, Time.deltaTime * headRotationSpeed);
            targetHeadOffsetX = Mathf.Lerp(targetHeadOffsetX, -headLeanOffset, Time.deltaTime * leanSpeed);
        }
        else if (Input.GetKey(KeyCode.E)) // rechts
        {
            targetHeadRotation = Mathf.Lerp(targetHeadRotation, -maxHeadRotation, Time.deltaTime * headRotationSpeed);
            targetHeadOffsetX = Mathf.Lerp(targetHeadOffsetX, headLeanOffset, Time.deltaTime * leanSpeed);
        }
        else
        {
            targetHeadRotation = Mathf.Lerp(targetHeadRotation, 0f, Time.deltaTime * headRotationSpeed);
            targetHeadOffsetX = Mathf.Lerp(targetHeadOffsetX, 0f, Time.deltaTime * leanSpeed);
        }

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            headPivot.localRotation = Quaternion.Euler(0f, 0f, targetHeadRotation);
        }
        else
        {
            headPivot.localRotation = Quaternion.Euler(camPitch, 0f, targetHeadRotation);
        }

        Vector3 newPos = initialLocalPosition;
        newPos.x += targetHeadOffsetX;
        headPivot.localPosition = newPos;

        float bodyYaw = bodyTransform.localEulerAngles.y;
        bodyTransform.localRotation = Quaternion.Euler(0f, bodyYaw, 0f);
    }
}
