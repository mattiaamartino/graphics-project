using UnityEngine;

public class ErosionBrush : TerrainBrush
{

    public float erosionRate = 0.1f; 

    public int erosionRadius = 5; 

    public override void draw(int x, int z)
    {

        float totalHeight = 0f;
        int count = 0;
        
        for (int zi = -erosionRadius; zi <= erosionRadius; zi++)
        {
            for (int xi = -erosionRadius; xi <= erosionRadius; xi++)
            {
                if (xi * xi + zi * zi <= erosionRadius * erosionRadius)
                {
                    totalHeight += terrain.get(x + xi, z + zi);
                    count++;
                }
            }
        }

        if (count == 0) return;

        // 2. Determine the target height (the average height).
        float averageHeight = totalHeight / count;

        // 3. Apply the new height to the points based on the average.
        for (int zi = -erosionRadius; zi <= erosionRadius; zi++)
        {
            for (int xi = -erosionRadius; xi <= erosionRadius; xi++)
            {
                if (xi * xi + zi * zi <= erosionRadius * erosionRadius)
                {
                    int currentX = x + xi;
                    int currentZ = z + zi;

                    // Get the current height of the point
                    float currentHeight = terrain.get(currentX, currentZ);
                    
                    // Move the current height toward the calculated average height.
                    // If currentHeight > averageHeight, the height decreases (Erosion).
                    // If currentHeight < averageHeight, the height increases (Deposition/Smoothing).
                    float newHeight = Mathf.Lerp(currentHeight, averageHeight, erosionRate);
                    
                    // Set the final new height back to the terrain.
                    terrain.set(currentX, currentZ, newHeight);
                }
            }
        }
    }
}