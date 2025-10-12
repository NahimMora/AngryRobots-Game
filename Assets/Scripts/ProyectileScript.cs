using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class ProyectileScript : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab; // ⭐ Cada proyectil puede tener su explosión
    [SerializeField] private float force;
    [SerializeField] private float maxDistance;
    [SerializeField] private float resetTime;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private Vector2 startPosition, clampPosition;

    public static event Action OnProyectileFinished;

    private void Finish()
    {
        // ⭐ Spawner explosión antes de destruirse
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        OnProyectileFinished?.Invoke();

        if (mainCamera != null)
        {
            CameraMovement cam = mainCamera.GetComponent<CameraMovement>();
            if (cam != null)
            {
                cam.ResetPositionCamera();
            }
        }

        Destroy(gameObject);
    }

    void Start()
    {
        mainCamera = Camera.main;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.gravityScale = 0f;
        startPosition = transform.position;
    }

    void Update()
    {
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 dragPosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            clampPosition = dragPosition;
            float dragDistance = Vector2.Distance(startPosition, dragPosition);

            if (dragDistance > maxDistance)
            {
                clampPosition = startPosition + (dragPosition - startPosition).normalized * maxDistance;
            }

            transform.position = clampPosition;
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 1f;
            Vector2 thowVector = startPosition - clampPosition;
            rb.AddForce(thowVector * force);
            Invoke("Finish", 5f);
        }
    }
}