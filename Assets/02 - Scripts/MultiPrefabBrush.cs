using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A brush that places a random prefab from a
/// specified list, allowing you to paint a mix of objects.
/// </summary>
public class MultiPrefabBrush : InstanceBrush
{
    // Assign your different prefabs (e.g., Tree1, Tree2, Bush)
    // in the Inspector.
    public List<GameObject> prefabs;

    public override void draw(float x, float z)
    {
        // 1. Check if the prefab list is empty or unassigned
        if (prefabs == null || prefabs.Count == 0)
        {
            Debug.LogError("MultiPrefabBrush has no prefabs assigned in the Inspector!");
            return;
        }

        // 2. Pick a random prefab from the list
        int prefabIndex = Random.Range(0, prefabs.Count);
        GameObject prefabToSpawn = prefabs[prefabIndex];

        // 3. Pick a random spot in the brush radius
        float randX = x + Random.Range(-terrain.brush_radius, terrain.brush_radius);
        float randZ = z + Random.Range(-terrain.brush_radius, terrain.brush_radius);

        
        // --- Spawning the Object ---
        // We must call the main terrain.spawnObject() function,
        // which requires a prototype index, a location, and a scale.

        // 1. Register the chosen prefab with the terrain.
        // This gets its unique ID (prototype index) for the terrain data.
        int proto_idx = terrain.registerPrefab(prefabToSpawn);

        // 2. Get the full 3D location, including the correct terrain height.
        Vector3 loc = terrain.getInterp3(randX, randZ);

        // 3. Get a random scale using the global settings from CustomTerrain.
        float scale = Random.Range(terrain.min_scale, terrain.max_scale);

        // 4. Call the terrain's spawn function with all the required data.
        terrain.spawnObject(loc, scale, proto_idx);
    }
}