using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] private List<ParkourAction> parkourActions;
    [SerializeField] private ParkourAction jumpDownAction;
    [SerializeField] private float autoDropHeightLimit = 1f;

    private EnvironmentScanner environmentScanner;
    private Animator animator;
    private PlayerController playerController;

    private void Awake()
    {
        environmentScanner = GetComponent<EnvironmentScanner>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
    }

    private void Update()
    {
        var hitData = environmentScanner.ObstacleCheck();
        if (Input.GetButton("Jump") && !playerController.InAction)
        {
            if (hitData.forwardHitFound)
            {
                foreach(var action in parkourActions)
                {
                    if (action.CheckIfPossible(hitData, transform))
                    {
                        StartCoroutine(DoParkourAction(action));
                        break;
                    }
                }
                //Debug.Log("Obstacle Found: " + hitData.forwardHit.transform.name);
            }
        }

        if (playerController.IsOnLedge && !playerController.InAction && !hitData.forwardHitFound)
        {
            bool shouldJump = true;
            Debug.Log(playerController.LedgeData.height);
            if(playerController.LedgeData.height > autoDropHeightLimit && !Input.GetButton("Jump"))
            {
                shouldJump = false;
            }

            if(shouldJump && playerController.LedgeData.angle <= 50)
            {
                playerController.IsOnLedge = false;
                StartCoroutine(DoParkourAction(jumpDownAction));
            }
        }
    }

    private IEnumerator DoParkourAction(ParkourAction action)
    {
        playerController.SetControl(false);

        MatchTargetParams matchParam = null;
        if (action.EnableTargetMatching)
        {
            matchParam = new MatchTargetParams()
            {
                pos = action.MatchPos,
                bodyPart = action.MatchBodyParty,
                posWeight = action.MatchPosWeight,
                startTime = action.MatchStartTime,
                targetTime = action.MatchTargetTime,
            };
        }
        yield return playerController.DoAction(action.AnimName, matchParam, action.TargetRotation,
            action.RotateToObstacle, action.PostActionDelay, action.Mirror);

        playerController.SetControl(true);
    }
}
