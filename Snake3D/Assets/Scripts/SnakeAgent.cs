using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SnakeAgent : Agent
{
    [Header("References")]
    public GameObject goal;
    public float moveDistance = 0.5f; // Distance per step

    [Header("Environment bounds")]
    public string wallTag = "Wall"; // Assign all wall objects this tag

    private Rigidbody rb;
    private Vector3 startPos;
    private float previousDistanceToGoal;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        startPos = transform.position;

        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
        rb.sleepThreshold = 0f;
    }

    public override void OnEpisodeBegin()
    {
        // Reset snake
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Respawn goal
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
        sensor.AddObservation(transform.position.x / 50f);
        sensor.AddObservation(transform.position.z / 50f);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int moveDir = actions.DiscreteActions[0];

        Vector3 move = Vector3.zero;

        // Discrete movement: 0=Forward, 1=Backward, 2=Left, 3=Right
        switch (moveDir)
        {
            case 0: move = transform.forward; break;
            case 1: move = -transform.forward; break;
            case 2: move = -transform.right; break;
            case 3: move = transform.right; break;
        }

        // Move snake
        Vector3 newPos = rb.position + move.normalized * moveDistance;
        rb.MovePosition(newPos);

        // Reward based on distance to goal
        float currentDistance = Vector3.Distance(transform.position, goal.transform.position);
      //  float delta = previousDistanceToGoal - currentDistance;
        if (currentDistance > 0.6f)
        {
            float delta = previousDistanceToGoal - currentDistance;
            AddReward(delta * 0.1f);
        }

// AddReward(delta * 0.1f);
        previousDistanceToGoal = currentDistance;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = -1; // Default: no movement

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            discreteActionsOut[0] = 0;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            discreteActionsOut[0] = 1;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            discreteActionsOut[0] = 2;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            discreteActionsOut[0] = 3;
    }

    private void RespawnGoal()
    {
        Collider goalCollider = goal.GetComponent<Collider>();
        goalCollider.enabled = false;

        goal.transform.position = new Vector3(
            Random.Range(-49f, 49f),
            0.5f,
            Random.Range(-49f, 49f)
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
        if (collision.gameObject == goal)
        {
            AddReward(1f);
            RespawnGoal();
            previousDistanceToGoal = Vector3.Distance(transform.position, goal.transform.position);
        }
        else if (collision.gameObject.CompareTag(wallTag))
        {
            AddReward(-1f);
            EndEpisode();
        }
    }
}
