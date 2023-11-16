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
    private bool hasControl = true;
    public bool InAction { get; private set; }
    public bool IsHanging { get; set; }
    public bool IsOnLedge {  get; set; }
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
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float moveAmount = Mathf.Clamp01(Mathf.Abs(horizontal) + Mathf.Abs(vertical));

        var moveInput = (new Vector3(horizontal, 0f, vertical)).normalized;
        desiredMoveDir = cameraController.PlanarRotation * moveInput;
        moveDir = desiredMoveDir;

        if (!hasControl || IsHanging) return;

        velocity = Vector3.zero;

        GroundCheck();

        if (isGrounded)
        {
            ySpeed = -0.5f;
            velocity = desiredMoveDir * moveSpeed;

            IsOnLedge = environmentScanner.ObstacleLedgeCheck(desiredMoveDir, out LedgeData ledgeData);
            if(IsOnLedge)
            {
                LedgeData = ledgeData;
                LedgeMovement();
            }
            animator.SetFloat("moveAmount", velocity.magnitude / moveSpeed, 0.2f, Time.deltaTime);
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
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius, groundLayer);
        animator.SetBool("isGrounded", isGrounded);
    }

    private void LedgeMovement()
    {
        float signedAngle = Vector3.SignedAngle(LedgeData.surfaceHit.normal, desiredMoveDir, Vector3.up);
        float angle = Mathf.Abs(signedAngle);

        if(Vector3.Angle(desiredMoveDir, transform.forward) >= 80)
        {
            // Don't move, but rotate
            velocity = Vector3.zero;
            return;
        }
        
        if(angle < 60)
        {
            velocity = Vector3.zero;
            moveDir = Vector3.zero;
        }else if(angle < 90)
        {
            // Angle is b/w 60 and 90, so limit the velocity to horizontal direction
            var left = Vector3.Cross(Vector3.up, LedgeData.surfaceHit.normal);
            var dir = left * Mathf.Sign(signedAngle);

            velocity = velocity.magnitude * dir;
            moveDir = dir;
        }
    }

    public IEnumerator DoAction(string animName, MatchTargetParams matchParams = null, Quaternion targetRotation = new Quaternion(),
        bool rotate = false, float postDelay = 0f, bool mirror = false)
    {
        InAction = true;

        animator.SetBool("mirrorAction", mirror);
        animator.CrossFadeInFixedTime(animName, 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if (!animState.IsName(animName)) Debug.LogError("The parkour animation is Wrong!!!");

        float rotateStartTime = (matchParams != null) ? matchParams.startTime : 0f;

        float timer = 0f;
        while (timer <= animState.length)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / animState.length;


            if (rotate && normalizedTime > rotateStartTime) transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (matchParams != null) MatchTarget(matchParams);

            if (animator.IsInTransition(0) && timer > 0.5f) break;

            yield return null;
        }
        yield return new WaitForSeconds(postDelay);

        InAction = false;
    }

    private void MatchTarget(MatchTargetParams matchTarget)
    {
        if (animator.isMatchingTarget) return;
        animator.MatchTarget(matchTarget.pos, transform.rotation, matchTarget.bodyPart, new MatchTargetWeightMask(matchTarget.posWeight, 0),
            matchTarget.startTime, matchTarget.targetTime);
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

    public void EnableCharacterController(bool enabled)
    {
        characterController.enabled = enabled;
    }

    public void RestTargetRotation()
    {
        targetRotation = transform.rotation;
    }

    public bool HasControl
    {
        get => hasControl;
        set => hasControl = value;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(groundCheckOffset), groundCheckRadius);
    }

    public float RotationSpeed => rotationSpeed;
}

public class MatchTargetParams
{
    public Vector3 pos;
    public AvatarTarget bodyPart;
    public Vector3 posWeight;
    public float startTime;
    public float targetTime;
}