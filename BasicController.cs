using UnityEngine;

// I recommend giving a slippery physics material to player's collider for wall sliding
public class BasicController : MonoBehaviour
{
    [SerializeField] private Transform fpsCam;
    [SerializeField] private float lookSpeed = 5.0f;
    [SerializeField] private float maxSpeed = 10.0f;
    [Range(0f, 1f)]
    [SerializeField] private float headBob = 0.1f;
    [Min(1)]
    [SerializeField] private float bobFrequency = 10.0f;

    private Rigidbody rb;
    private Vector2 inputDir;
    private Vector2 mouseDelta;
    private Vector3 camInitialPos;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        camInitialPos = fpsCam.localPosition;

        Cursor.lockState = CursorLockMode.Locked;
    }

    protected void Update()
    {
        inputDir.x = Input.GetAxisRaw("Horizontal");
        inputDir.y = Input.GetAxisRaw("Vertical");
        mouseDelta.x = Input.GetAxisRaw("Mouse X");
        mouseDelta.y = -Input.GetAxisRaw("Mouse Y");

    }

    private void FixedUpdate()
    {
        Movement();
        LookAround();
    }

    private void Movement()
    {
        bool wallCollision = Physics.CheckSphere(transform.position, 0.6f, 3);

        Vector3 moveDirDesired = (transform.right * inputDir.x + transform.forward * inputDir.y).normalized;
        Vector3 rbVelocity = new Vector3(rb.velocity.x, 0, rb.velocity.z).normalized;
        Vector3 moveDir = Vector3.Reflect(-rbVelocity, moveDirDesired).normalized;
        if (Vector3.Dot(moveDirDesired, rbVelocity) < 0)
            moveDir = -rbVelocity;
        if (moveDir.magnitude < 0.05f || wallCollision)
            moveDir = moveDirDesired;
        if (inputDir.magnitude > 0)
            rb.AddForce(moveDir * (maxSpeed - rb.velocity.magnitude), ForceMode.VelocityChange);
        rb.drag = inputDir == Vector2.zero ? 5 : 0;
    }

    float xRot, yRot, off;
    float xBob, yBob;
    private void LookAround()
    {
        off += Time.deltaTime * bobFrequency;

        xRot += lookSpeed * mouseDelta.y;
        yRot += lookSpeed * mouseDelta.x;
        xRot = Mathf.Clamp(xRot, -90, 90);
        
        float xBobDesired = Mathf.Sin(off) * headBob * inputDir.magnitude;
        float yBobDesired = Mathf.Abs(Mathf.Sin(off)) * headBob * inputDir.magnitude;
        xBob = Mathf.MoveTowards(xBob, xBobDesired, Time.deltaTime * 2.0f);
        yBob = Mathf.MoveTowards(yBob, yBobDesired, Time.deltaTime * 2.0f);
        fpsCam.localPosition = new Vector3(camInitialPos.x + xBob, camInitialPos.y + yBob, camInitialPos.z);
        
        fpsCam.localEulerAngles = new Vector3 { x = xRot };
        transform.localEulerAngles = new Vector3 { y = yRot };
    }
}
