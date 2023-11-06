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
        animator.CrossFade(action.AnimName, 0.2f);
        yield return null;
        var animState = animator.GetCurrentAnimatorStateInfo(0);
        if(!animState.IsName(action.AnimName))
        {
            Debug.LogError("The parkour animation is Wrong!!!");
        }
        yield return new WaitForSeconds(animState.length);
        playerController.SetControl(true);
        inAction = false;
    }
}
