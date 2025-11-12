using UnityEngine;

public class RobotBossScript : MonoBehaviour
{
    [Header("Boss Settings")]
    [SerializeField] private float flipInterval = 3f; // Tiempo entre giros
    [SerializeField] private float flipSpeed = 5f; // Velocidad de rotación del flip

    [Header("Weak Point (Back) Detection")]
    [SerializeField] private float backDetectionAngle = 120f; // Ángulo que considera "espalda"
    [SerializeField] private Color backGizmoColor = Color.red;
    [SerializeField] private Color frontGizmoColor = Color.green;

    [Header("Effects")]
    [SerializeField] private GameObject backHitEffectPrefab; // Partículas especiales al golpear espalda
    [SerializeField] private float backHitShakeDuration = 0.3f;
    [SerializeField] private float backHitShakeIntensity = 0.2f;
    [SerializeField] private float stunDuration = 1f; // Tiempo que queda aturdido al golpear espalda

    [Header("Visual Feedback")]
    [SerializeField] private Color stunColor = Color.yellow; // Color cuando está aturdido
    [SerializeField] private float flashSpeed = 10f; // Velocidad del parpadeo

    [Header("Audio")]
    [SerializeField] private AudioClip backHitSound;
    [SerializeField] private AudioClip frontHitSound;
    [SerializeField] private AudioClip flipSound;

    // Referencias
    private SpriteRenderer spriteRenderer;
    private RobotScript baseRobot;
    private Camera mainCamera;
    private AudioSource audioSource;

    // Estado
    private float flipTimer;
    private bool isFacingRight = false; // La espalda está a la IZQUIERDA inicialmente
    private bool isStunned = false;
    private float stunTimer;
    private Color originalColor;
    private bool isFlipping = false;

    void Awake()
    {
        SetupComponents();
    }

    private void SetupComponents()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseRobot = GetComponent<RobotScript>();
        mainCamera = Camera.main;

        // Audio Source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.playOnAwake = false;

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        flipTimer = flipInterval;
    }

    void Update()
    {
        HandleStun();
        HandleAutoFlip();
    }

    private void HandleStun()
    {
        if (!isStunned) return;

        stunTimer -= Time.deltaTime;

        // Flash visual mientras está aturdido
        if (spriteRenderer != null)
        {
            float lerp = Mathf.PingPong(Time.time * flashSpeed, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, stunColor, lerp);
        }

        if (stunTimer <= 0f)
        {
            RecoverFromStun();
        }
    }

    private void HandleAutoFlip()
    {
        if (isStunned || isFlipping) return;

        flipTimer -= Time.deltaTime;

        if (flipTimer <= 0f)
        {
            FlipBoss();
            flipTimer = flipInterval;
        }
    }

    private void FlipBoss()
    {
        if (isFlipping) return;

        StartCoroutine(FlipCoroutine());
    }

    private System.Collections.IEnumerator FlipCoroutine()
    {
        isFlipping = true;

        // Reproducir sonido
        PlaySound(flipSound);

        // Animación de flip suave
        float startScale = transform.localScale.x;
        float targetScale = -startScale;
        float elapsed = 0f;
        float duration = 1f / flipSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Usar una curva suave
            float smoothT = Mathf.SmoothStep(0f, 1f, t);
            float newScaleX = Mathf.Lerp(startScale, targetScale, smoothT);

            transform.localScale = new Vector3(
                newScaleX,
                transform.localScale.y,
                transform.localScale.z
            );

            yield return null;
        }

        // Asegurar escala final exacta
        transform.localScale = new Vector3(
            targetScale,
            transform.localScale.y,
            transform.localScale.z
        );

        // Actualizar dirección
        isFacingRight = !isFacingRight;
        isFlipping = false;

        Debug.Log($"🔄 Boss giró. Ahora mira hacia: {(isFacingRight ? "DERECHA" : "IZQUIERDA")}");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Verificar si es un proyectil
        ProyectileScript proyectile = collision.gameObject.GetComponent<ProyectileScript>();
        if (proyectile == null) return;

        // Obtener punto y dirección de impacto
        Vector2 impactPoint = collision.contacts[0].point;
        Vector2 impactDirection = (impactPoint - (Vector2)transform.position).normalized;

        // Determinar si golpeó la espalda
        bool hitBack = IsHitFromBack(impactDirection);

        if (hitBack)
        {
            OnBackHit(impactPoint, collision);
        }
        else
        {
            OnFrontHit(impactPoint, collision);
        }
    }

    private bool IsHitFromBack(Vector2 impactDirection)
    {
        // Determinar dirección frontal del boss
        Vector2 bossForward = isFacingRight ? Vector2.right : Vector2.left;

        // Calcular ángulo entre la dirección frontal y el impacto
        float angle = Vector2.Angle(bossForward, impactDirection);

        // Si el ángulo es mayor que backDetectionAngle, es un golpe por la espalda
        return angle > backDetectionAngle;
    }

    private void OnBackHit(Vector2 impactPoint, Collision2D collision)
    {
        Debug.Log("💥 ¡GOLPE CRÍTICO EN LA ESPALDA!");

        // Shake de cámara intenso
        CameraMovement cam = mainCamera?.GetComponent<CameraMovement>();
        cam?.TriggerShake(backHitShakeDuration, backHitShakeIntensity);

        // Efecto visual especial
        if (backHitEffectPrefab != null)
        {
            Instantiate(backHitEffectPrefab, impactPoint, Quaternion.identity);
        }

        // Aturdir al boss
        StunBoss();

        // Sonido especial
        PlaySound(backHitSound);

        // 🎯 AQUÍ PUEDES AÑADIR DAÑO AL JUGADOR EN EL FUTURO
        // Por ahora solo dejamos el feedback visual

        // Hacer daño al robot base (opcional, puedes comentar esta línea)
        float backDamage = 50f; // Daño extra por golpe crítico
        if (baseRobot != null)
        {
            baseRobot.TakeDamage(backDamage);
        }
    }

    private void OnFrontHit(Vector2 impactPoint, Collision2D collision)
    {
        Debug.Log("🛡️ Golpe bloqueado por el frente (sin efecto)");

        // Shake leve
        CameraMovement cam = mainCamera?.GetComponent<CameraMovement>();
        cam?.TriggerShake(0.1f, 0.05f);

        // Sonido de rebote
        PlaySound(frontHitSound);

        // Hacer que el proyectil rebote (opcional)
        Rigidbody2D proyectileRb = collision.gameObject.GetComponent<Rigidbody2D>();
        if (proyectileRb != null)
        {
            Vector2 reflectDirection = Vector2.Reflect(proyectileRb.linearVelocity.normalized, collision.contacts[0].normal);
            proyectileRb.linearVelocity = reflectDirection * proyectileRb.linearVelocity.magnitude * 0.5f;
        }

        // NO hacer daño cuando golpean el frente (está blindado)
    }

    private void StunBoss()
    {
        isStunned = true;
        stunTimer = stunDuration;

        // Cancelar flip automático
        flipTimer = flipInterval;

        Debug.Log("😵 Boss aturdido por " + stunDuration + " segundos");
    }

    private void RecoverFromStun()
    {
        isStunned = false;

        // Restaurar color original
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }

        Debug.Log("✅ Boss recuperado del aturdimiento");
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Visualización en editor
    private void OnDrawGizmosSelected()
    {
        // Dibujar zona frontal (verde) y trasera (roja)
        Vector2 bossForward = isFacingRight ? Vector2.right : Vector2.left;
        float radius = 2f;

        // Zona trasera (roja)
        Gizmos.color = backGizmoColor;
        Vector3 backDirection = -bossForward;
        Gizmos.DrawRay(transform.position, backDirection * radius);
        DrawArc(transform.position, backDirection, backDetectionAngle, radius);

        // Zona frontal (verde)
        Gizmos.color = frontGizmoColor;
        Gizmos.DrawRay(transform.position, bossForward * radius);
        DrawArc(transform.position, bossForward, 360f - backDetectionAngle, radius);

        // Indicador de dirección
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + (Vector3)bossForward * radius * 0.5f, 0.3f);
    }

    private void DrawArc(Vector3 center, Vector3 forward, float angle, float radius)
    {
        int segments = 20;
        float angleStep = angle / segments;
        float startAngle = -angle / 2f;

        Vector3 previousPoint = center + Quaternion.Euler(0, 0, startAngle) * forward * radius;

        for (int i = 1; i <= segments; i++)
        {
            float currentAngle = startAngle + angleStep * i;
            Vector3 currentPoint = center + Quaternion.Euler(0, 0, currentAngle) * forward * radius;
            Gizmos.DrawLine(previousPoint, currentPoint);
            previousPoint = currentPoint;
        }
    }
}