using UnityEngine;
using System.Collections;

public class CubeMover : MonoBehaviour
{
    public float distance = 4f;
    public float moveDuration = 10f;
    public bool isMovingUp=false;

    void Start()
    {
        StartCoroutine(MoveUpAndDown());
    }

    IEnumerator MoveUpAndDown()
    {
        Vector3 startPos = transform.position;
        Vector3 upPos = startPos + Vector3.up * distance;

        while (true)
        {
            // Move up smoothly
            isMovingUp = true;
            Debug.Log(isMovingUp);
            yield return StartCoroutine(MoveBetween(startPos, upPos));

            // Move down smoothly
            isMovingUp = false;
            Debug.Log(isMovingUp);
            yield return StartCoroutine(MoveBetween(upPos, startPos));
        }
    }

    IEnumerator MoveBetween(Vector3 from, Vector3 to)
    {
        float elapsed = 0f;

        while (elapsed < moveDuration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / moveDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to; // ensure exact final position
    }
}
