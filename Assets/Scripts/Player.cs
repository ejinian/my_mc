using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public float holdingSpaceTime = 0f;
    public float interpolatedValue = 0;
    public float maxPlayerSpeed = 25f;
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
    public float walkSpeed = 4.317f;
    public float gravity = -28f;
    public float sprintSpeed = 5.612f;
    public float creativeSpeed = 10f;
    public float jumpForce = 8.4f;
    public float playerWidth = 5/16f;
    public float playerHeight = 1.8f;
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

        Mathf.Clamp(velocity.x, -maxPlayerSpeed, maxPlayerSpeed);
        Mathf.Clamp(velocity.y, -maxPlayerSpeed, maxPlayerSpeed);
        Mathf.Clamp(velocity.z, -maxPlayerSpeed, maxPlayerSpeed);

        world.inUI = false;
    }

    IEnumerator InterpolateOverTime(float startValue, float targetValue, float duration)
    {
        float elapsedTime = 0f; // The time elapsed since the interpolation started

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration; // Calculate the normalized weight based on elapsed time and duration
            interpolatedValue = Mathf.Lerp(startValue, targetValue, t); // Perform the interpolation

            elapsedTime += Time.deltaTime; // Update the elapsed time using the frame time

            yield return null; // Yield the interpolated value directly
        }

        interpolatedValue = targetValue; // Ensure the interpolation ends at the exact value        
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
        CalculateVelocity();
        CollisionDetection();
    }
    private void Update()
    {

        //Check if player is holding down spacebar without having released it
        if (Input.GetKey(KeyCode.Space))
        {
            holdingSpaceTime += Time.deltaTime;
        }
        else if (Input.GetKeyUp(KeyCode.Space))
        {
            holdingSpaceTime = 0f;
        }

        if (wasGrounded && !isGrounded && jumpComplete)
        {
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
            
            if (jumpRequest)
            {
                Jump();
            }
            transform.Rotate(Vector3.up * mouseHorizontal * world.settings.mouseSensitivity);
            RotateCamera();
            transform.Translate(velocity, Space.World);
        }
    }

    private void RotateCamera()
    {
        float rotationAmount = -mouseVertical * world.settings.mouseSensitivity;
        float xRotation = cam.transform.localEulerAngles.x;

        xRotation += rotationAmount;
        xRotation = ClampRotation(xRotation);

        cam.transform.localEulerAngles = new Vector3(xRotation, 0f, 0f);
    }

    private float ClampRotation(float rotation)
    {
        if (rotation > 180f)
        {
            rotation -= 360f;
        }

        rotation = Mathf.Clamp(rotation, -89f, 89f);
        return rotation;
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
            //Jumping or falling
            if ((verticalMomentum > gravity) || !isGrounded)
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
                    velocity = forwardMovement + sideMovement;
                }
                else
                {
                    Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
                    Vector3 forwardMovement = transform.forward * inputDirection.z * Time.deltaTime * walkSpeed;
                    Vector3 sideMovement = transform.right * inputDirection.x * Time.deltaTime * walkSpeed;
                    velocity = forwardMovement + sideMovement;
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
    }

    // Note: Not counting as grounded constantly when on the ground.
    // Note: Might have to only count a collision if the player is not already colliding, i.e. if the player is colliding with a block directly in front of them then don't count the same black as a left or right collision.

    private void CollisionDetection() {
        if (((Collisions.frontDirectly(world, transform, playerWidth) || Collisions.frontLeft(world, transform, playerWidth) || Collisions.frontRight(world, transform, playerWidth) || Collisions.frontUp(world, transform, playerWidth) || Collisions.frontUpLeft(world, transform, playerWidth) || Collisions.frontUpRight(world, transform, playerWidth))))
        {
            //v > 0 means player is moving forward
            //Debug.Log("Front Collision z velocity: " + velocity.z);
            velocity.z = 0;
            velocity.z = Mathf.Clamp(velocity.z, -maxPlayerSpeed, 0);
        } else 
        {
            velocity.z = Mathf.Clamp(velocity.z, -maxPlayerSpeed, maxPlayerSpeed);
        }
        if ((Collisions.backDirectly(world, transform, playerWidth) || Collisions.backLeft(world, transform, playerWidth) || Collisions.backRight(world, transform, playerWidth) || Collisions.backUp(world, transform, playerWidth) || Collisions.backUpLeft(world, transform, playerWidth) || Collisions.backUpRight(world, transform, playerWidth))) {
            //v < 0 means player is moving backward
            velocity.z = 0;
            velocity.z = Mathf.Clamp(velocity.z, 0, maxPlayerSpeed);
        } 
        else 
        {
            velocity.z = Mathf.Clamp(velocity.z, -maxPlayerSpeed, maxPlayerSpeed);
        }
        if (((Collisions.rightDirectly(world, transform, playerWidth) || Collisions.rightForward(world, transform, playerWidth) || Collisions.rightBack(world, transform, playerWidth) || Collisions.rightUp(world, transform, playerWidth) || Collisions.rightUpForward(world, transform, playerWidth) || Collisions.rightUpBack(world, transform, playerWidth))))
        {
            //v > 0 means player is moving right
            velocity.x = 0;
            velocity.x = Mathf.Clamp(velocity.x, -maxPlayerSpeed, 0);
        } 
        else 
        {
            velocity.x = Mathf.Clamp(velocity.x, -maxPlayerSpeed, maxPlayerSpeed);
        }
        if (((Collisions.leftDirectly(world, transform, playerWidth) || Collisions.leftForward(world, transform, playerWidth) || Collisions.leftBack(world, transform, playerWidth) || Collisions.leftUp(world, transform, playerWidth) || Collisions.leftUpForward(world, transform, playerWidth) || Collisions.leftUpBack(world, transform, playerWidth))))
        {
            //v < 0 means player is moving left
            velocity.x = 0;
            velocity.x = Mathf.Clamp(velocity.x, 0, maxPlayerSpeed);
        }
        else 
        {
            velocity.x = Mathf.Clamp(velocity.x, -maxPlayerSpeed, maxPlayerSpeed);
        } 
        if (Collisions.downDirectly(world, transform, playerWidth, velocity) || Collisions.downForward(world, transform, playerWidth, velocity) || Collisions.downBack(world, transform, playerWidth, velocity) || Collisions.downRight(world, transform, playerWidth, velocity) || Collisions.downLeft(world, transform, playerWidth, velocity) || Collisions.downForwardRight(world, transform, playerWidth, velocity) || Collisions.downForwardLeft(world, transform, playerWidth, velocity) || Collisions.downBackRight(world, transform, playerWidth, velocity) || Collisions.downBackLeft(world, transform, playerWidth, velocity))
        {
            isGrounded = true;
            wasGrounded = true;
            jumpComplete = true;
            velocity.y = 0;
            velocity.y = Mathf.Clamp(velocity.y, 0, maxPlayerSpeed);
            verticalMomentum = 0f;
            Debug.Log("Grounded");
        }
        else
        {
            Debug.Log("Not Grounded"); //This appears when it should absolutely not appear
            velocity.y = Mathf.Clamp(velocity.y, -maxPlayerSpeed, maxPlayerSpeed);
            isGrounded = false;
        }
        if (Collisions.upDirectly(world, transform, playerWidth, playerHeight) || Collisions.upForward(world, transform, playerWidth, playerHeight) || Collisions.upBack(world, transform, playerWidth, playerHeight) || Collisions.upRight(world, transform, playerWidth, playerHeight) || Collisions.upLeft(world, transform, playerWidth, playerHeight) || Collisions.upForwardRight(world, transform, playerWidth, playerHeight) || Collisions.upForwardLeft(world, transform, playerWidth, playerHeight) || Collisions.upBackRight(world, transform, playerWidth, playerHeight) || Collisions.upBackLeft(world, transform, playerWidth, playerHeight)) {
            verticalMomentum = 0f;
            velocity.y = 0;
            velocity.y = Mathf.Clamp(velocity.y, -maxPlayerSpeed, 0);
        }
        else 
        {
            velocity.y = Mathf.Clamp(velocity.y, -maxPlayerSpeed, maxPlayerSpeed);
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
        if (isGrounded && holdingSpaceTime > 0.3f)
        {
            if (interpolatedValue == 0f) {
                jumpRequest = true;
                interpolatedValue = 1f;
                StartCoroutine(InterpolateOverTime(1f, 0f, 0.5f));
            }

        }
        else if (isGrounded && Input.GetButtonDown("Jump"))
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

    // Note: Make it so that if the player is up against a wall and is trying to move forward, the movement is turned into a strafe depending on the
    // direction the player is facing

    // Note: Movement speed should be tied to angle of the player's view in relation to the block they are trying to move into. If the player is looking
    // directly at the block, they should move at full speed sideways. If the player is looking at a block at a 45 degree angle, they should move at half speed
    // sideways in the direction of the block.

}