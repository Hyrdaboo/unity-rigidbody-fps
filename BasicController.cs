using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BasicController : MonoBehaviour
{
    [SerializeField] private float maxSpeed = 8.0f;
    [SerializeField] private float acceleration = 12.0f;
    [SerializeField] private float gravity = 9.81f;
    [SerializeField] private float jumpForce = 5.0f;
    [SerializeField] private Camera fpsCam;
    [SerializeField] private float lookSpeed = 5.0f;

    private Rigidbody rb;
    private Vector3 inputDir;

    private Vector3 FlatVelocity => new Vector3(rb.velocity.x, 0, rb.velocity.z);

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.useGravity = false;
    }

    float xRot, yRot;
    private void Update()
    {
        inputDir.x = Input.GetAxisRaw("Horizontal");
        inputDir.z = Input.GetAxisRaw("Vertical");
        inputDir.Normalize();

        // Camera Look
        Vector2 mouseDelta;
        mouseDelta.x = Input.GetAxisRaw("Mouse X");
        mouseDelta.y = -Input.GetAxisRaw("Mouse Y");
        xRot += lookSpeed * mouseDelta.y;
        yRot += lookSpeed * mouseDelta.x;
        xRot = Mathf.Clamp(xRot, -90, 90);
        fpsCam.transform.rotation = Quaternion.Euler(xRot, yRot, 0);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(Vector3.down * Mathf.Abs(gravity));

        Vector3 moveDirDesired = Quaternion.Euler(0, fpsCam.transform.eulerAngles.y, 0) * inputDir;
        moveDirDesired.Normalize();


        if (isGrounded)
        {
            Vector3 force = moveDirDesired * acceleration - acceleration / maxSpeed * FlatVelocity;
            rb.AddForce(force);
        }
        else
        {
            Vector3 airForce = moveDirDesired * maxSpeed - Vector3.ClampMagnitude(FlatVelocity, maxSpeed);
            rb.AddForce(airForce);
        }
    }

    private bool isGrounded, groundDetectedThisFrame;
    private void OnCollisionStay(Collision collision)
    {
        for (int i = 0; i < collision.contactCount; i++)
        {
            // ideally we should use slope angular limit here
            if (Vector3.Dot(Vector3.up, collision.contacts[i].normal) > 0.5f)
            {
                isGrounded = true;
                groundDetectedThisFrame = true;
                break;
            }
        }
    }

    private void LateUpdate()
    {
        if (!groundDetectedThisFrame)
            isGrounded = false;
        groundDetectedThisFrame = false;
    }
}
