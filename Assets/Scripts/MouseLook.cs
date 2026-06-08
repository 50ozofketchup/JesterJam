using UnityEngine;


public class MouseLook : MonoBehaviour
{
    [Header("Sensitivity")]
    [SerializeField] private float sensitivityX = 400f;
    [SerializeField] private float sensitivityY = 400f;

    [Header("References")]
    [SerializeField] private Transform playerBody;

    private float _xRotation = 0f;

    private void Start()
    {
        // Lock and hide the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void ResetRotation()
    {
        _xRotation = 0f;
    }
    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * sensitivityX * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * sensitivityY * Time.deltaTime;

        // Vertical look (camera only) — clamp so you can't flip upside down
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);
        transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);

        // Horizontal look — rotate the whole player body
        playerBody.Rotate(Vector3.up * mouseX);
    }
}