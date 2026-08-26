using UnityEngine;

public class FallingFoodItem : MonoBehaviour
{
    public enum FoodType { Burger, Fries }

    [Header("Item Type")]
    public FoodType foodType;

    [Header("Speed Settings")]
    public float minFallSpeed = 2f;
    public float maxFallSpeed = 4.5f;

    [Header("Rotation Settings")]
    public float minRotationSpeed = -90f;
    public float maxRotationSpeed = 90f;

    [Header("Cleanup Boundary")]
    public float destroyYThreshold = -6f;

    private float currentFallSpeed;
    private float currentRotationSpeed;

    void Start()
    {
        currentFallSpeed = Random.Range(minFallSpeed, maxFallSpeed);
        currentRotationSpeed = Random.Range(minRotationSpeed, maxRotationSpeed);

        // Ensure Z position is forced to 0 on spawn
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    void Update()
    {
        // 1. Move down on Y axis (keeping Z locked at 0)
        transform.position += Vector3.down * currentFallSpeed * Time.deltaTime;

        // 2. Rotate item visually
        transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);

        // 3. Fall off bottom of screen cleanup
        if (transform.position.y <= destroyYThreshold)
        {
            if (AdGameScoreManager.Instance != null)
            {
                AdGameScoreManager.Instance.ResetStreak();
            }
            Destroy(gameObject);
        }
    }

    // Trigger check for Player
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Debug check to verify collision triggers in Console
        Debug.Log("Collided with object: " + other.name);

        if (other.CompareTag("Player"))
        {
            if (AdGameScoreManager.Instance != null)
            {
                AdGameScoreManager.Instance.AddPoints(foodType);
            }

            // Immediately destroy item upon catching
            Destroy(gameObject);
        }
    }
}