//using UnityEngine;
//using System.Collections;

//public class CubeMover : MonoBehaviour
//{
//    void Start()
//    {
//        StartCoroutine(MoveUpAndDown());
//    }

//    IEnumerator MoveUpAndDown()
//    {
//        while (true)
//        {
//            // Move up by 3 units
//            transform.position += Vector3.up * 3f;
//            yield return new WaitForSeconds(1f);

//            // Move down by 3 units
//            transform.position += Vector3.down * 3f;
//            yield return new WaitForSeconds(1f);
//        }
//    }
//}
using UnityEngine;
using System.Collections;

public class CubeMover : MonoBehaviour
{
    public float distance = 4f;
    public float moveDuration = 10f;

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
            yield return StartCoroutine(MoveBetween(startPos, upPos));

            // Move down smoothly
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
