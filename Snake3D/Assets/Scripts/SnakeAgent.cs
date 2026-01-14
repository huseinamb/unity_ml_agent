using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SnakeAgent : Agent
{
    [Header("References")]
    public GameObject goal;
    public float moveSpeed = 2f;
    public float maxStepDistance = 0.5f; // Maximum allowed distance per step

    [Header("Movement bounds (visual/reference only)")]
    public float xBound = 5f;
    public float zBound = 5f;

    private Rigidbody rb;
    private Vector3 startPos;
    private float previousDistanceToGoal;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;
        rb.sleepThreshold = 0f;
    }

    public override void OnEpisodeBegin()
    {
        // Reset snake
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Reset goal
        RespawnGoal();

        previousDistanceToGoal = Vector3.Distance(transform.position, goal.transform.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Vector from snake to goal
        sensor.AddObservation(goal.transform.position - transform.position);

        // Snake velocity
        sensor.AddObservation(rb.linearVelocity);

        // Optional: normalized position
        sensor.AddObservation(transform.position.x / xBound);
        sensor.AddObservation(transform.position.z / zBound);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = Mathf.Clamp(actions.ContinuousActions[0], -1f, 1f);
        float moveZ = Mathf.Clamp(actions.ContinuousActions[1], -1f, 1f);

        Vector3 moveDir = new Vector3(moveX, 0f, moveZ);

        // Move using MovePosition
        Vector3 newPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Optional: face movement direction visually
        if (moveDir.sqrMagnitude > 0.001f)
            transform.forward = moveDir.normalized;

        // Reward for moving closer to goal
        float currentDistance = Vector3.Distance(transform.position, goal.transform.position);
        float delta = previousDistanceToGoal - currentDistance;
        AddReward(delta * 0.1f);
        previousDistanceToGoal = currentDistance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var actions = actionsOut.ContinuousActions;
        actions[0] = Input.GetAxis("Horizontal"); // A/D or Left/Right
        actions[1] = Input.GetAxis("Vertical");   // W/S or Up/Down
    }

    void RespawnGoal()
    {
        Collider goalCollider = goal.GetComponent<Collider>();
        goalCollider.enabled = false;

        goal.transform.position = new Vector3(
            Random.Range(-xBound + 1f, xBound - 1f),
            0.5f,
            Random.Range(-zBound + 1f, zBound - 1f)
        );

        StartCoroutine(ReenableColliderNextFixed(goalCollider));
    }

    System.Collections.IEnumerator ReenableColliderNextFixed(Collider col)
    {
        yield return new WaitForFixedUpdate();
        col.enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Collision with goal
        if (collision.gameObject == goal)
        {
            AddReward(1f);
            RespawnGoal();
            previousDistanceToGoal = Vector3.Distance(transform.position, goal.transform.position);
        }

        // Collision with wall (must tag your walls as "Wall")
        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
