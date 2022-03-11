using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Chunk : MonoBehaviour {
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    int i = 0;
    List<Vector3> vertices = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
    World world;
    void Start () {

        world = GameObject.Find("World").GetComponent<World>();

        PopulateVoxelMap();
        // creating chunk
        for (int y = 0; y < VoxelData.ChunkHeight; y += 1)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x += 1)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z += 1) {
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
                    // stone[0], bedrock[1], grass[2], furnace[3]
                    if (y < 1) voxelMap[x, y, z] = 1;
                    else if (y == VoxelData.ChunkHeight - 1) voxelMap[x, y, z] = 2;
                    else
                    {
                        voxelMap[x, y, z] = 0;
                    }
                }
            }
        }
    }

    bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);
        // prevent index out of range
        if (x < 0 || x > VoxelData.ChunkWidth - 1 || y < 0 || y > VoxelData.ChunkHeight - 1 || z < 0
            || z > VoxelData.ChunkWidth - 1){
            return false;
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
