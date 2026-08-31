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
    public ParticleSystem dust;

    [Header("Walk Animation")]
    public Sprite walkFrame1;
    public Sprite walkFrame2;
    public float animFrameRate = 0.15f; // Seconds per frame

    private Rigidbody2D rb;
    private float moveInputX;
    private float lastInputX;
    private float currentVelocityX;

    // Animation Timer Variables
    private float animTimer;
    private bool useFirstFrame = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
    }

    void Update()
    {
        // 1. Read horizontal input
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

        // 2. Play dust effect when changing direction
        HandleDustEffect();

        // 3. Flip the Sprite based on direction
        HandleSpriteFlip();

        // 4. Handle 2-Frame Walk Animation
        HandleWalkAnimation();

        // Store current input for comparison next frame
        lastInputX = moveInputX;
    }

    void FixedUpdate()
    {
        float targetVelocityX = moveInputX * moveSpeed;
        float rate = (Mathf.Abs(targetVelocityX) > 0.01f) ? acceleration : deceleration;

        currentVelocityX = Mathf.MoveTowards(currentVelocityX, targetVelocityX, rate * Time.fixedDeltaTime);
        rb.linearVelocity = new Vector2(currentVelocityX, 0f);
    }

    private void HandleWalkAnimation()
    {
        if (spriteRenderer == null || walkFrame1 == null || walkFrame2 == null) return;

        // Only animate if the player is actively pressing a move key
        if (moveInputX != 0f)
        {
            animTimer += Time.deltaTime;

            if (animTimer >= animFrameRate)
            {
                animTimer = 0f;
                useFirstFrame = !useFirstFrame;
                spriteRenderer.sprite = useFirstFrame ? walkFrame1 : walkFrame2;
            }
        }
        else
        {
            // Reset back to idle frame when stopped
            animTimer = 0f;
            useFirstFrame = true;
            spriteRenderer.sprite = walkFrame1;
        }
    }

    private void HandleDustEffect()
    {
        if (dust == null) return;

        bool changedDirection = (moveInputX != 0f) && (moveInputX != lastInputX);

        if (changedDirection)
        {
            dust.Stop();
            dust.Play();
        }
    }

    private void HandleSpriteFlip()
    {
        if (spriteRenderer == null) return;

        if (moveInputX > 0f)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveInputX < 0f)
        {
            spriteRenderer.flipX = true;
        }
    }
}