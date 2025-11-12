using UnityEngine;
using System;

public class ProyectileScript : MonoBehaviour
{
    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float resetTime = 5f;
    [SerializeField] private bool explodeOnContact = true;

    [Header("Rotation Settings")]
    [SerializeField] private bool rotateTowardsVelocity = true;
    [SerializeField] private float rotationSpeed = 15f;
    [SerializeField] private float rotationOffset = 0f;

    [Header("Trail Effect")]
    [SerializeField] private float trailTime = 0.5f;
    [SerializeField] private float trailWidthStart = 0.2f;
    [SerializeField] private float trailWidthEnd = 0.05f;
    [SerializeField] private Gradient trailGradient;

    private Camera mainCamera;
    private Rigidbody2D rb;
    private TrailRenderer trail;
    private bool hasLaunched = false;
    private bool isDestroying = false; // ⭐ NUEVO: Evita múltiples destrucciones

    public static event Action OnProyectileFinished;

    void Awake()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        mainCamera = Camera.main;

        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        SetupTrailRenderer();

        if (trail != null)
        {
            trail.emitting = false;
        }
    }

    private void SetupTrailRenderer()
    {
        trail = GetComponent<TrailRenderer>();
        if (trail == null)
        {
            trail = gameObject.AddComponent<TrailRenderer>();
        }

        trail.time = trailTime;
        trail.startWidth = trailWidthStart;
        trail.endWidth = trailWidthEnd;
        trail.material = new Material(Shader.Find("Sprites/Default"));

        if (trailGradient == null)
        {
            trailGradient = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[2];
            colorKeys[0] = new GradientColorKey(Color.white, 0f);
            colorKeys[1] = new GradientColorKey(Color.white, 1f);

            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[2];
            alphaKeys[0] = new GradientAlphaKey(1f, 0f);
            alphaKeys[1] = new GradientAlphaKey(0f, 1f);

            trailGradient.SetKeys(colorKeys, alphaKeys);
        }

        trail.colorGradient = trailGradient;
        trail.numCornerVertices = 5;
        trail.numCapVertices = 5;
        trail.sortingOrder = 4;
        trail.emitting = false;
    }

    void FixedUpdate()
    {
        if (hasLaunched && !isDestroying && rotateTowardsVelocity && rb.linearVelocity.sqrMagnitude > 0.1f)
        {
            RotateTowardsVelocity();
        }
    }

    private void RotateTowardsVelocity()
    {
        float targetAngle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
        targetAngle += rotationOffset;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.fixedDeltaTime * rotationSpeed);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    public void Activate()
    {
        if (hasLaunched)
        {
            Debug.LogWarning("⚠️ Proyectil ya fue activado previamente");
            return;
        }

        hasLaunched = true;

        if (trail != null)
        {
            trail.emitting = true;
            trail.Clear();
        }

        Invoke(nameof(Finish), resetTime);

        Debug.Log("✅ Proyectil activado");
    }

    private void Finish()
    {
        if (isDestroying) return; // ⭐ CAMBIO: Solo verificar si ya está destruyendo

        Debug.Log($"🎯 Finish() llamado - hasLaunched: {hasLaunched}, isDestroying: {isDestroying}");

        isDestroying = true;

        // Desactivar trail
        if (trail != null)
        {
            trail.emitting = false;
        }

        // Explosión
        if (explosionPrefab != null)
        {
            GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(explosion, 3f); // ⭐ Auto-destruir explosión
        }

        // Shake de cámara
        CameraMovement cam = mainCamera?.GetComponent<CameraMovement>();
        cam?.TriggerShake(0.25f, 0.12f);

        // Notificar evento
        OnProyectileFinished?.Invoke();

        Debug.Log("💥 Proyectil destruido");

        // ⭐ DESTRUCCIÓN INMEDIATA
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroying) return; // ⭐ Solo verificar si ya está destruyendo

        Debug.Log($"🔥 OnCollisionEnter2D - Objeto: {collision.gameObject.name}, hasLaunched: {hasLaunched}");

        // ⭐ EXPLOSIÓN AL CONTACTO (sin verificar hasLaunched)
        if (explodeOnContact)
        {
            CameraMovement cam = mainCamera?.GetComponent<CameraMovement>();
            float impactForce = rb.linearVelocity.magnitude;

            if (impactForce > 5f)
            {
                cam?.TriggerShake(0.2f, Mathf.Clamp(impactForce * 0.01f, 0.08f, 0.2f));
            }

            Debug.Log($"💥 Impacto con {collision.gameObject.name} - Fuerza: {impactForce}");

            // Cancelar cualquier Invoke pendiente
            CancelInvoke(nameof(Finish));

            // Ejecutar Finish inmediatamente
            Finish();
        }
        else
        {
            CameraMovement cam = mainCamera?.GetComponent<CameraMovement>();
            float impactForce = collision.relativeVelocity.magnitude;

            if (impactForce > 5f)
            {
                cam?.TriggerShake(0.15f, Mathf.Clamp(impactForce * 0.008f, 0.05f, 0.15f));
            }

            Debug.Log($"💥 Impacto con {collision.gameObject.name} - Fuerza: {impactForce}");
        }
    }

    private void OnBecameInvisible()
    {
        if (!isDestroying && hasLaunched)
        {
            Debug.Log("📺 Proyectil fuera de cámara");
            CancelInvoke(nameof(Finish));
            Invoke(nameof(Finish), 0.5f);
        }
    }

    private void OnDrawGizmos()
    {
        if (rb != null && hasLaunched)
        {
            Gizmos.color = isDestroying ? Color.red : Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.3f);

            if (rb.linearVelocity.magnitude > 0.1f)
            {
                Gizmos.DrawRay(transform.position, rb.linearVelocity.normalized * 2f);
            }
        }
        else
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 0.2f);
        }
    }
}