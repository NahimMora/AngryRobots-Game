using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour
{
    [SerializeField] private float radius = 2f;
    [SerializeField] private float maxDamage = 100f;
    [SerializeField] private float maxForce = 8f;
    [SerializeField] private LayerMask damageLayers;
    [SerializeField] private AnimationClip explosionAnim;
    [SerializeField] private float lifetime = 2f; // ⭐ NUEVO

    private void Start()
    {
        StartCoroutine(ExplosionSequence());
    }

    // ⭐ USAR COROUTINE PARA CONTROLAR TIMING
    private IEnumerator ExplosionSequence()
    {
        // Explosión inmediata
        Explode();

        // Esperar a que termine la animación
        float duration = explosionAnim != null ? explosionAnim.length : lifetime;
        yield return new WaitForSeconds(duration);

        // Ahora sí destruir
        Destroy(gameObject);
    }

    private void Explode()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius, damageLayers);

        foreach (var c in hits)
        {
            if (c == null) continue;

            float distance = Vector2.Distance(transform.position, c.transform.position);
            float t = Mathf.Clamp01(1f - (distance / radius));
            float damage = maxDamage * t;

            var robot = c.GetComponent<RobotScript>();
            if (robot != null)
            {
                robot.TakeDamage(damage);
            }

            var block = c.GetComponent<BlockScript>();
            if (block != null)
            {
                block.TakeDamage(damage);
            }

            var rb = c.attachedRigidbody;
            if (rb != null)
            {
                Vector2 dir = (c.transform.position - transform.position).normalized;
                float force = maxForce * t;
                rb.AddForce(dir * force, ForceMode2D.Impulse);
            }
        }

        // Shake de cámara
        CameraMovement cam = Camera.main?.GetComponent<CameraMovement>();
        cam?.TriggerShake(0.3f, 0.3f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}