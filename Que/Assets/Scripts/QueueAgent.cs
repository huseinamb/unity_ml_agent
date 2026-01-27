using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class QueueAgent : Agent
{
    public float moveSpeed = 2f;
    public float turnSpeed = 2f;
    public Transform goal;

    Rigidbody rb;
    AgentRaycaster raycaster;

    float previousDistanceToGoal;
    float maxGoalDistance;
    public CubeMover doorMover;

    private static int globalEpisodeCount = 0;


    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        raycaster = GetComponent<AgentRaycaster>();
    }

    public override void OnEpisodeBegin()
    {
        //track episode

        globalEpisodeCount++;
        Debug.Log($"Episode {globalEpisodeCount} started");

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        float x = Random.Range(-12f, 12f);
        float z = Random.Range(-12f, 12f);

        transform.position = new Vector3(x, 0.5f, z);

        //transform.position = new Vector3(0, 0.5f, -5);
        transform.rotation = Quaternion.identity;

        previousDistanceToGoal = Vector3.Distance(transform.position, goal.position);
        maxGoalDistance = Vector3.Distance(transform.position, goal.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // direction to goal (already correct)
        Vector3 toGoal = (goal.position - transform.position).normalized;
        sensor.AddObservation(transform.InverseTransformDirection(toGoal));

        // distance to goal (normalized)
        float dist = Vector3.Distance(transform.position, goal.position);
        sensor.AddObservation(dist / maxGoalDistance);

        sensor.AddObservation(doorMover.isMovingUp ? 1f : 0f);
        Debug.Log($"DoorUp: {(doorMover.isMovingUp ? 1 : 0)}  Ray0: {raycaster.rayDistances[0]}");


        foreach (float d in raycaster.rayDistances)
        {
            sensor.AddObservation(d);
        }

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Time penalty
        AddReward(-0.001f);
        Debug.Log("Got time penalty -0.001");

        float forward = actions.ContinuousActions[0];
        float right = actions.ContinuousActions[1];
        float turn = actions.ContinuousActions[2];

        Vector3 move =
            transform.forward * forward +
            transform.right * right;

        rb.MovePosition(
            rb.position + move * moveSpeed * Time.fixedDeltaTime
        );
        Quaternion rotation =Quaternion.Euler(0f, turn * turnSpeed * Time.fixedDeltaTime, 0f);
        rb.MoveRotation(rb.rotation * rotation);

        float currentDistance = Vector3.Distance(transform.position, goal.position);

        if (currentDistance < previousDistanceToGoal)
        {
            AddReward(+0.1f);
            Debug.Log("Got reward +0.1 (closer to goal)");
        }
        else if (currentDistance > previousDistanceToGoal)
        {
            AddReward(-0.1f);
            Debug.Log("Got penalty -0.1 (farther from goal)");
        }

        previousDistanceToGoal = currentDistance;
        // ---- Facing reward ----
        Vector3 toGoal = (goal.position - transform.position).normalized;
        // float facingScore = Vector3.Dot(transform.forward, toGoal);
        float facingScore = Vector3.Dot(-transform.forward, toGoal);

        float facingReward = Mathf.Max(0f, facingScore) * 0.02f;
        

        AddReward(facingReward);
        Debug.Log("------------Facing Reward ---------------");
        Debug.Log(facingReward.ToString());
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.ContinuousActions;

        a[0] = 0f; // forward/back
        a[1] = 0f; // left/right
        a[2] = 0f; // turn

        if (Input.GetKey(KeyCode.W)) a[0] = 1f;
        if (Input.GetKey(KeyCode.S)) a[0] = -1f;

        if (Input.GetKey(KeyCode.D)) a[1] = 1f;
        if (Input.GetKey(KeyCode.A)) a[1] = -1f;

        if (Input.GetKey(KeyCode.E)) a[2] = 1f;
        if (Input.GetKey(KeyCode.Q)) a[2] = -1f;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            AddReward(-1f);
            Debug.Log("------------------------Got penalty -1 (hit door)-------------------------------");
           // EndEpisode();
        }

        if (collision.gameObject.CompareTag("GoalPlane"))
        {
            AddReward(+1f);
            Debug.Log("------------------------Got reward +1 (reached goal)------------------------------------------");
            EndWithLog("Reached goal");
            //EndEpisode();
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
            Debug.Log("-----------------------Got penalty -0.1 (hit wall)-------------------------------------");
            EndWithLog("Wall Coallision");
            //EndEpisode();
        }
    }

    private void EndWithLog(string reason)
    {
        Debug.Log($"Episode {globalEpisodeCount} ended: {reason}");
        EndEpisode();
    }

}
