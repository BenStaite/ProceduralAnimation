using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootIK : MonoBehaviour
{

    public Animator anim;
    public LayerMask layerMask;
    public float DistanceToGround;
    public float Zoffset;

    public Collider LeftFootCollider;
    public Collider RightFootCollider;
    public Collider LeftToeCollider;
    public Collider RightToeCollider;

    public float footUpSpeed;
    public float footDownSpeed;
    public float footLockSpeed;
    public float rotationSpeed;

    public float Pivot;

    public bool LeftLock;
    public bool RightLock;

    public float StepHeight;

    public Vector3 rightLockPosition;
    public Vector3 leftLockPosition;

    public Quaternion rightLockRotation;
    public Quaternion leftLockRotation;

    private Vector3 LastPelvisPosition;

    public float leftWeight;
    public float rightWeight;

    public Vector3 Direction;

    public float pelvicOffset;
    public float pelvicSpeed;

    public bool stabalize;

    public float stepThresh;
    public float stepZ;

    public float averageStepTime;
    public float currentLeftTime, currentRightTime;

    public bool rightDown, leftDown;

    public float rightHeightTarget, leftHeightTarget;

    public bool adjustingRightHeight, adjustingLeftHeight;

    public float rightStartHeight, leftStartHeight;

    // Start is called before the first frame update
    void Start()
    {
        anim.SetLayerWeight(0, .5f);
        rightLockPosition = Vector3.zero;
        rightLockRotation = new Quaternion();
        leftLockPosition = Vector3.zero;
        leftLockRotation = new Quaternion();
        LeftLock = false;
        RightLock = false;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (!controller.isGrounded)
        {
            timeFalling += Time.deltaTime;
        }
        else
        {
            timeFalling = 0;
        }

        if(timeFalling > 0.1f)
        {
            anim.SetBool("Falling", true);
        }
        else
        {
            anim.SetBool("Falling", false);
        }
        */
    }

    private bool LockLeft()
    {
        RaycastHit foothit;
        Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * 0.5f, Vector3.down);

        if (Physics.Raycast(ray, out foothit, 10f, layerMask))
        {
            Debug.DrawLine(anim.GetIKPosition(AvatarIKGoal.LeftFoot) + Vector3.up * 0.5f, foothit.point, Color.red);
        }
        else
        {
            LeftLock = false;
            return false;
        }

        Vector3 footPosition;
        Quaternion footRotation;

        footPosition = foothit.point;
        footRotation = Quaternion.FromToRotation(Vector3.up, foothit.normal)*anim.bodyRotation;
        footPosition.y += DistanceToGround;

        Quaternion slerped = Quaternion.Slerp(anim.GetIKRotation(AvatarIKGoal.LeftFoot), footRotation, rotationSpeed);
        float footspeed = (anim.GetIKPosition(AvatarIKGoal.LeftFoot).y > footPosition.y) ? footDownSpeed : footUpSpeed;
        Vector3 lerped = Vector3.Lerp(anim.GetIKPosition(AvatarIKGoal.LeftFoot), footPosition, footspeed * Time.deltaTime);

        anim.SetIKPosition(AvatarIKGoal.LeftFoot, lerped);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, slerped);
        leftLockRotation = footRotation;
        leftLockPosition = footPosition;
        LeftLock = true;
        return true;
    }

    private bool LockRight()
    {
        RaycastHit foothit;
        Ray ray = new Ray(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * 0.5f, Vector3.down);

        if (Physics.Raycast(ray, out foothit, 10f, layerMask))
        {
            Debug.DrawLine(anim.GetIKPosition(AvatarIKGoal.RightFoot) + Vector3.up * 0.5f, foothit.point, Color.red);
        }
        else
        {
            RightLock = false;
            return false;
        }

        Vector3 footPosition;
        Quaternion footRotation;

        footPosition = foothit.point;
        footRotation = Quaternion.FromToRotation(Vector3.up, foothit.normal) * anim.bodyRotation;
        footPosition.y += DistanceToGround;
        
        Quaternion slerped = Quaternion.Slerp(anim.GetIKRotation(AvatarIKGoal.RightFoot), footRotation, rotationSpeed);
        float footspeed = (anim.GetIKPosition(AvatarIKGoal.RightFoot).y > footPosition.y) ? footDownSpeed : footUpSpeed;
        Vector3 lerped = Vector3.Lerp(anim.GetIKPosition(AvatarIKGoal.RightFoot), footPosition, footspeed * Time.deltaTime);

        anim.SetIKPosition(AvatarIKGoal.RightFoot, lerped);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, slerped);
        rightLockRotation = footRotation;
        rightLockPosition = footPosition;
        RightLock = true;
        return true;
    }


    private bool detectStep(AvatarIKGoal foot, bool left)
    {
        float time;
        if (left)
        {
            time = currentLeftTime;
        }
        else
        {
            time = currentRightTime;
        }

        Vector3 dir = Direction.normalized * (1-(time/averageStepTime)) * stepZ ; 

        RaycastHit hit;
        Ray ray = new Ray(anim.GetIKPosition(foot) + Vector3.up * 0.5f + dir, Vector3.down);


        if(Physics.Raycast(ray, out hit, 10f, layerMask))
        {
            Debug.DrawLine(anim.GetIKPosition(foot) + Vector3.up * 0.5f + dir, hit.point, Color.red);
        }
        else
        {
            return false;
        }

        if(Mathf.Abs(hit.point.y - anim.GetIKPosition(foot).y)>stepThresh)
        {
            if (left)
            {
                leftHeightTarget = hit.point.y + DistanceToGround;
            }
            else
            {
                rightHeightTarget = hit.point.y + DistanceToGround;
            }
            return true;
        }
        else
        {
            return false;
        }
    }

    private void moveFootHeight(AvatarIKGoal foot, float height, float currentTime, float start)
    {
        Vector3 footpos = anim.GetIKPosition(foot);
        if (currentTime > averageStepTime) { currentTime = averageStepTime; }
        if (height > anim.GetIKPosition(foot).y)
        {
            float lerp1 = Mathf.Lerp(start, StepHeight + height, (currentTime / averageStepTime));
            float lerp2 = Mathf.Lerp(StepHeight + height, height, (currentTime / averageStepTime));
            footpos.y = Mathf.Lerp(lerp1,lerp2,(currentTime/averageStepTime));
            anim.SetIKPosition(foot, footpos);
        }
        else if(height < anim.GetIKPosition(foot).y)
        {
            float lerp1 = Mathf.Lerp(start, StepHeight + start, (currentTime / averageStepTime));
            float lerp2 = Mathf.Lerp(StepHeight + start, height, (currentTime / averageStepTime));
            footpos.y = Mathf.Lerp(lerp1, lerp2, (currentTime / averageStepTime));
            anim.SetIKPosition(foot, footpos);
        }

    }
    private void KeepRightLocked()
    {
        Vector3 lerped = Vector3.Lerp(anim.GetIKPosition(AvatarIKGoal.RightFoot), rightLockPosition, footLockSpeed * Time.deltaTime);
        Quaternion slerped = Quaternion.Slerp(RightFootCollider.transform.rotation, rightLockRotation, rotationSpeed);
        anim.SetIKPosition(AvatarIKGoal.RightFoot, lerped);
        anim.SetIKRotation(AvatarIKGoal.RightFoot, slerped);

        Debug.DrawLine(anim.GetIKPosition(AvatarIKGoal.RightFoot), rightLockPosition,Color.cyan);
    }
    private void KeepLeftLocked()
    {
        Vector3 lerped = Vector3.Lerp(anim.GetIKPosition(AvatarIKGoal.LeftFoot), leftLockPosition, footLockSpeed * Time.deltaTime);
        Quaternion slerped = Quaternion.Slerp(LeftFootCollider.transform.rotation, leftLockRotation, rotationSpeed);
        anim.SetIKPosition(AvatarIKGoal.LeftFoot, lerped);
        anim.SetIKRotation(AvatarIKGoal.LeftFoot, slerped);
    }

    private void UnlockLeft()
    {
        LeftLock = false;
        leftLockPosition = Vector3.zero;
    }

    private void UnlockRight()
    {
        RightLock = false;
        rightLockPosition = Vector3.zero;
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

        Vector3 newPelvis = transform.position + Vector3.up * (pelvicOffset-totalOffset);
        newPelvis.y = Mathf.Lerp(transform.position.y, newPelvis.y, pelvicSpeed*Time.deltaTime);
        transform.position = newPelvis;
    }



    private void OnAnimatorIK(int layerIndex)
    {
        anim.stabilizeFeet = stabalize;
        // FOOT PLACEMENT
        Pivot = anim.pivotWeight;

        if(Pivot < .75f){leftWeight = .5f * Mathf.Cos(12.5f * Pivot) + .5f;}
        else {leftWeight = 0;}

        if (Pivot > .25f){rightWeight = .5f * Mathf.Cos(12.5f * Pivot) + .5f;}
        else {rightWeight = 0;}

        anim.SetFloat("PivotLeft", leftWeight);
        anim.SetFloat("PivotRight", rightWeight);


        

        float X = anim.GetFloat("X");
        float Y = anim.GetFloat("Y");
        Direction = (transform.right * X + transform.forward * Y).normalized;

        anim.SetFloat("RightMulti", 1);
        anim.SetFloat("LeftMulti", 1);

        

        if (anim.GetBool("Falling"))
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, 0);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0);
        }
        else
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, rightWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, rightWeight);
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, leftWeight);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, leftWeight);


            if (rightWeight == 0f)
            {
                if (rightDown)
                {
                    rightStartHeight = anim.GetIKPosition(AvatarIKGoal.RightFoot).y;
                    rightDown = false;
                }
                
                currentRightTime += Time.deltaTime;
                if (adjustingRightHeight)
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                    moveFootHeight(AvatarIKGoal.RightFoot, rightHeightTarget, currentRightTime, rightStartHeight);
                }
                else if (detectStep(AvatarIKGoal.RightFoot, false))
                {
                    adjustingRightHeight = true;
                    anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);
                    moveFootHeight(AvatarIKGoal.RightFoot, rightHeightTarget, currentRightTime, rightStartHeight);
                }
                else
                {
                    LockRight();
                }
                UnlockRight();
            }
            else
            {
                if (!rightDown)
                {
                    adjustingRightHeight = false;
                    averageStepTime = (averageStepTime + currentRightTime) / 2f;
                    currentRightTime = 0;
                    rightDown = true;
                }

                if (RightLock)
                {
                    KeepRightLocked();
                }
                else
                {
                    LockRight();
                }
            }


            if(leftWeight == 0f)
            {
                if (leftDown)
                {
                    leftStartHeight = anim.GetIKPosition(AvatarIKGoal.LeftFoot).y;
                    leftDown = false;
                }
                
                currentLeftTime += Time.deltaTime;
                if (adjustingLeftHeight)
                {
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    moveFootHeight(AvatarIKGoal.LeftFoot, leftHeightTarget, currentLeftTime, leftStartHeight);
                }
                else if (detectStep(AvatarIKGoal.LeftFoot, true))
                {
                    adjustingLeftHeight = true;
                    anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);
                    moveFootHeight(AvatarIKGoal.LeftFoot, leftHeightTarget, currentLeftTime, leftStartHeight);
                }
                else
                {
                    LockLeft();
                }
                UnlockLeft();
            }
            else
            {
                if (!leftDown)
                {
                    adjustingLeftHeight = false;
                    averageStepTime = (averageStepTime + currentLeftTime) / 2f;
                    currentLeftTime = 0;
                    leftDown = true;
                }

                if (LeftLock)
                {
                    KeepLeftLocked();
                }
                else
                {
                    LockLeft();
                }
            }
            setHips();
        }
    }
}
