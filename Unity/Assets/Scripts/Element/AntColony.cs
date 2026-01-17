using System.Collections;
using UnityEngine;

public sealed class AntColony : MonoBehaviour
{
    [SerializeField] private int initialAntCount = 100;
    [SerializeField] private int spawnPerFrame = 10;
    [SerializeField] private GameObject antPrefab;

    [SerializeField] private int foodCollected = 0;

    private void Start()
    {
        StartCoroutine(Spawn());
    }

    private IEnumerator Spawn()
    {
        int spawned = 0;
        while (spawned < initialAntCount)
        {
            int n = Mathf.Min(spawnPerFrame, initialAntCount - spawned);
            for (int i = 0; i < n; i++)
            {
                Instantiate(antPrefab, transform.position, Quaternion.identity);
            }

            spawned += n;
            yield return null;
        }
    }

    public void AddFood()
    {
        foodCollected++;
    }
}
