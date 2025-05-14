using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LegMovement : MonoBehaviour
{
    [Header("References")]
    public Transform leftLeg;            // Pivot-Empty des linken Beins
    public Transform rightLeg;           // Pivot-Empty des rechten Beins
    public CharacterController controller;
    public Movement movementScript;      // Referenz auf Movement.cs

    [Header("Leg Swing Settings")]
    public float walkSwingAmount = 12f; // Max. Rotationswinkel beim Gehen
    public float sprintSwingAmount = 20f; // beim Sprinten
    public float crouchSwingAmount = 8f;  // beim Ducken

    public float walkSwingSpeed = 40f;   // Schwunggeschwindigkeit beim Gehen
    public float sprintSwingSpeed = 50f;  // beim Sprinten
    public float crouchSwingSpeed = 30f;  // beim Ducken

    private float swingTime = 0f;

    void Update()
    {
        // Bewegungsgeschwindigkeit (horizontal)
        Vector3 horizVel = new Vector3(controller.velocity.x, 0f, controller.velocity.z);
        bool isMoving = horizVel.magnitude > 0.1f;

        // Zustand aus Movement-Skript abfragen
        bool sprint = movementScript.isSprinting;
        bool crouch = movementScript.isCrouching;

        // Aktuelle Swing-Parameter wählen
        float swingAmount = walkSwingAmount;
        float swingSpeed = walkSwingSpeed;

        if (sprint && !crouch)
        {
            swingAmount = sprintSwingAmount;
            swingSpeed = sprintSwingSpeed;
        }
        else if (crouch && !sprint)
        {
            swingAmount = crouchSwingAmount;
            swingSpeed = crouchSwingSpeed;
        }

        // Beine animieren
        if (isMoving)
        {
            swingTime += Time.deltaTime * swingSpeed;
            float angle = Mathf.Sin(swingTime) * swingAmount;
            leftLeg.localRotation = Quaternion.Euler(angle, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(-angle, 0f, 0f);
        }
        else
        {
            // Rückkehr zur Neutralstellung
            leftLeg.localRotation = Quaternion.Lerp(leftLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
            rightLeg.localRotation = Quaternion.Lerp(rightLeg.localRotation, Quaternion.identity, Time.deltaTime * 10f);
        }
    }
}
