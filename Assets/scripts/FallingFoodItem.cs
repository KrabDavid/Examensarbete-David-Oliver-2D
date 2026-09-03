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

        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
    }

    void Update()
    {
        transform.position += Vector3.down * currentFallSpeed * Time.deltaTime;
        transform.Rotate(0f, 0f, currentRotationSpeed * Time.deltaTime);

        // Item fell past player (Missed!)
        if (transform.position.y <= destroyYThreshold)
        {
            if (AdGameScoreManager.Instance != null)
            {
                AdGameScoreManager.Instance.ResetStreak();
            }

            // Trigger screen shake
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.TriggerShake(0.15f, 0.2f);
            }

            // Trigger miss SFX
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayMiss();
            }

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Collided with object: " + other.name);

        if (other.CompareTag("Player"))
        {
            if (AdGameScoreManager.Instance != null)
            {
                AdGameScoreManager.Instance.AddPoints(foodType);
            }

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayPickUp();
            }

            Destroy(gameObject);
        }
    }
}