using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClimbController : MonoBehaviour
{
    PlayerController playerController;
    EnvironmentScanner environmentScanner;

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
                playerController.SetControl(false);
                StartCoroutine(JumpToLedge("Idle To Braced Hang", ledgeHit.transform, 0.41f, 0.54f));
            }
        }
    }

    private void JumpLedgeToLedge()
    {

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
