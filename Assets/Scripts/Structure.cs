using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static void MakeTree (Vector3 position, Queue<VoxelMod> queue, int minTrunkHeight, int maxTrunkHeight)
    {
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 9));
        }
        // placing a cube of leaves on top of the trunk
        for (int x = -3; x <= 3; x++)
        {
            for (int y = -3; y <= 3; y++)
            {
                for (int z = -3; z <= 3; z++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z) <= 3)
                    {
                        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 10));
                    }
                }
            }
        }
    }
    public static void MakeTree2(Vector3 position, Queue<VoxelMod> queue, int minTrunkHeight, int maxTrunkHeight)
    {
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 100f, 3f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
        {
            queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 9));
        }
        // placing a cube of leaves on top of the trunk
        for (int x = -4; x <= 4; x++)
        {
            for (int y = -4; y <= 4; y++)
            {
                for (int z = -4; z <= 4; z++)
                {
                    if (Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z) <= 4)
                    {
                        queue.Enqueue(new VoxelMod(new Vector3(position.x + x, position.y + height + y, position.z + z), 10));
                    }
                }
            }
        }
    }
}
