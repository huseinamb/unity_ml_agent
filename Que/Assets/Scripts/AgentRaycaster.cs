using UnityEngine;

public class AgentRaycaster : MonoBehaviour
{
    public int rayCount = 8;
    public float rayLength = 5f;

    void Update()
    {
        CastRays();
    }

    void CastRays()
    {
        float angleStep = 360f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = i * angleStep;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;

            Ray ray = new Ray(transform.position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, rayLength))
            {
                Debug.DrawRay(transform.position, direction * hit.distance, Color.red, 0.1f);
            }
            else
            {
                Debug.Log("No hit");
               // Debug.DrawRay(transform.position, direction * rayLength, Color.green, 0.1f);
            }
        }
    }
}
