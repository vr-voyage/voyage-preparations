
using UdonSharp;
using UnityEngine;
using UnityEngine.AI;
using VRC.SDKBase;
using VRC.Udon;

public class UdonNavigation : UdonSharpBehaviour
{
    public Transform[] goals;
    public NavMeshAgent agent;

    public float minSpeed = 0.5f;
    public float maxSpeed = 3.5f;

    public float minimumWaitingTime = 0.5f;
    public float maximumWaitingTime = 10;

    public Animator agentAnimator;
    public string animatorWaitingParameterName = "waiting";

    public bool waiting = false;
    public float waitingTime = 0;
    public int currentGoal = 0;

    public float currentDistance = 0;

    void Start()
    {
        if ((agent == null) | (goals.Length == 0))
        {
            Debug.LogError($"{gameObject.name} UdonNavigation : Either the Agent or the path isn't set !");
            Debug.LogError("Engaging Self Destruct sequence !");
            Destroy(this);
        }

        currentGoal = 0;
        waitingTime = 0;
        RandomGoal();
        StartGoing(RandomSpeed());
    }

    float RandomSpeed()
    {
        return Random.Range(minSpeed, maxSpeed);
    }

    void StartGoing(float speed)
    {
        agent.isStopped = false;
        agent.speed = speed;
        agent.destination = goals[currentGoal].position;        
    }

    void NextGoal()
    {
        currentGoal += 1;
        if (currentGoal >= goals.Length)
        {
            currentGoal = 0;
        }
    }

    void PreviousGoal()
    {
        currentGoal -= 1;
        if (currentGoal < 0)
        {
            currentGoal = goals.Length - 1;
        }
    }

    void RandomGoal()
    {
        /* Just use Previous/Next in this case... */
        if (goals.Length < 3) return;
        int newGoal = currentGoal;
        while (newGoal == currentGoal)
        {
            newGoal = Random.Range((int)0, goals.Length);
        }
        currentGoal = newGoal;
        
    }

    void PropagateWaitToAnimator()
    {
        if ((agentAnimator != null) & (animatorWaitingParameterName != null))
        {
            agentAnimator.SetBool(animatorWaitingParameterName, waiting);
        }
    }

    private void Update()
    {
        /* FIXME
         * I don't know why agent.isStopped isn't set automatically
         * based on the Stop Distance.
         * The agent just won't stop automatically.
         * So... we'll do it by hand for the time being.
         * This is stupid though.
         */
        if ((agent.isStopped == false) & (agent.remainingDistance > agent.stoppingDistance))
        {
            //currentDistance = agent.remainingDistance;
            return;
        }

        if (!waiting)
        {

            waiting = true;
            agent.isStopped = true; // Stupid. The Agent should do it automatically.
            waitingTime = Random.Range(minimumWaitingTime, maximumWaitingTime);
            PropagateWaitToAnimator();
            Debug.Log("[UdonNavigation] Waiting !");
            return;
        }

        waitingTime -= Time.deltaTime;
        if (waitingTime > 0)
        {
            return;
        }

        waiting = false;
        PropagateWaitToAnimator();
        RandomGoal();
        StartGoing(RandomSpeed());
        Debug.Log($"[UdonNavigation] Moving towards Goal {currentGoal} !");

    }

}
