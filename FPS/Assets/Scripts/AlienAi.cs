using UnityEngine;

public class AlienAi : MonoBehaviour
{
    public float radius;
    public GameObject detected;

    public Vector3 moveTarget;
    public Vector3 rotTarget;

    public float followDist;
    public float followBuffer;
    StateMachine stateMachine;

    public float headHeight;
    public IState current;
    public float targetAngle;
    // Start is called before the first frame update
    void Start()
    {
        stateMachine = new StateMachine(this);
        stateMachine.ChangeState(new Idle(this));
    }

    bool detectOBject(LayerMask layermask)
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, radius, layermask);
        foreach (Collider col in cols)
        {
            Debug.DrawLine(transform.position, col.transform.position, Color.magenta);
            detected = col.transform.root.gameObject;
            if (canSeeTarget(detected))
            {
                return true;
            }
        }
        return false;
    }

    public bool canSeeTarget(GameObject target)
    {
        int layerMask = ~LayerMask.GetMask("Alien");
        Vector3 origin = transform.position;
        origin.y += headHeight;
        RaycastHit hit;
        Vector3 localTarget = transform.InverseTransformPoint(target.transform.position);
        float angle = Mathf.Atan2(localTarget.x, localTarget.z) * Mathf.Rad2Deg;
        targetAngle = angle;
        if(angle > -90 && angle < 90)
        {
            if (Physics.Raycast(origin, target.transform.position - origin, out hit, radius + 0.1f, layerMask))
            {
                Debug.DrawLine(origin, hit.point, Color.green, .5f);
                if (hit.collider.transform.root.gameObject == target)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public interface IState
    {
        void Enter();
        void Execute();
        void Exit();
    }

    public class StateMachine
    {
        IState currentState;
        AlienAi owner;
        public StateMachine(AlienAi owned)
        {
            owner = owned;
        }

        public void ChangeState(IState newState)
        {
            if (currentState != null)
            {
                currentState.Exit();
            }
            currentState = newState;
            currentState.Enter();
        }

        public void Update()
        {
            if (currentState != null) currentState.Execute();
        }
    }

    public class Idle : IState 
    {
        private AlienAi owner;
        private float timer;
        public Idle(AlienAi owner) {this.owner = owner;}

        public void Enter()
        {
            owner.moveTarget = owner.transform.position;
            timer = 0;
        }

        public void Execute()
        {
            if (owner.detectOBject(LayerMask.GetMask("Player")))
            {
                owner.stateMachine.ChangeState(new Follow(owner, owner.detected));
            }
            else if (timer > 5f)
            {
                owner.stateMachine.ChangeState(new Wander(owner));
            }
            timer += Time.deltaTime;
        }

        public void Exit()
        {

        }
    }

    public class Follow : IState
    {
        private GameObject target;
        private Vector3 rotTarg;
        private Vector3 moveTarg;
        private AlienAi owner;
        private Vector3 LastSeen;
        public Follow(AlienAi owner, GameObject target) { this.owner = owner; this.target = target; }

        void followTarget()
        {
            float distance = Vector3.Distance(owner.transform.position, target.transform.position);
            rotTarg = target.transform.position;
            //Move Towards
            if (distance > owner.followDist + owner.followBuffer)
            {
                moveTarg = target.transform.position;
            }
            //Move Away
            else if (distance < owner.followDist)
            {
                Vector3 dir = (target.transform.position - owner.transform.position).normalized;
                moveTarg = owner.transform.position - dir;
            }
            else
            {
                moveTarg = owner.transform.position;
            }
        }

        public void Enter()
        {

        }

        public void Execute()
        {
            if (owner.canSeeTarget(target))
            {
                followTarget();
                owner.moveTarget = moveTarg;
                owner.rotTarget = rotTarg;
            }
            else
            {
                owner.stateMachine.ChangeState(new LookForTarget(owner,target));
            }
        }

        public void Exit()
        {

        }
    }

    public class LookForTarget : IState
    {
        AlienAi owner;
        GameObject target;
        float timer;
        public LookForTarget(AlienAi owner, GameObject target) { this.owner = owner; this.target = target; }

        public void Enter()
        {
            owner.moveTarget = owner.transform.position;
            timer = 0;
        }

        private void LookTowards()
        {
            owner.rotTarget = target.transform.position;
        }

        public void Execute()
        {
            LookTowards();
            if (owner.canSeeTarget(target))
            {
                owner.stateMachine.ChangeState(new Follow(owner, target));
            }
            else if (owner.detectOBject(LayerMask.GetMask("Player")))
            {
                owner.stateMachine.ChangeState(new Follow(owner, owner.detected));
            }
            else if(timer > 5)
            {
                owner.stateMachine.ChangeState(new Idle(owner));
            }
            timer += Time.deltaTime;
        }

        public void Exit()
        {

        }
    }

    public class Wander : IState
    {
        private AlienAi owner;
        private Vector3 target;
        public Wander(AlienAi owner) { this.owner = owner; }

        private void targetWander()
        {
            bool found = false;
            while (!found)
            {
                Vector3 rand = new Vector3(Random.Range(-owner.radius, owner.radius) + owner.transform.position.x, owner.transform.position.y + 0.5f, Random.Range(-owner.radius, owner.radius) + owner.transform.position.z);
                RaycastHit hit;
                if (Physics.Raycast(rand, Vector3.down, out hit, 5f, LayerMask.GetMask("Default")))
                {
                    Debug.DrawLine(rand, hit.point, Color.green, .5f);
                    target = hit.point;
                    found = true;
                }
            }
        }

        public void Enter()
        {
            targetWander();

            owner.moveTarget = target;
            owner.rotTarget = target;
        }

        public void Execute()
        {
            if(Vector3.Distance(target, owner.transform.position)< .5f)
            {
                owner.stateMachine.ChangeState(new Idle(owner));
            }
            else if (owner.detectOBject(LayerMask.GetMask("Player")))
            {
                owner.stateMachine.ChangeState(new Follow(owner, owner.detected));
            }

        }

        public void Exit()
        {
        }
    }

    // Update is called once per frame
    void Update()
    {
        stateMachine.Update();
    }


}
