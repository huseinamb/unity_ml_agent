using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class QueueAgent : Agent
{
    public float moveSpeed = 2f;
    public Transform goal;

    Rigidbody rb;
    AgentRaycaster raycaster;

    float previousDistanceToGoal;
    float maxGoalDistance;
    public CubeMover doorMover;



    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        raycaster = GetComponent<AgentRaycaster>();
    }

    public override void OnEpisodeBegin()
    {
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

        Vector3 move =
            transform.forward * forward +
            transform.right * right;

        rb.MovePosition(
            rb.position + move * moveSpeed * Time.fixedDeltaTime
        );

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
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;

        actions[0] = 0f;
        actions[1] = 0f;

        if (Input.GetKey(KeyCode.W)) actions[0] = 1f;
        if (Input.GetKey(KeyCode.S)) actions[0] = -1f;
        if (Input.GetKey(KeyCode.D)) actions[1] = 1f;
        if (Input.GetKey(KeyCode.A)) actions[1] = -1f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Door"))
        {
            AddReward(-1f);
            Debug.Log("------------------------Got penalty -1 (hit door)-------------------------------");
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("GoalPlane"))
        {
            AddReward(+1f);
            Debug.Log("------------------------Got reward +1 (reached goal)------------------------------------------");
            EndEpisode();
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
            Debug.Log("-----------------------Got penalty -0.1 (hit wall)-------------------------------------");
        }
    }
}
