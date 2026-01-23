using UnityEngine;

public class FoodCollectorArea : MonoBehaviour
{
    public GameObject food;
    public GameObject badFood;
    public int numFood;
    public int numBadFood;
    public bool respawnFood;
    public float range;

    void CreateFood(int num, GameObject type)
    {
        for (int i = 0; i < num; i++)
        {
            GameObject f = Instantiate(
                type,
                new Vector3(
                    Random.Range(-range, range),
                    1f,
                    Random.Range(-range, range)
                ) + transform.position,
                Quaternion.Euler(0f, Random.Range(0f, 360f), 90f) // ✅ FIXED
            );

            var logic = f.GetComponent<FoodLogic>();
            logic.respawn = respawnFood;
            logic.myArea = this;
        }
    }

    public void ResetFoodArea(GameObject[] agents)
    {
        foreach (GameObject agent in agents)
        {
            agent.transform.position = new Vector3(
                Random.Range(-range, range),
                2f,
                Random.Range(-range, range)
            ) + transform.position;

            agent.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
        }

        CreateFood(numFood, food);
        CreateFood(numBadFood, badFood);
    }

}
