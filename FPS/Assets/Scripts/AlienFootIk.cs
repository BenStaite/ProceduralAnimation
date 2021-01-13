using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienFootIk : MonoBehaviour
{
    public GameObject LeftTarget, RightTarget, Head;
    public Animator anim;

    public float threshHold;
    public float DistanceToGround;

    public float stepTime, stepHeight, footSpeed, overshoot;
    private float RightTimeElapsed, LeftTimeElapsed, RightDistance, LeftDistance;


    private bool LeftMoving;
    private bool RightMoving;
    public LayerMask layerMask;

    private Vector3 LeftCurrentTarget, RightCurrentTarget;
    private Vector3 LeftOrigin, RightOrigin;

    private Quaternion LeftTargetRotation, RightTargetRotation;
    public bool stabalise;

    private Vector3 lastLeftPos, lastRightPos; 

    public float pelvicOffset, pelvicSpeed;
    // Start is called before the first frame update
    void Start()
    {
        LeftMoving = false; RightMoving = false;
        LeftOrigin = anim.GetIKPosition(AvatarIKGoal.LeftFoot); RightOrigin = anim.GetIKPosition(AvatarIKGoal.RightFoot);
        lastLeftPos = LeftOrigin; lastRightPos = RightOrigin;
    }

    void SetLeftTarget()
    {
        RaycastHit hit;
        Vector3 direction = LeftTarget.transform.position - Head.transform.position;
        Ray ray = new Ray(Head.transform.position, direction.normalized);
        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            Vector3 Target = hit.point;
            Target.y += DistanceToGround;
            LeftTarget.transform.Translate(new Vector3(0, Target.y - LeftTarget.transform.position.y, 0));
            LeftTargetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * anim.bodyRotation;
        }
    }

    void SetRightTarget()
    {
        RaycastHit hit;
        Vector3 direction = RightTarget.transform.position - Head.transform.position;
        Ray ray = new Ray(Head.transform.position, direction.normalized);
        if (Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            Vector3 Target = hit.point;
            Target.y += DistanceToGround;
            RightTarget.transform.Translate(new Vector3 (0, Target.y - RightTarget.transform.position.y, 0));
            RightTargetRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * anim.bodyRotation;
        }
    }

    void MoveRightToTarget()
    {
        Vector3 overshotTarget = RightTarget.transform.position ;
        if (RightTimeElapsed>stepTime)
        {
            RightTimeElapsed = 0;
            RightMoving = false;
            LockRightFoot();
            return;
        }
        RightTimeElapsed += Time.deltaTime;
        Vector3 midPoint = (RightTarget.transform.position + RightOrigin) / 2f;
        midPoint.y += stepHeight;
        Vector3 firstLerp = Vector3.Lerp(RightOrigin, midPoint, RightTimeElapsed / stepTime);
        Vector3 secondLerp = Vector3.Lerp(midPoint, overshotTarget, RightTimeElapsed / stepTime);
        Vector3 lerped = Vector3.Lerp(firstLerp, secondLerp, RightTimeElapsed / stepTime);
        Quaternion slerped = Quaternion.Slerp(anim.GetIKRotation(AvatarIKGoal.RightFoot), RightTargetRotation, footSpeed * Time.deltaTime);
        RightCurrentTarget = lerped;
        anim.SetIKPosition(AvatarIKGoal.RightFoot, lerped);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, slerped);
    }


    void MoveLeftToTarget()
    {
        Vector3 overshotTarget = LeftTarget.transform.position;
        if(LeftTimeElapsed > stepTime)
        {
            LeftTimeElapsed = 0;
            LeftMoving = false;
            LockLeftFoot();
            return;
        }
        LeftTimeElapsed += Time.deltaTime;
        Vector3 midPoint = (LeftTarget.transform.position + LeftOrigin)/ 2f;
        midPoint.y += stepHeight;
        Vector3 firstLerp = Vector3.Lerp(LeftOrigin, midPoint, LeftTimeElapsed/stepTime);
        Vector3 secondLerp = Vector3.Lerp(midPoint, overshotTarget, LeftTimeElapsed / stepTime);
        Vector3 lerped = Vector3.Lerp(firstLerp, secondLerp, LeftTimeElapsed / stepTime);
        Quaternion slerped = Quaternion.Slerp(anim.GetIKRotation(AvatarIKGoal.LeftFoot), LeftTargetRotation, footSpeed * Time.deltaTime);
        LeftCurrentTarget = lerped;
        anim.SetIKPosition(AvatarIKGoal.LeftFoot, lerped);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, slerped);
    }

    bool RightFootShouldMove()
    {
        RightDistance = Vector3.Distance(RightCurrentTarget, RightTarget.transform.position);
        if (RightDistance>threshHold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool LeftFootShouldMove()
    {
        LeftDistance = Vector3.Distance(LeftCurrentTarget, LeftTarget.transform.position);
        if (LeftDistance > threshHold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    private bool LockRightFoot()
    {
        if (RightCurrentTarget != Vector3.zero)
        {
            anim.SetIKPosition(AvatarIKGoal.RightFoot, RightCurrentTarget);
            anim.SetIKRotation(AvatarIKGoal.RightFoot, RightTargetRotation);
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool LockLeftFoot()
    {
        if(LeftCurrentTarget != Vector3.zero)
        {
            anim.SetIKPosition(AvatarIKGoal.LeftFoot, LeftCurrentTarget);
            anim.SetIKRotation(AvatarIKGoal.LeftFoot, LeftTargetRotation);
            return true;
        }
        else
        {
            return false;
        }
    }

    private void setHips()
    {
        float leftOffsetPos = transform.position.y - anim.GetIKPosition(AvatarIKGoal.LeftFoot).y;
        float rightOffsetPos = transform.position.y - anim.GetIKPosition(AvatarIKGoal.RightFoot).y;
        float totalOffset;
        if (leftOffsetPos < rightOffsetPos)
        {
            totalOffset = rightOffsetPos;
        }
        else
        {
            totalOffset = leftOffsetPos;
        }

        Vector3 newPelvis = anim.bodyPosition + Vector3.up * (pelvicOffset - totalOffset);
        newPelvis.y = Mathf.Lerp(anim.bodyPosition.y, newPelvis.y, pelvicSpeed * Time.deltaTime);
        anim.bodyPosition = newPelvis;
    }


    // Update is called once per frame
    private void OnAnimatorIK(int layerIndex)
    {
        anim.stabilizeFeet = stabalise;

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);

        anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

        SetRightTarget();
        SetLeftTarget();

        if (RightMoving && !LeftMoving)
        {
            MoveRightToTarget();
        }
        else if (RightFootShouldMove() && !LeftMoving)
        {
            RightOrigin = RightCurrentTarget;
            RightMoving = true;
            MoveRightToTarget();
        }
        else
        {
            LockRightFoot();
        }

        if (LeftMoving && !RightMoving)
        {
            MoveLeftToTarget();
        }
        else if (LeftFootShouldMove()&& !RightMoving)
        {
            LeftOrigin = LeftCurrentTarget;
            LeftMoving = true;
            MoveLeftToTarget();
        }
        else
        {
            LockLeftFoot();
        }
        lastLeftPos = anim.GetIKPosition(AvatarIKGoal.LeftFoot);
        lastRightPos = anim.GetIKPosition(AvatarIKGoal.RightFoot);

    }
}
