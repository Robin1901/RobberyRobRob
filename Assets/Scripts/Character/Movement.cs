using System.Diagnostics;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Movement : MonoBehaviour
{
    [SerializeField] private Transform headPivot;

    private float walkSpeed = 4.5f;
    private float sprintSpeed = 7f;
    private float crouchSpeed = 2f;

    public bool isSprinting { get; private set; }
    public bool isCrouching { get; private set; }

    private float currentSpeed = 0f;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 forward = headPivot.forward;
        Vector3 right = headPivot.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 rawMove = right * x + forward * z;

        if (rawMove.sqrMagnitude > 1f) rawMove = rawMove.normalized;

        bool isMoving = rawMove != Vector3.zero;

        isSprinting = isMoving && Input.GetKey(KeyCode.LeftShift);
        isCrouching = isMoving && Input.GetKey(KeyCode.LeftControl);

        float targetSpeed;

        if (isSprinting && !isCrouching)
            targetSpeed = sprintSpeed;
        else if (isCrouching && !isSprinting)
            targetSpeed = crouchSpeed;
        else
            targetSpeed = walkSpeed;

        currentSpeed = isMoving ? targetSpeed : 0f;

        controller.Move(rawMove * currentSpeed * Time.deltaTime);
    }
}
