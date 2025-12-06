using UnityEngine;

public class SurvivorSpawner : MonoBehaviour
{
    public GameObject humanPrefab;
    public GameObject predatorPrefab;
    public GameObject preyPrefab;
    public GameObject vegetablePrefab;
    public GameObject fishPrefab;

    public int humanCount = 1;
    public int predatorCount = 2;
    public int preyCount = 10;
    public int vegetableCount = 20;

    public int fishCount = 50;
    public float waterLevelY = 9.0f;
    public float fishSpawnDepth = 2.0f;

    public CustomTerrain terrain;

    void Start()
    {
        if (terrain == null) terrain = FindObjectOfType<CustomTerrain>();
        
        Spawn(humanPrefab, humanCount, false);
        Spawn(predatorPrefab, predatorCount, false);
        Spawn(preyPrefab, preyCount, false);
        Spawn(vegetablePrefab, vegetableCount, false);
        Spawn(fishPrefab, fishCount, true);
    }

    void Spawn(GameObject prefab, int count, bool isFish)
    {
        if (prefab == null) return;

        Vector3 size = terrain != null ? terrain.terrainSize() : new Vector3(100, 0, 100);

        for (int i = 0; i < count; i++)
        {
            float x = Random.Range(40, 60); // Random.Range(0, size.x);
            float z = Random.Range(40, 60); // Random.Range(0, size.z);
            float y;

            if (isFish)
            {
                y = waterLevelY + Random.Range(0f, fishSpawnDepth);
            }
            else
            {
                y = terrain != null ? terrain.getInterp(x, z) : 0;
            }

            Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity);
        }
    }
}