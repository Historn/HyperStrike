using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public float movementSpeed = 5f;
    public float verticalSpeed = 5f;

    private float pitch = 0f;
    private float yaw = 0f;
    private bool canMove = true;

    void Update()
    {
        // If input is disabled, skip the camera movement
        if (!canMove)
        {
            return;
        }

        // Check if the mouse is over a UI element before processing input
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        // Mouse look
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -90f, 90f);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // WASD movement
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 moveDirection = transform.right * moveX + transform.forward * moveZ;
        transform.position += moveDirection * movementSpeed * Time.deltaTime;

        // Vertical movement with Shift (down) and Space (up)
        if (Input.GetKey(KeyCode.Space))
        {
            transform.position += Vector3.up * verticalSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            transform.position += Vector3.down * verticalSpeed * Time.deltaTime;
        }
    }

    public void DisableInput()
    {
        canMove = false;
    }

    public void EnableInput()
    {
        canMove = true;
    }
}
