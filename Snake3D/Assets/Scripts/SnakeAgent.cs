using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SnakeAgent : Agent
{
    [Header("References")]
    public GameObject goal;
    public float moveSpeed = 2f;
    public float rotateSpeed = 100f;
    public float maxStepDistance = 0.5f; // Maximum allowed distance per step

    [Header("Environment bounds")]
    public float xBound = 5f;
    public float zBound = 5f;

    private Rigidbody rb;
    private Vector3 startPos;
    private float previousDistanceToGoal;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
    }

    public override void OnEpisodeBegin()
    {
        // Reset snake
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset goal randomly
        goal.transform.position = new Vector3(
            Random.Range(-xBound + 0.5f, xBound - 0.5f),
            0.5f,
            Random.Range(-zBound + 0.5f, zBound - 0.5f)
        );

        previousDistanceToGoal = Vector3.Distance(transform.position, goal.transform.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Vector from snake to goal
        sensor.AddObservation(goal.transform.position - transform.position);

        // Snake velocity
        sensor.AddObservation(rb.linearVelocity);

        // Optional: normalized position (can help learning)
        sensor.AddObservation(transform.position.x / xBound);
        sensor.AddObservation(transform.position.z / zBound);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveInput = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float rotateInput = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        // Move the snake
        rb.MovePosition(transform.position + transform.forward * moveInput * moveSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(transform.rotation * Quaternion.Euler(0f, rotateInput * rotateSpeed * Time.fixedDeltaTime, 0f));

        // Calculate distance to goal
        float currentDistance = Vector3.Distance(transform.position, goal.transform.position);

        // Reward for approaching or penalize for moving away
        float distanceDelta = previousDistanceToGoal - currentDistance;
        SetReward(distanceDelta * 0.1f); // Scale factor for reward
        previousDistanceToGoal = currentDistance;

        // Check if snake reached the goal
        if (currentDistance < 0.5f)
        {
            SetReward(1f);
            // Respawn goal randomly
            goal.transform.position = new Vector3(
                Random.Range(-xBound + 0.5f, xBound - 0.5f),
                0.5f,
                Random.Range(-zBound + 0.5f, zBound - 0.5f)
            );
            previousDistanceToGoal = Vector3.Distance(transform.position, goal.transform.position);
        }

        // Check wall collisions
        if (Mathf.Abs(transform.position.x) > xBound || Mathf.Abs(transform.position.z) > zBound)
        {
            SetReward(-1f);
            EndEpisode();
        }

        // Optional: terminate if agent is stuck (exceeds max steps)
        if (StepCount > MaxStep && MaxStep > 0)
        {
            SetReward(-0.5f);
///
            EndEpisode();
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Vertical");   // W/S or Up/Down
        continuousActionsOut[1] = Input.GetAxis("Horizontal"); // A/D or Left/Right
    }
}
