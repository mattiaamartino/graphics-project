using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A terrain-aware brush that places fewer instances on steep slopes
/// and at higher elevations.
/// </summary>
public class TerrainAwareInstanceBrush : InstanceBrush
{
    // --- Prefabs and Base Density ---
    public GameObject prefab;
    [Range(0.0f, 1.0f)]
    public float baseSpawnChance = 0.5f; // Base chance to spawn per click

    // --- Slope-based Control ---
    [Header("Slope Limits")]
    public float maxSlopeAngle = 45.0f;  // Instances won't spawn above this angle (in degrees)
    public float slopeDensityReduction = 0.5f; // Rate at which chance reduces as slope increases (0 to 1)

    // --- Height-based Control ---
    [Header("Height Limits")]
    public float maxTerrainHeight = 100.0f; // Maximum height of the terrain (for normalization)
    public float heightDensityReduction = 1.0f; // Higher value means faster reduction with height

    public override void draw(float x, float z)
    {
        if (prefab == null)
        {
            Debug.LogError("TerrainAwareInstanceBrush is missing a Prefab!");
            return;
        }

        // 1. Calculate the final, terrain-adjusted spawn chance
        float finalSpawnChance = CalculateFinalSpawnChance(x, z);

        // 2. Check the final chance against a random value
        if (Random.value < finalSpawnChance)
        {
            // 3. If chance passes, spawn the instance
            SpawnInstance(x, z);
        }
    }

    /// <summary>
    /// Calculates the actual probability of spawning based on slope and height.
    /// </summary>
    private float CalculateFinalSpawnChance(float x, float z)
    {
        // Start with the user-defined base chance
        float chance = baseSpawnChance;
        
        // --- 1. Slope Reduction ---
        


        
        // Calculate the angle between the normal (perpendicular to surface) and Vector3.up
        // The steeper the slope, the larger the angle (90 degrees = vertical cliff)
        float slopeAngle = terrain.getSteepness(x, z);

        if (slopeAngle > maxSlopeAngle)
        {
            // Do not spawn at all if the slope is too steep
            return 0.0f;
        }

        // Calculate the reduction factor based on the slope:
        // slopeFactor goes from 1.0 (flat) down to 0.0 (maxSlopeAngle)
        float slopeFactor = 1.0f - (slopeAngle / maxSlopeAngle);
        
        // Apply a smooth reduction using the user-defined slopeDensityReduction rate
        chance *= Mathf.Lerp(1.0f, slopeFactor, slopeDensityReduction);


        // --- 2. Height Reduction ---

        // Get the current height at the location (x, z)
        float currentHeight = terrain.getInterp(x, z);

        // Normalize height (0.0 at sea level, 1.0 at max height)
        // Ensure maxTerrainHeight is set correctly in the Inspector
        float normalizedHeight = Mathf.Clamp01(currentHeight / maxTerrainHeight);

        // Calculate the reduction factor based on height:
        // heightFactor goes from 1.0 (low) down to 0.0 (high)
        float heightFactor = 1.0f - normalizedHeight;

        // Apply reduction, using heightDensityReduction to control the steepness of the falloff
        chance *= Mathf.Pow(heightFactor, heightDensityReduction);
        
        return Mathf.Clamp01(chance); // Ensure the chance is never below 0 or above 1
    }
    
    /// <summary>
    /// Handles the actual spawning process after checks have passed.
    /// </summary>
    private void SpawnInstance(float x, float z)
    {
        // Pick a random spot in the brush radius
        float randX = x + Random.Range(-terrain.brush_radius, terrain.brush_radius);
        float randZ = z + Random.Range(-terrain.brush_radius, terrain.brush_radius);

        // 1. Register the chosen prefab with the terrain.
        int proto_idx = terrain.registerPrefab(prefab);

        // 2. Get the full 3D location, including the correct terrain height.
        Vector3 loc = terrain.getInterp3(randX, randZ);

        // 3. Get a random scale using the global settings.
        float scale = Random.Range(terrain.min_scale, terrain.max_scale);

        // 4. Call the terrain's spawn function with all the required data.
        terrain.spawnObject(loc, scale, proto_idx);
    }
}