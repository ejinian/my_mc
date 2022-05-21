using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public static class Noise {
    public static float Get2DPerlin(Vector2 position, float offset, float scale)
    {
        position.x += (offset + VoxelData.seed + 0.1f);
        position.y += (offset + VoxelData.seed + 0.1f);
        // scale is the portion of the perlin noise that you want generated
        // i.e. smaller scale means smoother transitions and more even ground,
        // and larger scale means more bumps/mountains
        return Mathf.PerlinNoise(position.x / VoxelData.ChunkWidth * scale,
            position.y / VoxelData.ChunkWidth * scale);
    }
    public static bool Get3DPerlin(Vector3 position, float offset, float scale, float threshold)
    {
        float x = (position.x + offset + VoxelData.seed + 0.1f) * scale;
        float y = (position.y + offset + VoxelData.seed + 0.1f) * scale;
        float z = (position.z + offset + VoxelData.seed + 0.1f) * scale; 
        float AB = Mathf.PerlinNoise(x, y);
        float BC = Mathf.PerlinNoise(y, z);
        float AC = Mathf.PerlinNoise(x, z);
        float BA = Mathf.PerlinNoise(y, x);
        float CB = Mathf.PerlinNoise(z, y);
        float CA = Mathf.PerlinNoise(z, x);

        return ((AB + BC + AC + BA + CB + CA) / 6f > threshold);
    }
}
