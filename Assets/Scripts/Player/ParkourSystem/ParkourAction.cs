using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Parkour System/New Parkour Action")]
public class ParkourAction : ScriptableObject
{
    [SerializeField] private string animName;
    [SerializeField] private string obstacleTag;

    [SerializeField] private float minHeight;
    [SerializeField] private float maxHeight;
    [SerializeField] private bool rotateToObstacle;
    [SerializeField] private float postActionDelay;

    [Header("Target Matching")]
    [SerializeField] private bool enableTargetMatching = true;
    [SerializeField] protected AvatarTarget matchBodyPart;
    [SerializeField] private float matchStartTime;
    [SerializeField] private float matchTargetTime;
    [SerializeField] private Vector3 matchPosWeight = new Vector3(0, 1, 0);
    public Quaternion TargetRotation { get; set; }
    public Vector3 MatchPos { get; set; }
    public bool Mirror { get; set; }

    public virtual  bool CheckIfPossible(ObstacleHitData hitData, Transform player)
    {
        // Check Tag
        if (!string.IsNullOrEmpty(obstacleTag) && hitData.forwardHit.transform.tag != obstacleTag)
        {
            return false;
        }

        // Height Tag
        float height = hitData.heightHit.point.y - player.position.y;
        if(height < minHeight || height > maxHeight)
        {
            return false;
        }

        if (rotateToObstacle)
        {
            TargetRotation = Quaternion.LookRotation(-hitData.forwardHit.normal);
        }

        if(enableTargetMatching)
        {
            MatchPos = hitData.heightHit.point;
        }

        return true;
        //return height >= minHeight && height <= maxHeight;
    }

    public string AnimName => animName;

    public bool RotateToObstacle => rotateToObstacle;
    public float PostActionDelay => postActionDelay;

    public bool EnableTargetMatching => enableTargetMatching;
    public AvatarTarget MatchBodyParty => matchBodyPart;
    public float MatchStartTime => matchStartTime;
    public float MatchTargetTime => matchTargetTime;
    public Vector3 MatchPosWeight => matchPosWeight;
}
