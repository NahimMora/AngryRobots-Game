using UnityEngine;

public class RobotScript : MonoBehaviour
{
    private Animator animator;
    private bool isDead = false;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (ShouldDie(collision) && !isDead)
        {
            isDead = true;
            animator.SetBool("Dead", true); // 🔹 Activa la animación
            StartCoroutine(Die());         // 🔹 Espera antes de destruir
        }
    }

    private bool ShouldDie(Collision2D collision)
    {
        bool isProyectile = collision.gameObject.GetComponent<ProyectileScript>();
        bool isCrushed = collision.contacts[0].normal.y < -0.5f;
        return isProyectile || isCrushed;
    }

    private System.Collections.IEnumerator Die()
    {
        yield return new WaitForSeconds(1f); // ⏳ tiempo de la animación (ajusta según tu animación)
        Destroy(gameObject);
    }
}
