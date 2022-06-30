using UnityEngine;

public class FpsController : MonoBehaviour
{
    [Header("Camera\n")]
    public Camera fpsCam;
    public float lookSensitivity = 4.5f;
    public float smoothTime = 6.5f;
    public bool invertedLook = false;
    [Header("Movement\n")]
    public Rigidbody rb;
    [Tooltip("Make sure to add physics material to your capsule collider and set its friction to 0")]
    public PhysicMaterial slipperyMaterial;
    [Tooltip("How fast player reaches max speed. Don't make this too high you might encounter some bugs")]
    public float acceleration = 10f;
    public float walkMaxSpeed = 5f;
    public float sprintMaxSpeed = 10;
    public float crouchMaxSpeed = 2;
    public float groundFriction = 7.8f;
    public bool stopInstantly = false;
    public float jumpForce = 10;
    public float stepOffset = 1f;
    public LayerMask WhatIsGround;
    [Tooltip("The Y position from where the ray is being shot to check grounded")]
    public float groundCheckStartY = 0;
    [Tooltip("How much distance the ray travels")]
    public float groundCheckDist = 1.1f;

    private Vector3 PlayerRotation;
    private Vector2 inputDir = Vector2.zero;
    float MouseX, MouseY;
    private float maxSpeed = 0;
    private bool crouching = false;
    private bool sprinting = false;
    private bool jumping = false;
    private bool canJump = true;
    private bool isGrounded = false;
    private void Start()
    {
        PlayerRotation = new Vector3(transform.localRotation.eulerAngles.x, 0, transform.localRotation.eulerAngles.z);
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        GetInput();
        // clamp velocity so we don't go over max speed
        // calling this from here because fixed update causes bugs
        ClampVelocity();
        
    }

    private void FixedUpdate()
    {
        // mouse input
        float invert = invertedLook ? -1 : 1;
        MouseX = Input.GetAxisRaw("Mouse X") * invert;
        MouseY = Input.GetAxisRaw("Mouse Y") * invert;
    
        Movement();
        MoveCam();
    }

    private void Movement()
    {
        rb.AddForce(-transform.up * 9.8f);
        // ground check
        Ray ray = new Ray(transform.position+new Vector3(0, groundCheckStartY, 0), -transform.up);
        RaycastHit hit;
        isGrounded = Physics.Raycast(ray, out hit, groundCheckDist, WhatIsGround);
        Debug.DrawRay(ray.origin, ray.direction.normalized * groundCheckDist, Color.black);

        if (inputDir.magnitude == 0 && stopInstantly && isGrounded)
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
        }
        // prevent jittery movement caused by physics material when not moving
        if (inputDir.magnitude == 0)
        {
            slipperyMaterial.frictionCombine = PhysicMaterialCombine.Average;
        }
        else slipperyMaterial.frictionCombine = PhysicMaterialCombine.Multiply;

        // move the player by adding force
        Vector3 moveDir = (transform.forward * inputDir.y + transform.right * inputDir.x).normalized * acceleration * Time.fixedDeltaTime * 10;
        rb.AddForce(moveDir, ForceMode.VelocityChange);


        // if the player is not pressing any keys apply friction
        if (inputDir.magnitude <= .2f && (int)rb.velocity.magnitude > 0 && isGrounded)
        {
            Vector3 vDir = -rb.velocity.normalized * groundFriction;
            Vector3 friction = new Vector3(vDir.x, 0, vDir.z);
            rb.AddForce(friction);
        }
        if (jumping && isGrounded && canJump)
        {
            canJump = false;
            Invoke(nameof(Jump), .1f);
        }
        StepOffset();
        Crouch();
        Sprint();
        if (!crouching && !sprinting) maxSpeed = walkMaxSpeed;
    }

    private void ClampVelocity()
    {
        Vector3 velClamped = Vector3.ClampMagnitude(rb.velocity, maxSpeed);
        rb.velocity = new Vector3(velClamped.x, rb.velocity.y, velClamped.z);
    }

    private void Crouch()
    {
        if (!sprinting && crouching && isGrounded)
        {
            rb.gameObject.transform.localScale = new Vector3(1, .5f, 1);
            maxSpeed = crouchMaxSpeed;
        }
        if (!crouching)
        {
            rb.gameObject.transform.localScale = Vector3.one;
        }
    }
    private void Sprint()
    {
        if (isGrounded && !crouching && sprinting)
        {

            maxSpeed = sprintMaxSpeed;
        }
    }

    private void StepOffset()
    {
        Vector3 vel = (transform.forward * inputDir.y + transform.right * inputDir.x).normalized;
        vel.y = 0;
        Vector3 leg = transform.position - new Vector3(0, transform.localScale.y - .15f, 0);
        Vector3 step = transform.position - new Vector3(0, transform.localScale.y - stepOffset, 0);
        Ray legRay = new Ray(leg, vel);
        Ray stepRay = new Ray(step, vel);
        Debug.DrawRay(legRay.origin, legRay.direction * 10, Color.red);
        Debug.DrawRay(stepRay.origin, stepRay.direction * 10, Color.blue);
        RaycastHit legHit;
        RaycastHit stepHit;
        float castDist = .9f;
        bool legBlocked = Physics.Raycast(legRay, out legHit, castDist, WhatIsGround);
        bool stepBlocked = Physics.Raycast(stepRay, out stepHit, castDist, WhatIsGround);
        if (legBlocked && !stepBlocked)
        {
            Rigidbody hitbody = legHit.transform.gameObject.GetComponent<Rigidbody>();
            if (hitbody != null) return;
            inputDir = Vector3.zero;
            rb.AddForce(transform.up * stepOffset / 2, ForceMode.Impulse);
        }
    }


    private void Jump()
    {
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        Invoke(nameof(ResetJump), .1f);
    }
    private void ResetJump()
    {
        canJump = true;
    }

    private void GetInput()
    {
        inputDir.x = Input.GetAxisRaw("Horizontal");
        inputDir.y = Input.GetAxisRaw("Vertical");
        if (Input.GetKey(KeyCode.Space))
        {
            jumping = true;
        }
        else jumping = false;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            sprinting = true;
        }
        else sprinting = false;
        if (Input.GetKeyDown(KeyCode.C))
        {
            crouching = !crouching && !sprinting ? true : false;
        }
    }

    Vector3 camRotation = new Vector3(0, 0, 0);
    private void MoveCam()
    {
        camRotation.x -= MouseY * lookSensitivity;

        camRotation.x = Mathf.Clamp(camRotation.x, -90, 90);
        Vector3 newCamRot = new Vector3(camRotation.x, 0, 0);
        fpsCam.transform.localRotation = Quaternion.Slerp(fpsCam.transform.localRotation, Quaternion.Euler(newCamRot.x, newCamRot.y, newCamRot.z), Time.fixedDeltaTime * smoothTime);

        PlayerRotation.y += MouseX * lookSensitivity;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(PlayerRotation), Time.fixedDeltaTime * smoothTime);
    }
}
