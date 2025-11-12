using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RobotScript))]
public class RobotFlyScript : MonoBehaviour
{
    [Header("Movimiento flotante")]
    [SerializeField] private float floatAmplitude = 0.25f;
    [SerializeField] private float floatSpeed = 2f;

    [Header("Rotación suave")]
    [SerializeField] private float rotationAmplitude = 5f;
    [SerializeField] private float rotationSpeed = 1.5f;

    [Header("Caída al suelo")]
    [SerializeField] private float fallGravity = 2f;

    private Vector3 startPos;
    private float randomOffset;
    private float randomRotOffset;
    private bool isFalling = false;
    private bool hasExploded = false; // Evitar múltiples explosiones
    private RobotScript robot;
    private Rigidbody2D rb;

    void Start()
    {
        startPos = transform.position;
        randomOffset = Random.Range(0f, 2f * Mathf.PI);
        randomRotOffset = Random.Range(0f, 2f * Mathf.PI);

        robot = GetComponent<RobotScript>();
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0;

        if (robot != null)
        {
            robot.OnHealthChanged += HandleHealthChanged;
        }
    }

    void OnDestroy()
    {
        if (robot != null)
            robot.OnHealthChanged -= HandleHealthChanged;
    }

    void HandleHealthChanged(float healthPercent)
    {
        if (!isFalling && healthPercent <= 0.25f)
        {
            StartFalling();
        }
    }

    void StartFalling()
    {
        isFalling = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = fallGravity;

        Debug.Log("🤖 Robot comenzando a caer...");
    }

    void Update()
    {
        if (!isFalling)
        {
            float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed + randomOffset) * floatAmplitude;
            float rotZ = Mathf.Sin(Time.time * rotationSpeed + randomRotOffset) * rotationAmplitude;

            transform.position = new Vector3(startPos.x, newY, startPos.z);
            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
        else
        {
            float rotZ = Mathf.Sin(Time.time * rotationSpeed * 2f + randomRotOffset) * rotationAmplitude;
            transform.rotation = Quaternion.Euler(0, 0, rotZ);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!isFalling || hasExploded) return;

        // Detecta suelo por Layer o por componente Tilemap
        bool isGround = collision.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                        collision.gameObject.GetComponent<UnityEngine.Tilemaps.Tilemap>() != null;

        if (isGround)
        {
            Debug.Log("💥 Robot tocó el suelo - ¡EXPLOSIÓN!");
            ExplodeOnGround();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isFalling || hasExploded) return;

        bool isGround = other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
                        other.gameObject.GetComponent<UnityEngine.Tilemaps.Tilemap>() != null;

        if (isGround)
        {
            Debug.Log("💥 Robot tocó el suelo (trigger) - ¡EXPLOSIÓN!");
            ExplodeOnGround();
        }
    }

    private void ExplodeOnGround()
    {
        if (hasExploded) return;

        hasExploded = true;

        if (robot != null)
        {
            robot.TakeDamage(9999f);
        }
    }
}