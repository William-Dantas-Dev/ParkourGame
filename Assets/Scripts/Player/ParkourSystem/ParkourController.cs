using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkourController : MonoBehaviour
{
    [SerializeField] List<ParkourAction> parkourActions;
    private bool inAction;

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
        if (Input.GetButton("Jump") && !inAction)
        {
            var hitData = environmentScanner.ObstacleCheck();
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
    }

    private IEnumerator DoParkourAction(ParkourAction action)
    {
        inAction = true;
        playerController.SetControl(false);

        animator.SetBool("mirrorAction", action.Mirror);
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;

        var animState = animator.GetNextAnimatorStateInfo(0);
        if(!animState.IsName(action.AnimName))
        {
            Debug.LogError("The parkour animation is Wrong!!!");
        }
        //yield return new WaitForSeconds(animState.length);

        float timer = 0f;
        while(timer <= animState.length)
        {
            timer += Time.deltaTime;

            // Rotate the player towards the obstacles
            if (action.RotateToObstacle)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, action.TargetRotation, playerController.RotationSpeed * Time.deltaTime);
            }

            if(action.EnableTargetMatching)
            {
                MatchTarget(action);
            }

            if (animator.IsInTransition(0) && timer > 0.5f)
            {
                break;
            }

            yield return null;
        }
        yield return new WaitForSeconds(action.PostActionDelay);
        playerController.SetControl(true);
        inAction = false;
    }

    private void MatchTarget(ParkourAction action)
    {
        if (animator.isMatchingTarget) return;
        animator.MatchTarget(action.MatchPos, transform.rotation, action.MatchBodyParty,
            new MatchTargetWeightMask(action.MatchPosWeight, 0), action.MatchStartTime,
            action.MatchTargetTime);
    }
}
