using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collisions : MonoBehaviour
{
    // FRONT COLLISIONS

    public static bool frontDirectly(World world, Transform transform, float width)
    {
        // Directly in front of the player
        
        if (world.CheckForVoxel(transform.position + Vector3.forward * width))
        {
            Debug.Log("Front directly");
            return true;
        }
        else 
        {
            return false;
        }
        
    }

    public static bool frontRight(World world, Transform transform, float width)
    {
        // Directly in front of the player + width right
        
        if (world.CheckForVoxel(transform.position + Vector3.forward * width +
            Vector3.right * width))
        {
            Debug.Log("Front right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool frontLeft(World world, Transform transform, float width)
    {
        // Directly in front of the player + width left
        
        if (world.CheckForVoxel(transform.position + Vector3.forward * width +
            Vector3.left * width))
        {
            Debug.Log("Front left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool frontUp(World world, Transform transform, float width)
    {
        // Directly in front of the player + 1 block up
        
        if (world.CheckForVoxel(transform.position + Vector3.up +
            Vector3.forward * width))
        {
            Debug.Log("Front up");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool frontUpRight(World world, Transform transform, float width)
    {
        // Directly in front of the player + 1 block up + width right
        
        if (world.CheckForVoxel(transform.position + Vector3.up +
            Vector3.forward * width + Vector3.right * width))
        {
            Debug.Log("Front up right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool frontUpLeft(World world, Transform transform, float width)
    {
        // Directly in front of the player + 1 block up + width left
       
        if (world.CheckForVoxel(transform.position + Vector3.up +
            Vector3.forward * width + Vector3.left * width))
        {
            Debug.Log("Front up left");
            return true;
        }
        else 
        {
            return false;
        }
    }


    // BACK COLLISIONS


    public static bool backDirectly(World world, Transform transform, float width)
    {
        // Directly behind the player
        
        if (world.CheckForVoxel(transform.position - Vector3.forward * width))
        {
            Debug.Log("Back directly");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool backRight(World world, Transform transform, float width)
    {
        // Directly behind the player + width right
        
        if (world.CheckForVoxel(transform.position - Vector3.forward * width +
            Vector3.right * width))
        {
            Debug.Log("Back right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool backLeft(World world, Transform transform, float width)
    {
        // Directly behind the player + width left
        
        if (world.CheckForVoxel(transform.position - Vector3.forward * width +
            Vector3.left * width))
        {
            Debug.Log("Back left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool backUp(World world, Transform transform, float width)
    {
        // Directly behind the player + 1 block up
        
        if (world.CheckForVoxel(transform.position + Vector3.up -
            Vector3.forward * width))
        {
            Debug.Log("Back up");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool backUpRight(World world, Transform transform, float width)
    {
        // Directly behind the player + 1 block up + width right
        
        if (world.CheckForVoxel(transform.position + Vector3.up -
            Vector3.forward * width + Vector3.right * width))
        {
            Debug.Log("Back up right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool backUpLeft(World world, Transform transform, float width)
    {
        // Directly behind the player + 1 block up + width left
        
        if (world.CheckForVoxel(transform.position + Vector3.up -
            Vector3.forward * width + Vector3.left * width))
        {
            Debug.Log("Back up left");
            return true;
        }
        else 
        {
            return false;
        }
    }


    // LEFT COLLISIONS


    public static bool leftDirectly(World world, Transform transform, float width)
    {
        // Directly to the left of the player
        
        if (world.CheckForVoxel(transform.position - Vector3.right * width))
        {
            Debug.Log("Left directly");
            return true;
        }
        else 
        {
            return false;
        }

    }

    public static bool leftForward(World world, Transform transform, float width)
    {
        // Directly to the left of the player + width forward
        
        if (world.CheckForVoxel(transform.position - Vector3.right * width +
            Vector3.forward * width))
        {
            Debug.Log("Left forward");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool leftBack(World world, Transform transform, float width)
    {
        // Directly to the left of the player + width back
        
        if (world.CheckForVoxel(transform.position - Vector3.right * width +
            Vector3.back * width))
        {
            Debug.Log("Left back");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool leftUp(World world, Transform transform, float width)
    {
        // Directly to the left of the player + 1 block up
        
        if (world.CheckForVoxel(transform.position + Vector3.up -
            Vector3.right * width))
        {
            Debug.Log("Left up");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool leftUpForward(World world, Transform transform, float width)
    {
        // Directly to the left of the player + 1 block up + width forward
        
        if (world.CheckForVoxel(transform.position + Vector3.up -
            Vector3.right * width + Vector3.forward * width))
        {
            Debug.Log("Left up forward");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool leftUpBack(World world, Transform transform, float width)
    {
        // Directly to the left of the player + 1 block up + width back
        
        if (world.CheckForVoxel(transform.position + Vector3.up -
            Vector3.right * width + Vector3.back * width))
        {
            Debug.Log("Left up back");
            return true;
        }
        else 
        {
            return false;
        }
    }


    // RIGHT COLLISIONS


    public static bool rightDirectly(World world, Transform transform, float width)
    {
        // Directly to the right of the player
        
        if (world.CheckForVoxel(transform.position + Vector3.right * width))
        {
            Debug.Log("Right directly");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool rightForward(World world, Transform transform, float width)
    {
        // Directly to the right of the player + width forward
        
        if (world.CheckForVoxel(transform.position + Vector3.right * width +
            Vector3.forward * width))
        {
            Debug.Log("Right forward");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool rightBack(World world, Transform transform, float width)
    {
        // Directly to the right of the player + width back
        
        if (world.CheckForVoxel(transform.position + Vector3.right * width +
            Vector3.back * width))
        {
            Debug.Log("Right back");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool rightUp(World world, Transform transform, float width)
    {
        // Directly to the right of the player + 1 block up
        
        if (world.CheckForVoxel(transform.position + Vector3.up +
            Vector3.right * width))
        {
            Debug.Log("Right up");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool rightUpForward(World world, Transform transform, float width)
    {
        // Directly to the right of the player + 1 block up + width forward
        
        if (world.CheckForVoxel(transform.position + Vector3.up +
            Vector3.right * width + Vector3.forward * width))
        {
            Debug.Log("Right up forward");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool rightUpBack(World world, Transform transform, float width)
    {
        // Directly to the right of the player + 1 block up + width back
        
        if (world.CheckForVoxel(transform.position + Vector3.up +
            Vector3.right * width + Vector3.back * width))
        {
            Debug.Log("Right up back");
            return true;
        }
        else 
        {
            return false;
        }
    }


    // UP COLLISIONS


    public static bool upDirectly(World world, Transform transform, float width, float height)
    {
        // Directly above the player
       
        if (world.CheckForVoxel(transform.position + (Vector3.up * height)))
        {
            Debug.Log("Up directly");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upForward(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width forward
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.forward * width))
        {
            Debug.Log("Up forward");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upBack(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width back
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.back * width))
        {
            Debug.Log("Up back");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upLeft(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width left
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.left * width))
        {
            Debug.Log("Up left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upRight(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width right
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.right * width))
        {
            Debug.Log("Up right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upForwardLeft(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width left + width forward
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.left * width + Vector3.forward * width))
        {
            Debug.Log("Up forward left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upForwardRight(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width right + width forward
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.right * width + Vector3.forward * width))
        {
            Debug.Log("Up forward right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upBackLeft(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width left + width back
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.left * width + Vector3.back * width))
        {
            Debug.Log("Up back left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool upBackRight(World world, Transform transform, float width, float height)
    {
        // Directly above the player + width right + width back
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * height) +
            Vector3.right * width + Vector3.back * width))
        {
            Debug.Log("Up back right");
            return true;
        }
        else 
        {
            return false;
        }
    }


    // DOWN COLLISIONS



    public static bool downDirectly(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y)))
        {
            Debug.Log("Down directly");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downForward(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width forward
       
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.forward * width))
        {
            Debug.Log("Down forward");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downBack(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width back
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.back * width))
        {
            Debug.Log("Down back");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downLeft(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width left
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.left * width))
        {
            Debug.Log("Down left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downRight(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width right
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.right * width))
        {
            Debug.Log("Down right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downForwardLeft(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width forward + width left
       
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.forward * width + Vector3.left * width))
        {
            Debug.Log("Down forward left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downForwardRight(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width forward + width right
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.forward * width + Vector3.right * width))
        {
            Debug.Log("Down forward right");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downBackLeft(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width back + width left
        
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.back * width + Vector3.left * width))
        {
            Debug.Log("Down back left");
            return true;
        }
        else 
        {
            return false;
        }
    }

    public static bool downBackRight(World world, Transform transform, float width, Vector3 velocity)
    {
        // Directly below the player + width back + width right
        if (world.CheckForVoxel(transform.position + (Vector3.up * velocity.y) +
            Vector3.back * width + Vector3.right * width)) 
        {
            Debug.Log("Down back right");
            return true;
        }
        else 
        {
            return false;
        }
    }
}