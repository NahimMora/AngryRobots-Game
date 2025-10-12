using UnityEngine;

public class RobotScript : MonoBehaviour
{
    [SerializeField] private GameObject explosionPrefab; // ⭐ Cada robot puede tener su explosión
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        Debug.Log($"🤖 Robot recibió {damage} de daño. Vida: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ShouldDie(collision) && !isDead)
        {
            Die();
        }
    }

    private bool ShouldDie(Collision2D collision)
    {
        bool isProyectile = collision.gameObject.GetComponent<ProyectileScript>();
        bool isCrushed = collision.contacts[0].normal.y < -0.5f;
        return isProyectile || isCrushed;
    }

    private void Die()
    {
        if (isDead) return;

        isDead = true;
        animator.SetBool("Dead", true);

        // ⭐ Spawner explosión antes de morir
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        StartCoroutine(DeathSequence());
    }

    private System.Collections.IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }
}