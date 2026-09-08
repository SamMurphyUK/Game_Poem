using UnityEngine;

public class CutscenePlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private KeyCode upKey = KeyCode.W;
    [SerializeField] private KeyCode downKey = KeyCode.S;
    [SerializeField] private KeyCode leftKey = KeyCode.A;
    [SerializeField] private KeyCode rightKey = KeyCode.D;

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

    public float GetMoveSpeed()
    {
        return moveSpeed;
    }
}