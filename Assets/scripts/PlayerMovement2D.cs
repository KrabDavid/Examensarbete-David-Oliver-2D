using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 8f;
    public float acceleration = 50f;
    public float deceleration = 60f;

    [Header("Visuals")]
    public SpriteRenderer spriteRenderer;

    private Rigidbody2D rb;
    private float moveInputX;
    private float currentVelocityX;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // Ensure Rigidbody2D is configured correctly for smooth 2D movement
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
    }

    void Update()
    {
        // 1. Read horizontal input from Keyboard (A/D or Left/Right Arrow keys)
        moveInputX = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                moveInputX = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                moveInputX = 1f;
            }
        }

        // 2. Flip the Sprite based on direction
        HandleSpriteFlip();
    }

    void FixedUpdate()
    {
        // 3. Smooth acceleration/deceleration physics
        float targetVelocityX = moveInputX * moveSpeed;
        float rate = (Mathf.Abs(targetVelocityX) > 0.01f) ? acceleration : deceleration;

        currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(currentVelocityX, 0f);
    }

    private void HandleSpriteFlip()
    {
        if (spriteRenderer == null) return;

        if (moveInputX > 0f)
        {
            spriteRenderer.flipX = false; // Facing Right
        }
        else if (moveInputX < 0f)
        {
            spriteRenderer.flipX = true;  // Facing Left
        }
    }
}