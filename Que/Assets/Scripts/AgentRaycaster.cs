using UnityEngine;

public class AgentRaycaster : MonoBehaviour
{
    public int rayCount = 8;
    public float rayLength = 5f;
    public bool FrontBlocked { get; private set; }
    public float[] rayDistances;
    void Start()
    {
        rayDistances = new float[rayCount];
    }

    void Update()
    {
        CastRays();
    }

    void CastRays()
    {
        FrontBlocked = false;

        float angleStep = 360f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, rayLength))
            {
                rayDistances[i] = hit.distance / rayLength;
                Debug.DrawRay(transform.position, direction * hit.distance, Color.red, 0.1f);
                FrontBlocked = true;
            }
            else
            {
                rayDistances[i] = 1f;
            }
        }
    }


}
