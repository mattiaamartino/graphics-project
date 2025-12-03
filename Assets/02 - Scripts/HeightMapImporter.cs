using UnityEngine;
using UnityEditor; // We need this for the [ContextMenu] attribute

/// <summary>
/// This component script is added to a Terrain object to import a Texture2D as a heightmap.
/// This is necessary because Unity's built-in "Import Raw" only works for .raw files, not .png or .jpg.
///
/// HOW TO USE:
/// 1. Save this script as "HeightmapImporter.cs".
/// 2. Drag this script onto your Terrain object in the Hierarchy.
/// 3. In the Inspector, assign your AI-generated .png to the "Heightmap Texture" slot.
/// 4. Set the "Terrain Max Height" (e.g., 100).
/// 5. Right-click the component's name (or click the '...' menu) and select one of the functions.
/// </summary>
[RequireComponent(typeof(Terrain))]
public class HeightmapImporter : MonoBehaviour
{
    [Header("Import Settings")]
    public Texture2D heightmapTexture;
    public float terrainMaxHeight = 100.0f;

    private Terrain terrain;
    private TerrainData terrainData;

    /// <summary>
    /// This is the main function. It REPLACES the terrain with the new heightmap.
    /// </summary>
    [ContextMenu("1. (SET) Import Heightmap (Overwrite)")]
    void ImportHeightmap_Overwrite()
    {
        if (heightmapTexture == null)
        {
            Debug.LogError("Heightmap Importer: No heightmap texture assigned!");
            return;
        }

        if (terrain == null)
        {
            terrain = this.GetComponent<Terrain>();
        }
        terrainData = terrain.terrainData;

        // Get terrain size
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Create a new heightmap array
        float[,] heights = new float[width, height];

        // We must loop through all terrain pixels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // We need to sample the texture correctly.
                // We convert the terrain pixel (x, y) to a texture UV coordinate (0.0 to 1.0)
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);

                // Sample the texture at that UV point.
                // GetPixelBilinear provides smoother results than GetPixel.
                float heightValue = heightmapTexture.GetPixelBilinear(u, v).grayscale;

                // Set the height in our array.
                // The heightmap (0-1) is multiplied by the desired max height.
                // Unity's SetHeights expects a value between 0.0 and 1.0,
                // so we must divide by the terrain's max Y scale.
                heights[y, x] = heightValue * (terrainMaxHeight / terrainData.heightmapScale.y);
            }
        }

        // Finally, apply the new height array to the terrain data
        terrainData.SetHeights(0, 0, heights);
        Debug.Log("Heightmap import complete!");
    }

    /// <summary>
    /// This function ADDS the heightmap to the existing terrain.
    /// </summary>
    [ContextMenu("2. (ADD) Import Heightmap (Additive)")]
    void ImportHeightmap_Additive()
    {
        if (heightmapTexture == null)
        {
            Debug.LogError("Heightmap Importer: No heightmap texture assigned!");
            return;
        }

        if (terrain == null)
        {
            terrain = this.GetComponent<Terrain>();
        }
        terrainData = terrain.terrainData;

        // Get terrain size
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Get the *current* heights from the terrain
        float[,] heights = terrainData.GetHeights(0, 0, width, height);

        // We must loop through all terrain pixels
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Sample the texture (see above)
                float u = (float)x / (width - 1);
                float v = (float)y / (height - 1);
                float heightValue = heightmapTexture.GetPixelBilinear(u, v).grayscale;

                // Get the *current* height
                float currentHeight = heights[y, x];

                // Add the new height to the current height
                heights[y, x] = currentHeight + (heightValue * (terrainMaxHeight / terrainData.heightmapScale.y));
            }
        }

        // Apply the modified height array
        terrainData.SetHeights(0, 0, heights);
        Debug.Log("Heightmap additive import complete!");
    }
}