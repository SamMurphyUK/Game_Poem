using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private KeyCode upKey;
    [SerializeField] private KeyCode downKey;
    [SerializeField] private KeyCode leftKey;
    [SerializeField] private KeyCode rightKey;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        moveInput = Vector2.zero;

        if (Input.GetKey(upKey)) moveInput.y += 1f;
        if (Input.GetKey(downKey)) moveInput.y -= 1f;
        if (Input.GetKey(rightKey)) moveInput.x += 1f;
        if (Input.GetKey(leftKey)) moveInput.x -= 1f;

        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveInput * moveSpeed;
    }

    //private void FixedUpdate()
    //{
    //    // Continuously move the Rigidbody2D based on the held direction
    //    rb.linearVelocity = moveInput * moveSpeed;
    //}

    //public void Move(InputAction.CallbackContext context)
    //{
    //    moveInput = context.ReadValue<Vector2>();
    //}
}
