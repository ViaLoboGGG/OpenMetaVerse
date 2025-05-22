using UnityEngine;
using UnityEngine.XR;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonVRController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float gravity = -9.81f;
    public float jumpHeight = 1.5f;

    [Header("VR Settings")]
    public bool useVR = false;
    public Transform xrOrigin; // Reference to XR Origin or camera
    public Transform vrHead;   // Main Camera / Head
    public Transform desktopCamera; // Fallback camera for non-VR

    private CharacterController controller;
    private float verticalVelocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (!useVR)
        {
            // Lock mouse for desktop mode
            Cursor.lockState = CursorLockMode.Locked;
            if (desktopCamera == null)
            {
                GameObject fallback = new GameObject("DesktopCamera");
                fallback.transform.SetParent(transform);
                fallback.transform.localPosition = new Vector3(0, 0.9f, 0);
                fallback.AddComponent<Camera>();
                desktopCamera = fallback.transform;
            }
        }
    }

    void Update()
    {
        HandleMovement();

        if (!useVR)
        {
            HandleMouseLook();
        }
    }

    void HandleMovement()
    {
        Vector3 inputDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

        Vector3 move = inputDirection;

        if (useVR && vrHead != null)
        {
            // Move based on head forward in VR
            Vector3 headForward = vrHead.forward;
            headForward.y = 0;
            headForward.Normalize();
            move = headForward * inputDirection.z + vrHead.right * inputDirection.x;
        }
        else
        {
            move = transform.right * inputDirection.x + transform.forward * inputDirection.z;
        }

        if (controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;
        move.y = verticalVelocity;

        controller.Move(move * moveSpeed * Time.deltaTime);
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * 2f;
        float mouseY = Input.GetAxis("Mouse Y") * 2f;

        Vector3 euler = desktopCamera.localEulerAngles;
        euler.x -= mouseY;
        euler.y += mouseX;
        euler.x = Mathf.Clamp(euler.x, -80f, 80f);
        desktopCamera.localEulerAngles = new Vector3(euler.x, 0f, 0f);
        transform.Rotate(0f, mouseX, 0f);
    }
}
