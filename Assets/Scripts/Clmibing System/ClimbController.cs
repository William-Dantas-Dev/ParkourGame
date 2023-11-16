using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    private ClimbPoint currentPoint;
    private PlayerController playerController;
    private EnvironmentScanner environmentScanner;

    private void Start()
    {
        playerController = GetComponent<PlayerController>();
        environmentScanner = GetComponent<EnvironmentScanner>();
    }
    private void Update()
    {
        if (playerController.IsHanging)
        {
            JumpLedgeToLedge();
        }
        else
        {
            JumpToLedge();
        }

    }

    private void JumpToLedge()
    {
        if (Input.GetButton("Jump") && !playerController.InAction)
        {
            if (environmentScanner.ClimbLedgeCheck(transform.forward, out RaycastHit ledgeHit))
            {
                currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);
                playerController.SetControl(false);
                StartCoroutine(JumpToLedge("Idle To Braced Hang", currentPoint.transform, 0.41f, 0.54f));
            }
        }
        else
        {
            if(Input.GetButton("Drop") && !playerController.InAction)
            {
                if(environmentScanner.DropLedgeCheck(out RaycastHit ledgeHit))
                {
                    currentPoint = GetNearestClimbPoint(ledgeHit.transform, ledgeHit.point);

                    playerController.SetControl(false);
                    StartCoroutine(JumpToLedge("Drop To Hang", currentPoint.transform, 0.30f, 0.45f, handOffset: new Vector3(0.25f, 0.2f, -0.2f)));
                }
            }
        }
    }

    private void JumpLedgeToLedge()
    {
        if(Input.GetButton("Drop") && !playerController.InAction)
        {
            StartCoroutine(JumpFromHang());
            return;
        }

        float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
        float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));
        var inputDir = new Vector2(horizontal, vertical);

        if (playerController.InAction || inputDir == Vector2.zero) return;

        if(currentPoint.MountPoint && inputDir.y == 1)
        {
            StartCoroutine(MountFromHang());
            return;
        }

        var neighbour = currentPoint.GetNeighbour(inputDir);
        if (neighbour == null) return;

        if(neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
        {
            currentPoint = neighbour.point;

            if(neighbour.direction.y == 1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Hop Up", currentPoint.transform, 0.35f, 0.65f, handOffset: new Vector3(0.25f, 0.08f, 0.15f)));
            }else if(neighbour.direction.y == -1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Drop", currentPoint.transform, 0.31f, 0.65f, handOffset: new Vector3(0.25f, 0.1f, 0.13f)));
            }
            else if (neighbour.direction.x == 1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Hop Right", currentPoint.transform, 0.20f, 0.50f));
            }
            else if (neighbour.direction.x == -1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Hop Left", currentPoint.transform, 0.20f, 0.50f));
            }
        }
        else if(neighbour.connectionType == ConnectionType.Move)
        {
            currentPoint = neighbour.point;

            if(neighbour.direction.x == 1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Shimmy Right", currentPoint.transform, 0f, 0.38f, handOffset: new Vector3(0.2f, 0.05f, 0.1f)));
            }
            if (neighbour.direction.x == -1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Shimmy Left", currentPoint.transform, 0f, 0.38f, AvatarTarget.LeftHand, handOffset: new Vector3(0.2f, 0.05f, 0.1f)));
            }
        }
    }

    private IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime,
        AvatarTarget hand = AvatarTarget.RightHand, Vector3? handOffset = null)
    {
        var matchParams = new MatchTargetParams()
        {
            pos = GetHandPos(ledge, hand, handOffset),
            bodyPart = hand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one,
        };

        var targetRot = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRot, true);
        playerController.IsHanging = true;
    }

    private Vector3 GetHandPos(Transform ledge, AvatarTarget hand, Vector3? handOffset)
    {
        var offVal = (handOffset != null) ? handOffset.Value : new Vector3(0.25f, 0.1f, 0.1f);
        var hDir = (hand == AvatarTarget.RightHand) ? ledge.right : -ledge.right;
        return ledge.position + ledge.forward * offVal.z + Vector3.up * offVal.y - hDir * offVal.x;
    }

    private IEnumerator JumpFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("Jump From Wall");

        playerController.RestTargetRotation();
        playerController.SetControl(true);
    }

    private IEnumerator MountFromHang()
    {
        playerController.IsHanging = false;
        yield return playerController.DoAction("Braced Hang To Crouch");

        playerController.EnableCharacterController(true);

        yield return new WaitForSeconds(0.5f);
        playerController.RestTargetRotation();
        playerController.SetControl(true);
    }

    private ClimbPoint GetNearestClimbPoint(Transform ledge, Vector3 hitPoint)
    {
        var points = ledge.GetComponentsInChildren<ClimbPoint>();

        ClimbPoint nearestPoint = null;
        float nearestPointDistance = Mathf.Infinity;

        foreach(var point in points)
        {
            float distance = Vector3.Distance(point.transform.position, hitPoint);
            if(distance < nearestPointDistance)
            {
                nearestPoint = point;
                nearestPointDistance = distance;
            }
        }

        return nearestPoint;
    }
}
