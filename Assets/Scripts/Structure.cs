using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Structure
{
    public static Queue<VoxelMod> MakeTree(Vector3 position, int minTrunkHeight, int maxTrunkHeight, bool isBig)
    {
        Queue<VoxelMod> queue = new Queue<VoxelMod>();
        int height = (int)(maxTrunkHeight * Noise.Get2DPerlin(new Vector2(position.x, position.z), 250f, 3f));
        if (height < minTrunkHeight)
            height = minTrunkHeight;

        for (int i = 1; i < height; i++)
        {
            if (isBig)
            {
                queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 9));
                queue.Enqueue(new VoxelMod(new Vector3(position.x + 1, position.y + i, position.z), 9));
                queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z + 1), 9));
                queue.Enqueue(new VoxelMod(new Vector3(position.x + 1, position.y + i, position.z + 1), 9));
            }
            else
            {
                queue.Enqueue(new VoxelMod(new Vector3(position.x, position.y + i, position.z), 9));
            }
        }
        //placing a cube of leaves on top of the trunk
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
        return queue;
    }
}