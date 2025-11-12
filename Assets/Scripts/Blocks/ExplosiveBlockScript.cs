using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BlockScript))]
public class ExplosiveBehaviorFixed : MonoBehaviour
{
    [Header("Explosion Settings")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float maxDamage = 100f;
    [SerializeField] private float maxForce = 8f;
    [SerializeField] private float preExplosionTime = 1.2f;
    [SerializeField] private LayerMask damageLayers;
    [SerializeField] private GameObject explosionParticles;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float explosionDuration = 1f; // ⭐ NUEVO

    private BlockScript block;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private bool isExploding = false;
    private float lastHealth;

    private void Awake()
    {
        block = GetComponent<BlockScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
    }

    private void Start()
    {
        lastHealth = GetCurrentHealth();
        Debug.Log($"⭐ ExplosiveBehavior iniciado. Salud inicial: {lastHealth}");
    }

    private void Update()
    {
        float currentHealth = GetCurrentHealth();

        // Debug cada vez que cambie la salud
        if (currentHealth != lastHealth)
        {
            Debug.Log($"🔄 Salud cambió: {lastHealth} → {currentHealth}");
        }

        if (!isExploding && currentHealth <= 0 && lastHealth > 0)
        {
            Debug.Log("🔥 Iniciando secuencia de explosión!");
            StartCoroutine(ExplodeSequence());
        }

        lastHealth = currentHealth;
    }

    private float GetCurrentHealth()
    {
        var field = typeof(BlockScript).GetField("health", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return field != null ? (float)field.GetValue(block) : 100f;
    }

    private IEnumerator ExplodeSequence()
    {
        Debug.Log("⚡ ExplodeSequence INICIADA");
        isExploding = true;

        // ⭐ NO desactivar colisiones aún
        var rb = GetComponent<Rigidbody2D>();
        var collider = GetComponent<Collider2D>();

        // Congelar física para evitar movimientos durante explosión
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Parpadeo pre-explosión
        Debug.Log($"⏱️ Iniciando parpadeo ({preExplosionTime}s)...");
        float elapsed = 0f;
        while (elapsed < preExplosionTime)
        {
            float t = Mathf.Abs(Mathf.Sin(elapsed * 10f));
            spriteRenderer.color = Color.Lerp(originalColor, flashColor, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = originalColor;
        Debug.Log("✅ Parpadeo completado. Llamando a Explode()...");

        // ⭐ EXPLOTAR PRIMERO (con colisiones activas)
        Explode();

        // ⭐ AHORA SÍ desactivar colisiones
        if (rb != null) rb.simulated = false;
        if (collider != null) collider.enabled = false;

        // ⭐ ESPERAR ANTES DE DESTRUIR
        yield return new WaitForSeconds(explosionDuration);
        Debug.Log("🗑️ Destruyendo objeto explosivo");
        Destroy(gameObject);
    }

    private void Explode()
    {
        Debug.Log($"💥 Explosión en {transform.position} con radio {radius}");

        // Efectos visuales
        if (explosionParticles != null)
        {
            GameObject particles = Instantiate(explosionParticles, transform.position, Quaternion.identity);
            Destroy(particles, 3f);
        }

        // Daño radial
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, damageLayers);

        Debug.Log($"🎯 Objetos detectados en explosión: {hits.Length}");

        foreach (var hit in hits)
        {
            if (hit == null || hit.gameObject == gameObject) continue;

            float distance = Vector2.Distance(transform.position, hit.transform.position);
            float t = Mathf.Clamp01(1f - (distance / radius));
            float damage = maxDamage * t;

            Debug.Log($"💥 Daño a {hit.gameObject.name}: {damage:F1} (distancia: {distance:F2})");

            // Robots
            var robot = hit.GetComponent<RobotScript>();
            if (robot != null)
                robot.TakeDamage(damage);

            // Bloques
            var otherBlock = hit.GetComponent<BlockScript>();
            if (otherBlock != null && otherBlock != block)
                otherBlock.TakeDamage(damage);

            // Fuerza
            var rb = hit.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                rb.AddForce(dir * (maxForce * t), ForceMode2D.Impulse);
            }
        }

        // Cámara
        CameraMovement cam = Camera.main?.GetComponent<CameraMovement>();
        cam?.TriggerShake(0.25f, 0.25f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}