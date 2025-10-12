using UnityEngine;

public class ExplosionScript : MonoBehaviour
{

    [SerializeField] private AnimationClip explosionAnimation;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float explosionDamage = 100f;

    private Animator animator;
    private float animationDuration;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        if (explosionAnimation != null)
        {
            animationDuration = explosionAnimation.length;
        }
        else
        {
            animationDuration = 1f;
        }
        Explode();
        Destroy(gameObject, animationDuration);
    }

    void Explode()
    {
        // Encontrar todos los objetos en el radio de explosión
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D obj in hitObjects)
        {
            // Intentar obtener el componente RobotScript
            RobotScript robot = obj.GetComponent<RobotScript>();
            if (robot != null)
            {
                robot.TakeDamage(explosionDamage);
            }

            // Si tiene Rigidbody2D, aplicar fuerza (efecto de empuje)
            Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 direction = (obj.transform.position - transform.position).normalized;
                rb.AddForce(direction * 500f); // Ajusta la fuerza según necesites
            }
        }

        Debug.Log($"💥 Explosión causó {explosionDamage} de daño en radio {explosionRadius}");
    }

    // Para visualizar el radio en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
