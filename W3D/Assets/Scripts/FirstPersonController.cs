using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 2f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private Transform cameraTransform;
    private float verticalVelocity;
    private float xRotation = 0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        // Create a simple camera child if none exists
        if (transform.Find("Camera") == null)
        {
            GameObject camObj = new GameObject("Camera");
            camObj.transform.SetParent(transform);
            camObj.transform.localPosition = new Vector3(0, 0.9f, 0); // eye height
            cameraTransform = camObj.transform;
            camObj.AddComponent<Camera>();
        }
        else
        {
            cameraTransform = transform.Find("Camera");
        }

        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleMouseLook();
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraTransform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        Vector3 move = transform.right * Input.GetAxis("Horizontal") +
                       transform.forward * Input.GetAxis("Vertical");

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }
}
