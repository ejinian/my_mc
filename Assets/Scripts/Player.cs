using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool isGrounded;
    public bool isSprinting;
    private Transform cam;
    private World world;

    public float walkSpeed = 3f;
    public float gravity = -13f;
    public float sprintSpeed = 6f;
    public float jumpForce = 5.5f;
    public float playerWidth = 0.15f;
    //public float boundsTolerance;
    private float horizontal;
    private float vertical;
    private float mouseHorizontal;
    private float mouseVertical;
    private Vector3 velocity;
    private float verticalMomentum = 0;
    private bool jumpRequest;

    public Transform highlightBlock;
    public Transform placeBlock;
    public float checkIncrement = 0.1f;
    public float reach = 8f;

    public Toolbar toolbar;

    private void Start()
    {
        cam = GameObject.Find("Main Camera").transform;
        world = GameObject.Find("World").GetComponent<World>();

        Cursor.lockState = CursorLockMode.Locked;
    }
    private void FixedUpdate()
    {
        
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            world.inUI = !world.inUI;
        }
        if (Input.GetKeyDown(KeyCode.Escape)) {
            world.inUI = false;
        }
        if (!world.inUI)
        {
            GetPlayerInputs();
            placeCursorBlocks();
            CalculateVelocity();
            if (jumpRequest)
            {
                Jump();
            }
            transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSensitivity);
            cam.Rotate(Vector3.right * -mouseVertical * world.settings.mouseSensitivity);
            transform.Translate(velocity, Space.World);
        }
        //if (!world.inUI) ZA WARUDO EFFECT
        //{
        //    GetPlayerInputs();
        //    placeCursorBlocks();
        //    CalculateVelocity();
        //    if (jumpRequest)
        //    {
        //        Jump();
        //    }
        //    transform.Rotate(Vector3.up * mouseHorizontal);
        //    cam.Rotate(Vector3.right * -mouseVertical);
        //    transform.Translate(velocity, Space.World);
        //}
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
        if (isGrounded && Input.GetButton("Jump"))
        {
            jumpRequest = true;
        }
        if (highlightBlock.gameObject.activeSelf)
        {
            // destroy block
            if (Input.GetMouseButtonDown(0))
            {
                world.GetChunkFromVector3(highlightBlock.position).EditVoxel(highlightBlock.position, 0);
                // play a sound
                
            }
            // place block
            if (Input.GetMouseButtonDown(1))
            {
                if (toolbar.slots[toolbar.slotIndex].HasItem)
                {
                    world.GetChunkFromVector3(placeBlock.position)
                        .EditVoxel(placeBlock.position, toolbar.slots[toolbar.slotIndex].itemSlot.stack.id);
                    toolbar.slots[toolbar.slotIndex].itemSlot.Take(1);
                }
            }
        }
    }
    private void placeCursorBlocks()
    {
        float step = checkIncrement;
        Vector3 lastPos = new Vector3();
        // pseudo ray
        while (step < reach)
        {
            Vector3 pos = cam.position + (cam.forward * step);
            if (world.CheckForVoxel(pos))
            {
                highlightBlock.position = new Vector3(Mathf.FloorToInt(pos.x), 
                    Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
                placeBlock.position = lastPos;
                highlightBlock.gameObject.SetActive(true);
                placeBlock.gameObject.SetActive(true);
                return;
            }
            lastPos = new Vector3(Mathf.FloorToInt(pos.x),
                    Mathf.FloorToInt(pos.y), Mathf.FloorToInt(pos.z));
            step += checkIncrement;
        }
        highlightBlock.gameObject.SetActive(false);
        placeBlock.gameObject.SetActive(false);
    }
    private float checkDownSpeed(float downSpeed)
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed,
            transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed,
            transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + downSpeed,
            transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + downSpeed,
            transform.position.z + playerWidth)))
        {
            // there is a solid voxel under the player
            // check if player is on ground
            if (world.CheckForVoxel(new Vector3(transform.position.x, transform.position.y + downSpeed,
                transform.position.z)))
            {
                // player is on ground
                isGrounded = true;
                return 0;
            }
            else
            {
                // player is not on ground
                isGrounded = false;
                return downSpeed;
            }
            //isGrounded = true;
            //return 0;
        }
        else
        {
            isGrounded = false;
            return downSpeed;
        }
    }
    private float checkUpSpeed(float upSpeed)
    {
        if (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z - playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x + playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z + playerWidth)) ||
            world.CheckForVoxel(new Vector3(transform.position.x - playerWidth, transform.position.y + upSpeed + 2f,
            transform.position.z + playerWidth)))
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
            return (world.CheckForVoxel(new Vector3(transform.position.x, 
                transform.position.y, transform.position.z + playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x,
                transform.position.y + 1f, transform.position.z + playerWidth)));
        }
    }
    public bool back
    {
        get
        {
            return (world.CheckForVoxel(new Vector3(transform.position.x,
                transform.position.y, transform.position.z - playerWidth)) ||
                world.CheckForVoxel(new Vector3(transform.position.x,
                transform.position.y + 1f, transform.position.z - playerWidth)));
        }
    }
    public bool left
    {
        get
        {
            return (world.CheckForVoxel(new Vector3(transform.position.x - playerWidth,
                transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x - playerWidth,
                transform.position.y + 1f, transform.position.z)));
        }
    }
    public bool right
    {
        get
        {
            return (world.CheckForVoxel(new Vector3(transform.position.x + playerWidth,
                transform.position.y, transform.position.z)) ||
                world.CheckForVoxel(new Vector3(transform.position.x + playerWidth,
                transform.position.y + 1f, transform.position.z)));
        }
    }
}
