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
                currentPoint = ledgeHit.transform.GetComponent<ClimbPoint>();
                playerController.SetControl(false);
                StartCoroutine(JumpToLedge("Idle To Braced Hang", ledgeHit.transform, 0.41f, 0.54f));
            }
        }
    }

    private void JumpLedgeToLedge()
    {
        float horizontal = Mathf.Round(Input.GetAxisRaw("Horizontal"));
        float vertical = Mathf.Round(Input.GetAxisRaw("Vertical"));
        var inputDir = new Vector2(horizontal, vertical);

        if (playerController.InAction || inputDir == Vector2.zero) return;

        var neighbour = currentPoint.GetNeighbour(inputDir);
        if (neighbour == null) return;

        if(neighbour.connectionType == ConnectionType.Jump && Input.GetButton("Jump"))
        {
            currentPoint = neighbour.point;
            if(neighbour.direction.y == 1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Hop Up", currentPoint.transform, 0.35f, 0.65f));
            }else if(neighbour.direction.y == -1)
            {
                StartCoroutine(JumpToLedge("Braced Hang Drop", currentPoint.transform, 0.31f, 0.65f));
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
    }

    private IEnumerator JumpToLedge(string anim, Transform ledge, float matchStartTime, float matchTargetTime)
    {
        var matchParams = new MatchTargetParams()
        {
            pos = GetHandPos(ledge),
            bodyPart = AvatarTarget.RightHand,
            startTime = matchStartTime,
            targetTime = matchTargetTime,
            posWeight = Vector3.one,
        };

        var targetRot = Quaternion.LookRotation(-ledge.forward);

        yield return playerController.DoAction(anim, matchParams, targetRot, true);
        playerController.IsHanging = true;
    }

    private Vector3 GetHandPos(Transform ledge)
    {
        return ledge.position + ledge.forward * 0.1f + Vector3.up * 0.1f - ledge.right * 0.25f;
    }

}
