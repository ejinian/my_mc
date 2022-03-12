using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class VoxelData {

    public static readonly int ChunkWidth = 16;
    public static readonly int ChunkHeight = 128;
    public static readonly int WorldSizeInChunks = 100;
    public static int WorldSizeInVoxels
    {
        get { return WorldSizeInChunks * ChunkWidth; }
    }
    public static readonly int viewDistanceInChunks = 5;

    public static readonly int TextureAtlasSizeInBlocks = 4;
    public static float NormalizedBlockTextureSize
    {
        get { return 1f / (float)TextureAtlasSizeInBlocks; }
    }
    public static readonly Vector3[] voxelVerts = new Vector3[8]
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f),
    };
    public static readonly Vector3[] faceChecks = new Vector3[6]
    {
        new Vector3(0.0f, 0.0f, -1.0f), // checking back face
        new Vector3(0.0f, 0.0f, 1.0f), // front
        new Vector3(0.0f, 1.0f, 0.0f), // top
        new Vector3(0.0f, -1.0f, 0.0f), // bottom
        new Vector3(-1.0f, 0.0f, 0.0f), // left
        new Vector3(1.0f, 0.0f, 0.0f), // right
    };
    public static readonly int[,] voxelTris = new int[6, 4]
    { // vertex render order
        {0, 3, 1, 2 }, // back
        {5, 6, 4, 7 }, // front
        {3, 7, 2, 6 }, // top
        {1, 5, 0, 4 }, // bottom
        {4, 7, 0, 3 }, // left
        {1, 2, 5, 6 }, // right
    };
    public static readonly Vector2[] voxeluvs = new Vector2[4]
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f),
    };
}
