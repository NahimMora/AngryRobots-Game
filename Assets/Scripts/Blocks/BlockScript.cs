using UnityEngine;
using System.Collections;

public class BlockScript : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float health = 100f;
    [SerializeField] private float damageMultiplier = 5f;

    [Header("Visual Feedback")]
    [SerializeField] private Sprite[] damageSprites;
    [SerializeField] private GameObject destructionParticles;
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private Color damageFlashColor = Color.white;
    [SerializeField] private float destructionDelay = 0.5f; // ⭐ NUEVO

    private SpriteRenderer spriteRenderer;
    private float maxHealth;
    private Color originalColor;
    private bool isFlashing = false;
    private bool isDestroyed = false; // ⭐ NUEVO

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxHealth = health;
        originalColor = spriteRenderer.color;

        if (damageSprites.Length > 0)
        {
            spriteRenderer.sprite = damageSprites[0];
        }

        spriteRenderer.sortingOrder = 1;
    }

    private void UpdateSprite()
    {
        if (damageSprites.Length == 0) return;

        float healthPercent = health / maxHealth;

        if (healthPercent > 0.66f && damageSprites.Length > 0)
        {
            spriteRenderer.sprite = damageSprites[0];
        }
        else if (healthPercent > 0.33f && damageSprites.Length > 1)
        {
            spriteRenderer.sprite = damageSprites[1];
        }
        else if (damageSprites.Length > 2)
        {
            spriteRenderer.sprite = damageSprites[2];
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDestroyed) return; // ⭐ NUEVO

        health -= amount;

        if (!isFlashing)
        {
            StartCoroutine(DamageFlash());
        }

        StartCoroutine(DamageShake());
        UpdateSprite();

        if (health <= 0)
        {
            DestroyBlock();
        }
    }

    private IEnumerator DamageFlash()
    {
        isFlashing = true;
        spriteRenderer.color = damageFlashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
        isFlashing = false;
    }

    private IEnumerator DamageShake()
    {
        Vector3 originalPos = transform.localPosition;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(0.1f, 0f, elapsed / duration);
            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }

        transform.localPosition = originalPos;
    }

    private void DestroyBlock()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        Debug.Log($"💀 BlockScript.DestroyBlock() llamado. Tiene ExplosiveBehavior: {GetComponent<ExplosiveBehaviorFixed>() != null}");

        // Desactivar colisiones inmediatamente
        var collider = GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        // Spawn de partículas
        if (destructionParticles != null)
        {
            Instantiate(destructionParticles, transform.position, Quaternion.identity);
        }

        // Shake de cámara
        CameraMovement cam = Camera.main?.GetComponent<CameraMovement>();
        cam?.TriggerShake(0.15f, 0.15f);

        // ⭐ SI TIENE EXPLOSIVE BEHAVIOR, NO DESTRUIR AQUÍ
        ExplosiveBehaviorFixed explosive = GetComponent<ExplosiveBehaviorFixed>();
        if (explosive == null)
        {
            // ⭐ DESTRUIR DESPUÉS DE UN DELAY (solo si NO es explosivo)
            Destroy(gameObject, destructionDelay);
        }
        else
        {
            Debug.Log("⚠️ Es explosivo, NO destruyendo desde BlockScript");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDestroyed) return; // ⭐ NUEVO

        float impactSpeed = collision.relativeVelocity.magnitude;
        float damage = impactSpeed * damageMultiplier;

        if (impactSpeed > 1f)
        {
            TakeDamage(damage);
        }
    }
}