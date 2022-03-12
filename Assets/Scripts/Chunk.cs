using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chunk {
    public ChunkCoord coord;
    GameObject chunkObj;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    int i = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    World world;
    public Chunk(ChunkCoord _coord, World _world)
    {
        coord = _coord;
        world = _world;

        chunkObj = new GameObject();
        meshFilter = chunkObj.AddComponent<MeshFilter>();
        meshRenderer = chunkObj.AddComponent<MeshRenderer>();
        meshRenderer.material = world.material;
        chunkObj.transform.SetParent(world.transform);
        chunkObj.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);
        chunkObj.name = "Chunk " + coord.x + ", " + coord.z;

        PopulateVoxelMap();
        // creating chunk
        for (int y = 0; y < VoxelData.ChunkHeight; y += 1)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x += 1)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z += 1)
                {
                    AddVoxelData(new Vector3(x, y, z));
                }
            }
        }
        // creating mesh
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
    }

    void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y += 1)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x += 1)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z += 1)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }
    }

    public bool isActive
    {
        get { return chunkObj.activeSelf; }
        set { chunkObj.SetActive(value); }
    }

    public Vector3 position {
        get { return chunkObj.transform.position; }
    }

    bool IsVoxelInChunk(int x, int y, int z)
    {
        return !(x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0
            || z > VoxelData.ChunkWidth - 1);
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        // prevent index out of range
        if (!IsVoxelInChunk(x, y, z)){
            // prevent chunks from rendering inside each other
            return world.blocktypes[world.GetVoxel(pos + position)].isSolid;
        }
        return world.blocktypes[voxelMap[x, y, z]].isSolid;
    }

    void AddVoxelData(Vector3 pos)
    {
        for (int l = 0; l < 6; l += 1)
        {
            if (!CheckVoxel(pos + VoxelData.faceChecks[l])) {
                byte blockID = voxelMap[(int)pos.x, (int)pos.y, (int)pos.z];
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 0]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 1]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 2]]);
                vertices.Add(pos + VoxelData.voxelVerts[VoxelData.voxelTris[l, 3]]);
                //AddTexture(0); // 0 is the texture for stone
                AddTexture(world.blocktypes[blockID].GetTextureID(l));
                triangles.Add(i);
                triangles.Add(i + 1);
                triangles.Add(i + 2);
                triangles.Add(i + 2);
                triangles.Add(i + 1);
                triangles.Add(i + 3);
                i += 4;
            }
        }
    }

    void AddTexture(int textureID)
    {
        // calculate correct placement based on texture ID
        float y = textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureID - (y * VoxelData.TextureAtlasSizeInBlocks);
        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;
        y = 1f - y - VoxelData.NormalizedBlockTextureSize;
        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));

    }
}

public class ChunkCoord {
    public int x; // position of chunk we are drawing in the chunk map, not world space
    public int z;
    public ChunkCoord(int _x, int _z){
        x = _x;
        z = _z;
    }
    public bool Equals(ChunkCoord other)
    {
        if (other == null)
        {
            return false;
        } else if (other.x == x && other.z == z)
        {
            return true;
        }else
        {
            return false;
        }
    }
}