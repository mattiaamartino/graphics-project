using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A contextual brush that places a primary instance (e.g., a Tree)
/// and then scatters a secondary instance (e.g., Mushrooms)
/// around the primary instance, modelling a relationship.
/// </summary>
public class ContextualBrush : InstanceBrush
{
    // --- PRIMARY OBJECT (The center of the relationship, e.g., the Tree) ---
    public GameObject primaryPrefab;
    public float primarySpawnChance = 0.5f; // Chance that a primary object spawns per brush click

    // --- SECONDARY OBJECT (The scattered object, e.g., Mushrooms/Coconuts) ---
    public GameObject secondaryPrefab;
    public int secondaryCount = 5; // How many secondary objects to spawn per primary
    public float minSecondaryRadius = 2.0f; // Minimum distance from primary
    public float maxSecondaryRadius = 4.0f; // Maximum distance from primary
    public float secondarySpawnChance = 0.9f; // Chance for each secondary item to spawn

    public override void draw(float x, float z)
    {
        // 1. Check if the primary object should spawn based on chance
        if (Random.value > primarySpawnChance)
        {
            return; // Skip spawning everything if the primary fails its chance
        }

        // 2. Pick a random central spot for the primary object within the brush radius
        float centerX = x + Random.Range(-terrain.brush_radius, terrain.brush_radius);
        float centerZ = z + Random.Range(-terrain.brush_radius, terrain.brush_radius);

        // --- Spawn the Primary Object (The Tree) ---
        SpawnInstance(primaryPrefab, centerX, centerZ, true);


        // --- Spawn the Secondary Objects (Mushrooms/Coconuts) ---
        
        // Loop to spawn the specified count of secondary objects
        for (int i = 0; i < secondaryCount; i++)
        {
            // Check if this specific secondary object should spawn based on its chance
            if (Random.value < secondarySpawnChance)
            {
                // Calculate position relative to the primary object
                float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
                float radius = Random.Range(minSecondaryRadius, maxSecondaryRadius);

                float secondaryX = centerX + radius * Mathf.Cos(angle);
                float secondaryZ = centerZ + radius * Mathf.Sin(angle);

                // Spawn the secondary object
                SpawnInstance(secondaryPrefab, secondaryX, secondaryZ, false);
            }
        }
    }

    /// <summary>
    /// Helper function to handle the actual registration and spawning process.
    /// </summary>
    private void SpawnInstance(GameObject prefab, float posX, float posZ, bool isPrimary)
    {
        if (prefab == null)
        {
            // If the secondary prefab isn't assigned, it's ok, just continue.
            if (isPrimary) Debug.LogError("ContextualBrush is missing the Primary Prefab!");
            return;
        }

        // 1. Register the chosen prefab with the terrain to get its ID.
        int proto_idx = terrain.registerPrefab(prefab);

        // 2. Get the full 3D location, including the correct terrain height.
        // Note: Using the interpolated location is often better for detail placement.
        Vector3 loc = terrain.getInterp3(posX, posZ);

        // 3. Get a random scale.
        float scale = Random.Range(terrain.min_scale, terrain.max_scale);

        // 4. Call the terrain's spawn function.
        terrain.spawnObject(loc, scale, proto_idx);
    }
}