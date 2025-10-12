
using UnityEngine;

public class BlockScript : MonoBehaviour
{
    [SerializeField] private float health = 100f;
    [SerializeField] private float damageMultiplier = 5f;
    [SerializeField] private Sprite[] damageSprites;
    private SpriteRenderer spriteRenderer;
    private float maxHealth;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        maxHealth = health;

        spriteRenderer.sprite = damageSprites[0];
        spriteRenderer.sortingOrder = 1;

    }

    private void UpdateSprite()
    {
        float healthPercent = health / maxHealth;

        if (healthPercent > 0.66f)
        {
            spriteRenderer.sprite = damageSprites[0];
        }
        else if (healthPercent > 0.33f)
        {
            spriteRenderer.sprite = damageSprites[1];
            spriteRenderer.sortingOrder = 1;
        }
        else
        {
            spriteRenderer.sprite = damageSprites[2];
        }
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        UpdateSprite();
        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        float damage = collision.relativeVelocity.magnitude * damageMultiplier;
        TakeDamage(damage);
    }

}
