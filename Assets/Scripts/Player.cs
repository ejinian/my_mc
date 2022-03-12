using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;
    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float gravity = -9.8f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5f;
    public float playerWidth = 0.15f;
    //public float boundsTolerance;

    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();
    }
    private void FixedUpdate()
    {
        
    }
    private void Update()
    {
        GetPlayerInputs();
        CalculateVelocity();
        if (jumpRequest)
        {
            Jump();
        }
        transform.Rotate(Vector3.up * mouseHorizontal);
        cam.Rotate(Vector3.right * -mouseVertical);
        transform.Translate(velocity, Space.World);
    }
    void Jump()
    {
        verticalMomentum = jumpForce;
        isGrounded = false;
        jumpRequest = false;
    }
    private void CalculateVelocity()
    {
        if (verticalMomentum > gravity)
        {
            verticalMomentum += Time.deltaTime * gravity;
        }
        if (isSprinting)
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal))
                * Time.deltaTime * sprintSpeed;
        }
        else
        {
            velocity = ((transform.forward * vertical) + (transform.right * horizontal))
                * Time.deltaTime * walkSpeed;
        }
        velocity += Vector3.up * verticalMomentum * Time.deltaTime;
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
        {
            velocity.z = 0;
        }
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
        {
            velocity.x = 0;
        }
        if ((velocity.y < 0))
        {
            velocity.y = checkDownSpeed(velocity.y);
        }else if (velocity.y > 0)
        {
            velocity.y = checkUpSpeed(velocity.y);
        }
    }
    private void GetPlayerInputs()
    {
        horizontal = Input.GetAxis("Horizontal");
        vertical = Input.GetAxis("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");

        if (Input.GetButtonDown("Sprint"))
        {
            isSprinting = true;
        }
        if (Input.GetButtonUp("Sprint"))
        {
            isSprinting = false;
        }
        if (isGrounded && Input.GetButtonDown("Jump"))
        {
            jumpRequest = true;
        }
    }
    private float checkDownSpeed(float downSpeed)
    {
        if (world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed,
            transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed,
            transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + downSpeed,
            transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + downSpeed,
            transform.position.z + playerWidth))
        {
            // there is a solid voxel under the player
            isGrounded = true;
            return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }
    private float checkUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z - playerWidth) ||
            world.CheckForVoxel(transform.position.x + playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z + playerWidth) ||
            world.CheckForVoxel(transform.position.x - playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z + playerWidth))
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }

    public bool front
    {
        get
        {
            return (world.CheckForVoxel(transform.position.x, 
                transform.position.y, transform.position.z + playerWidth) ||
                world.CheckForVoxel(transform.position.x,
                transform.position.y + 1f, transform.position.z + playerWidth));
        }
    }
    public bool back
    {
        get
        {
            return (world.CheckForVoxel(transform.position.x,
                transform.position.y, transform.position.z - playerWidth) ||
                world.CheckForVoxel(transform.position.x,
                transform.position.y + 1f, transform.position.z - playerWidth));
        }
    }
    public bool left
    {
        get
        {
            return (world.CheckForVoxel(transform.position.x - playerWidth,
                transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x - playerWidth,
                transform.position.y + 1f, transform.position.z));
        }
    }
    public bool right
    {
        get
        {
            return (world.CheckForVoxel(transform.position.x + playerWidth,
                transform.position.y, transform.position.z) ||
                world.CheckForVoxel(transform.position.x + playerWidth,
                transform.position.y + 1f, transform.position.z));
        }
    }
}
