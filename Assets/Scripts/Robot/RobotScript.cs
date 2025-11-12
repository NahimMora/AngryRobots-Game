using UnityEngine;
using System;
using System.Collections;

public class RobotScript : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private GameObject explosionPrefab; // Asignar prefab de explosión en el inspector
    [SerializeField] private float maxHealth = 100f;

    [Header("Mecánica de Caída")]
    [SerializeField] private float minFallVelocity = 3f; // Velocidad mínima para considerar caída mortal
    [SerializeField] private float fallDamageMultiplier = 50f; // Multiplicador de daño por caída
    [SerializeField] private bool instantKillOnFall = true; // Si true, muerte instantánea; si false, usa multiplicador

    [Header("Efectos Visuales")]
    [SerializeField] private bool enableVisualEffects = true;
    [SerializeField] private Color damageFlashColor = new Color(1f, 0.3f, 0.3f); // Rojo
    [SerializeField] private Color criticalHealthColor = new Color(1f, 0.5f, 0f); // Naranja
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float shakeIntensity = 0.1f;

    private float currentHealth;
    private Animator animator;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private bool isFlashing = false;

    public static event Action OnRobotDestroyed;
    public event Action<float> OnHealthChanged;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHealth = maxHealth;

        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        originalScale = transform.localScale;
    }

    public float GetHealthPercent()
    {
        return currentHealth / maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"🤖 Robot recibió {damage} de daño. Vida: {currentHealth}/{maxHealth}");

        if (enableVisualEffects && !isFlashing)
            StartCoroutine(DamageEffect());

        if (currentHealth <= 0)
            Die();
        else
            OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDead) return;

        // Colisión con proyectiles (comportamiento normal)
        ProyectileScript proyectile = collision.gameObject.GetComponent<ProyectileScript>();
        if (proyectile != null)
        {
            float impactForce = collision.relativeVelocity.magnitude;
            float damage = impactForce * 10f;
            TakeDamage(damage);
            Debug.Log($"💥 Robot impactado - Fuerza: {impactForce:F1}, Daño: {damage:F1}");
            return;
        }

        // NUEVA MECÁNICA: Detectar objetos cayendo desde arriba
        Rigidbody2D otherRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (otherRb != null)
        {
            // Verificar si el objeto viene desde arriba
            Vector2 relativeVelocity = collision.relativeVelocity;
            float verticalVelocity = relativeVelocity.y;

            // Si la velocidad vertical es negativa (cayendo) y supera el mínimo
            if (verticalVelocity < -minFallVelocity)
            {
                if (instantKillOnFall)
                {
                    // Muerte instantánea
                    Debug.Log($"☠️ ¡MUERTE INSTANTÁNEA! Objeto cayó sobre el robot con velocidad: {Mathf.Abs(verticalVelocity):F1} m/s");
                    TakeDamage(maxHealth); // Daño igual a toda la vida
                }
                else
                {
                    // Daño proporcional a la velocidad de caída
                    float fallDamage = Mathf.Abs(verticalVelocity) * otherRb.mass * fallDamageMultiplier;
                    TakeDamage(fallDamage);
                    Debug.Log($"📦 ¡Objeto cayó sobre el robot! Velocidad: {Mathf.Abs(verticalVelocity):F1}, Daño: {fallDamage:F1}");
                }
                return;
            }

            // Colisión lateral con objetos pesados (comportamiento original)
            if (otherRb.mass > 1f)
            {
                float impactForce = collision.relativeVelocity.magnitude;
                float damage = impactForce * otherRb.mass * 2f;
                if (damage > 20f)
                {
                    TakeDamage(damage);
                    Debug.Log($"📦 Robot golpeado por objeto pesado - Daño: {damage:F1}");
                }
            }
        }
    }

    private IEnumerator DamageEffect()
    {
        isFlashing = true;
        float healthPercent = currentHealth / maxHealth;

        if (spriteRenderer != null)
            spriteRenderer.color = damageFlashColor;

        Vector3 originalPos = transform.position;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            float offsetX = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            float offsetY = UnityEngine.Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = originalPos + new Vector3(offsetX, offsetY, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPos;

        if (spriteRenderer != null)
        {
            if (healthPercent <= 0.25f)
            {
                spriteRenderer.color = criticalHealthColor;
                StartCoroutine(CriticalHealthBlink());
            }
            else if (healthPercent <= 0.5f)
                spriteRenderer.color = Color.Lerp(originalColor, Color.yellow, 0.3f);
            else
                spriteRenderer.color = originalColor;
        }

        isFlashing = false;
    }

    private IEnumerator CriticalHealthBlink()
    {
        while (currentHealth > 0 && currentHealth <= maxHealth * 0.25f && !isDead)
        {
            if (spriteRenderer != null)
                spriteRenderer.color = criticalHealthColor;
            yield return new WaitForSeconds(0.2f);

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (enableVisualEffects)
            StartCoroutine(DeathEffect());

        if (animator != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == "Dead")
                {
                    animator.SetBool("Dead", true);
                    break;
                }
            }
        }

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        OnRobotDestroyed?.Invoke();
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathEffect()
    {
        float duration = 0.5f;
        float elapsed = 0f;
        Vector3 startScale = transform.localScale;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            if (spriteRenderer != null)
                spriteRenderer.color = Color.Lerp(Color.white, Color.red, Mathf.PingPong(Time.time * 20f, 1f));

            transform.Rotate(0, 0, 1000f * Time.deltaTime);
            float scale = Mathf.Lerp(1f, 0.5f, t);
            transform.localScale = startScale * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1.5f);
        Destroy(gameObject);
    }
}