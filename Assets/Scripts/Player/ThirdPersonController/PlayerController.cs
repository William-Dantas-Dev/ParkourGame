using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    CameraController cameraController;
    private Animator animator;
    private CharacterController characterController;
    private EnvironmentScanner environmentScanner;


    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 5f;
    public bool isOnLedge {  get; set; }
    public LedgeData LedgeData { get; set; }
    private Vector3 desiredMoveDir;
    private Vector3 moveDir;
    private Vector3 velocity;

    [Header("Gravity Settings")]
    private float ySpeed;

    [Header("Ground Check Settings")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private Vector3 groundCheckOffset;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded;

    private bool hasControl = true;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 500f;
    private Quaternion targetRotation;

    private void Awake()
    {
        cameraController = Camera.main.GetComponent<CameraController>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    private void Update()
    {
        MovePlayer();
    }

    private void MovePlayer()
    {
        if (!hasControl) return;
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

        var moveInput = (new Vector3(horizontal, 0f, vertical)).normalized;
        desiredMoveDir = cameraController.PlanarRotation * moveInput;
        moveDir = desiredMoveDir;
        velocity = Vector3.zero;

        GroundCheck();

        if (isGrounded)
        {
            ySpeed = -0.5f;
            velocity = desiredMoveDir * moveSpeed;
            isOnLedge = environmentScanner.LedgeCheck(desiredMoveDir, out LedgeData ledgeData);
            if(isOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
                Debug.Log("On Ledge");
            }
        }
        else
        {
            ySpeed += Physics.gravity.y * Time.deltaTime;
            velocity = transform.forward * moveSpeed;
        }

        
        velocity.y = ySpeed;

        characterController.Move(velocity * Time.deltaTime);
        if (moveAmount > 0 && moveDir.magnitude > 0.2f)
        {
            //transform.position += moveDir * moveSpeed * Time.deltaTime;
            targetRotation = Quaternion.LookRotation(moveDir);
        }

        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);

    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
    }

    private void LedgeMovement()
    {
        float angle = Vector3.Angle(LedgeData.surfaceHit.normal, desiredMoveDir);
        if(angle < 90)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }
    }

    public void SetControl(bool hasControl)
    {
        this.hasControl = hasControl;
        characterController.enabled = hasControl;

        if(!hasControl)
        {
            animator.SetFloat("moveAmount", 0f);
            targetRotation = transform.rotation;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
}
