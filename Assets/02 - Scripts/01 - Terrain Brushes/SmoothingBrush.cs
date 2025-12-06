using UnityEngine;

public class SmoothingBrush : TerrainBrush
{
    public float strength = 0.1f;
    public int brush_radius = 5;

    public override void draw(int x, int z)
    {
        for (int zi = -brush_radius; zi <= brush_radius; zi++)
        {
            for (int xi = -brush_radius; xi <= brush_radius; xi++)
            {
                if (xi * xi + zi * zi <= brush_radius * brush_radius)
                {
                    float h = terrain.get(x + xi, z + zi);
                    h -= strength;
                    terrain.set(x + xi, z + zi, h);
                }
            }
        }
    }
}