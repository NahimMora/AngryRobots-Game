using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [Header("Target Following")]
    [SerializeField] private Transform proyectileTransform;
    [SerializeField] private float xStartFollowing = 0f;
    [SerializeField] private float xStopPosition = 15f;
    [SerializeField] private float followSpeed = 5f;

    [Header("Screen Shake")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeMagnitude = 0.2f;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private bool isFollowing = false;

    // Shake
    private float shakeTimer = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    // 🔧 NUEVO: Para evitar que LateUpdate sobreescriba posiciones
    private bool isResetting = false;

    private void LateUpdate()
    {
        // 🔧 No actualizar si estamos reseteando
        if (isResetting) return;

        UpdateShake();
        UpdateFollowing();
    }

    private void UpdateFollowing()
    {
        // 🔧 Si no hay target, solo aplicar shake sobre la posición actual sin moverla
        if (proyectileTransform == null)
        {
            if (shakeTimer > 0f)
            {
                transform.position = startPosition + shakeOffset;
            }
            return;
        }

        // Activar seguimiento cuando el proyectil cruza el umbral
        if (!isFollowing && proyectileTransform.position.x > xStartFollowing)
        {
            isFollowing = true;
        }

        // Seguimiento suave con límite
        if (isFollowing && transform.position.x < xStopPosition)
        {
            if (proyectileTransform.position.x > transform.position.x)
            {
                targetPosition = new Vector3(
                    proyectileTransform.position.x,
                    startPosition.y,
                    startPosition.z
                );

                // Lerp suave
                Vector3 smoothPosition = Vector3.Lerp(
                    transform.position,
                    targetPosition,
                    followSpeed * Time.deltaTime
                );

                transform.position = smoothPosition + shakeOffset;
            }
        }
        else if (shakeTimer > 0f)
        {
            // Solo aplicar shake si hay
            transform.position = transform.position + shakeOffset;
        }
    }

    private void UpdateShake()
    {
        if (shakeTimer > 0f)
        {
            shakeTimer -= Time.deltaTime;

            // Shake aleatorio con decay
            float magnitude = shakeMagnitude * (shakeTimer / shakeDuration);
            shakeOffset = new Vector3(
                Random.Range(-1f, 1f) * magnitude,
                Random.Range(-1f, 1f) * magnitude,
                0f
            );
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
    }

    public void TriggerShake()
    {
        shakeTimer = shakeDuration;
    }

    public void TriggerShake(float customDuration, float customMagnitude)
    {
        shakeDuration = customDuration;
        shakeMagnitude = customMagnitude;
        shakeTimer = shakeDuration;
    }

    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;
        transform.position = position;
        isFollowing = false;
        proyectileTransform = null;
        shakeTimer = 0f;
        shakeOffset = Vector3.zero;

        Debug.Log($"📷 CameraMovement.SetStartPosition: {position} | Transform ahora en: {transform.position}");
    }

    public void ResetPositionCamera()
    {
        isFollowing = false;
        shakeTimer = 0f;
        shakeOffset = Vector3.zero;
        proyectileTransform = null;

        // Reseteo suave usando startPosition
        StopAllCoroutines(); // Por si hay un reset anterior
        StartCoroutine(SmoothReset());
    }

    private System.Collections.IEnumerator SmoothReset()
    {
        isResetting = true;

        float elapsed = 0f;
        float duration = 0.5f;
        Vector3 currentPos = transform.position;

        Debug.Log($"📷 Iniciando reset desde {currentPos} hacia {startPosition}");

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Ease out
            t = 1f - Mathf.Pow(1f - t, 3f);

            transform.position = Vector3.Lerp(currentPos, startPosition, t);
            yield return null;
        }

        transform.position = startPosition;
        isResetting = false;

        Debug.Log($"📷 Reset completado. Posición final: {transform.position}");
    }

    public void SetTarget(Transform newTarget)
    {
        proyectileTransform = newTarget;
        isFollowing = false;

        if (newTarget != null)
        {
            Debug.Log($"🎯 Target establecido: {newTarget.name}");
        }
        else
        {
            Debug.Log("🎯 Target removido (null)");
        }
    }

    // 🔧 NUEVO: Método para debugging en el Inspector
    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            // Dibujar la startPosition
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(startPosition, 0.5f);

            // Dibujar posición actual
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 0.3f);
        }
    }
}