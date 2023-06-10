using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public bool jumpComplete = true;
    public bool wasGrounded;
    public bool isGrounded;
    public bool isSprinting;
    private Transform cam;
    private World world;

    private bool creativeMode = true;

    private float airborneTime = 0f;
    public float airborneMovementDuration = 1f;

    public float forwardImpairmentMultiplier = 0.85f;
    public float walkSpeed = 4.5f;
    public float gravity = -28f;
    public float sprintSpeed = 6.5f;
    public float creativeSpeed = 10f;
    public float jumpForce = 9f;
    public float playerWidth = 0.4f;
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
        PlacePlayerAtGround();
        isGrounded = true;
        wasGrounded = true;
        jumpComplete = true;

        //Setting highlight/place block and toolbar values
        highlightBlock = GameObject.Find("HighlightBlock").transform;
        placeBlock = GameObject.Find("PlaceHighlightBlock").transform;
        toolbar = GameObject.Find("Toolbar").GetComponent<Toolbar>();

        world.inUI = false;
    }

    public void PlacePlayerAtGround()
    {
        float startY = GetHeightAtPosition(transform.position.x, transform.position.z);
        transform.position = new Vector3(transform.position.x, startY, transform.position.z);
    }

    private float GetHeightAtPosition(float x, float z)
    {
        RaycastHit hit;
        if (Physics.Raycast(new Vector3(x, 100f, z), Vector3.down, out hit, 200f, LayerMask.GetMask("Terrain")))
        {
            return hit.point.y;
        }
        return 50f; // Default height if no terrain is found
    }
    private void FixedUpdate()
    {
        
    }
    private void Update()
    {

        if (wasGrounded && !isGrounded && jumpComplete)
        {
            Debug.Log(verticalMomentum);
            airborneTime = 0f;
            verticalMomentum = 0f;
            wasGrounded = false;
        }

        //Airborne timing:
        if (isGrounded)
        {
            airborneTime = 0f;
        }
        else
        {
            airborneTime += Time.deltaTime;
        }

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
        jumpComplete = false;
        isGrounded = false;
        jumpRequest = false;
    }
    private void CalculateVelocity()
    {


        //Note: Moving in the opposite direction of the current velocity must slow down the player

        if (!creativeMode)
        {
            //Jumping
            if (verticalMomentum > gravity)
            {
                verticalMomentum += Time.deltaTime * gravity;
            }
            //Airborne affecting movement
            if (isGrounded)
            {

                // Horizontal Movement
                if (isSprinting)
                {
                    Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
                    Vector3 forwardMovement = transform.forward * inputDirection.z * Time.deltaTime * sprintSpeed;
                    Vector3 sideMovement = transform.right * inputDirection.x * Time.deltaTime * sprintSpeed;
                    velocity = forwardMovement + sideMovement + (Vector3.up * vertical * Time.deltaTime) + (Vector3.up * verticalMomentum * Time.deltaTime);
                }
                else
                {
                    Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
                    Vector3 forwardMovement = transform.forward * inputDirection.z * Time.deltaTime * walkSpeed;
                    Vector3 sideMovement = transform.right * inputDirection.x * Time.deltaTime * walkSpeed;
                    velocity = forwardMovement + sideMovement + (Vector3.up * vertical * Time.deltaTime) + (Vector3.up * verticalMomentum * Time.deltaTime);
                }
                
            }
            else
            {
                // Airborne movement
                Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
                Vector3 forwardMovement;
                Vector3 sideMovement;

                float fallingSpeed = Mathf.Min(4f, airborneTime * 4f);
                float impairment = Mathf.Min(0.15f, airborneTime * 0.15f);

                // If the player is sprinting before jumping, they will continue to sprint in the air
                if (isSprinting)
                {
                    forwardMovement = transform.forward * inputDirection.z * Time.deltaTime * sprintSpeed;
                    sideMovement = transform.right * inputDirection.x * Time.deltaTime * sprintSpeed;
                    velocity = forwardMovement + sideMovement + (Vector3.up * verticalMomentum * Time.deltaTime);
                }
                else
                {
                    forwardMovement = transform.forward * inputDirection.z * Time.deltaTime * walkSpeed;
                    sideMovement = transform.right * inputDirection.x * Time.deltaTime * walkSpeed;
                    velocity = forwardMovement + sideMovement + (Vector3.up * verticalMomentum * Time.deltaTime);
                }

                // Forward/Backward Impairment
                velocity += transform.forward * inputDirection.z * Time.deltaTime * walkSpeed * impairment;

                // Side-to-Side Impairment
                velocity += transform.right * inputDirection.x * Time.deltaTime * walkSpeed * impairment;
            }
        }
        else
        {
            velocity = transform.forward * vertical * Time.deltaTime * creativeSpeed * 2f;
            velocity += transform.right * horizontal * Time.deltaTime * creativeSpeed * 2f;
        
            if (Input.GetButton("Jump"))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.05f, transform.position.z);
            }

            if (Input.GetKey(KeyCode.LeftControl))
            {
                transform.position = new Vector3(transform.position.x, transform.position.y - 0.05f, transform.position.z);
            }
        }
        


        //Collisions:
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
        }
        else if (velocity.y > 0)
        {
            velocity.y = checkUpSpeed(velocity.y);
        }
    }
    private void GetPlayerInputs()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            creativeMode = !creativeMode;
            verticalMomentum = 0f;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");
        mouseHorizontal = Input.GetAxis("Mouse X");
        mouseVertical = Input.GetAxis("Mouse Y");
        if ((Input.GetButtonDown("Sprint") || Input.GetButton("Sprint")) && Input.GetButton("Vertical") && vertical > 0)
        {
            isSprinting = true;
        } 
        if (Input.GetButtonUp("Sprint") || !Input.GetButton("Vertical") || vertical <= 0) 
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
                wasGrounded = true;
                jumpComplete = true;
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
