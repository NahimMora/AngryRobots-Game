using UnityEngine;
using UnityEngine.InputSystem;

public class TankLauncherScript : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private float maxLaunchForce = 20f;
    [SerializeField] private float maxAimDistance = 5f;
    [SerializeField] private float spawnOffset = 0.5f; // ⭐ NUEVO: Distancia extra desde el tanque

    [Header("Trajectory Prediction")]
    [SerializeField] private LineRenderer trajectoryLine;
    [SerializeField] private int trajectoryPoints = 30;
    [SerializeField] private float trajectoryTimeStep = 0.05f;
    [SerializeField] private Color trajectoryColor = new Color(1f, 0f, 0f, 0.7f);
    [SerializeField] private float trajectoryWidth = 0.1f;

    [Header("Visual Feedback")]
    [SerializeField] private Transform aimIndicator;
    [SerializeField] private SpriteRenderer tankRenderer;

    private Camera mainCamera;
    private bool isAiming = false;
    private Vector2 pullVector;
    private GameObject currentProjectile;
    private bool canShoot = true;
    private float currentForce;

    private GameObject[] proyectilePrefabs;
    private int[] proyectileQuantities;
    private int currentProyectileIndex = 0;

    void Start()
    {
        mainCamera = Camera.main;
        SetupTrajectoryLine();

        if (spawnPoint == null)
            spawnPoint = transform;

        if (proyectilePrefabs == null || proyectilePrefabs.Length == 0)
        {
            canShoot = false;
            Debug.LogWarning("⚠️ Tanque esperando configuración de proyectiles del LevelManager");
        }
    }

    public void SetupProjectiles(GameObject[] projectiles, int[] quantities)
    {
        if (projectiles == null || projectiles.Length == 0)
        {
            Debug.LogError("❌ No se proporcionaron proyectiles al tanque!");
            canShoot = false;
            return;
        }

        proyectilePrefabs = projectiles;
        proyectileQuantities = quantities;
        currentProyectileIndex = 0;
        canShoot = true;
    }

    void Update()
    {
        if (!canShoot) return;
        HandleAiming();
    }

    private void SetupTrajectoryLine()
    {
        if (trajectoryLine == null)
            trajectoryLine = gameObject.AddComponent<LineRenderer>();

        trajectoryLine.startWidth = trajectoryWidth;
        trajectoryLine.endWidth = trajectoryWidth * 0.5f;
        trajectoryLine.material = new Material(Shader.Find("Sprites/Default"));
        trajectoryLine.startColor = trajectoryColor;
        trajectoryLine.endColor = new Color(trajectoryColor.r, trajectoryColor.g, trajectoryColor.b, 0.1f);
        trajectoryLine.numCornerVertices = 5;
        trajectoryLine.numCapVertices = 5;
        trajectoryLine.sortingOrder = 10;
        trajectoryLine.positionCount = 0;
        trajectoryLine.useWorldSpace = true;
    }

    private void HandleAiming()
    {
        Vector2 mouseWorldPos = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());

        if (Mouse.current.leftButton.wasPressedThisFrame)
            isAiming = true;

        if (isAiming && Mouse.current.leftButton.isPressed)
        {
            Vector2 dir = (Vector2)spawnPoint.position - mouseWorldPos;
            float distance = Mathf.Min(dir.magnitude, maxAimDistance);

            pullVector = dir.normalized * distance;
            currentForce = (distance / maxAimDistance) * maxLaunchForce;

            if (aimIndicator != null)
            {
                float angle = Mathf.Atan2(pullVector.y, pullVector.x) * Mathf.Rad2Deg;
                aimIndicator.rotation = Quaternion.Euler(0, 0, angle);
            }

            Vector2 launchDir = pullVector.normalized;
            DrawTrajectory(launchDir, currentForce);
        }

        if (Mouse.current.leftButton.wasReleasedThisFrame && isAiming)
        {
            LaunchProjectile();
            isAiming = false;
            trajectoryLine.positionCount = 0;
        }
    }

    private void DrawTrajectory(Vector2 direction, float force)
    {
        if (trajectoryLine == null) return;

        Vector2 velocity = direction * force;
        Vector2 startPos = spawnPoint.position;
        trajectoryLine.positionCount = trajectoryPoints;

        for (int i = 0; i < trajectoryPoints; i++)
        {
            float time = i * trajectoryTimeStep;
            Vector2 point = startPos + velocity * time + 0.5f * Physics2D.gravity * (time * time);
            trajectoryLine.SetPosition(i, point);
        }
    }

    private void LaunchProjectile()
    {
        if (currentProyectileIndex >= proyectilePrefabs.Length)
        {
            Debug.Log("❌ No hay más proyectiles disponibles");
            canShoot = false;
            return;
        }

        // ⭐ NUEVO: Spawnear el proyectil con un offset en la dirección de disparo
        Vector2 spawnPosition = (Vector2)spawnPoint.position + pullVector.normalized * spawnOffset;

        currentProjectile = Instantiate(proyectilePrefabs[currentProyectileIndex], spawnPosition, Quaternion.identity);

        Rigidbody2D rb = currentProjectile.GetComponent<Rigidbody2D>();
        ProyectileScript projectileScript = currentProjectile.GetComponent<ProyectileScript>();

        if (rb == null)
        {
            Debug.LogError("❌ El proyectil no tiene Rigidbody2D!");
            Destroy(currentProjectile);
            return;
        }

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 1f;

        // ⭐ NUEVO: Ignorar colisiones con el tanque
        Collider2D tankCollider = GetComponent<Collider2D>();
        Collider2D projectileCollider = currentProjectile.GetComponent<Collider2D>();

        if (tankCollider != null && projectileCollider != null)
        {
            Physics2D.IgnoreCollision(tankCollider, projectileCollider);
            Debug.Log("✅ Colisiones ignoradas entre tanque y proyectil");
        }

        rb.AddForce(pullVector.normalized * currentForce, ForceMode2D.Impulse);

        if (projectileScript != null)
            projectileScript.Activate();

        Debug.Log($"🚀 Proyectil lanzado con fuerza {currentForce:F2}, dirección {pullVector.normalized}");

        CameraMovement cam = mainCamera.GetComponent<CameraMovement>();
        if (cam != null)
            cam.SetTarget(currentProjectile.transform);

        StartCoroutine(ShootFeedback());

        canShoot = false;
        currentProyectileIndex++;
    }

    private System.Collections.IEnumerator ShootFeedback()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = originalScale * 0.95f;
        yield return new WaitForSeconds(0.1f);
        transform.localScale = originalScale;
    }

    public void OnProjectileFinished()
    {
        if (currentProyectileIndex < proyectilePrefabs.Length)
        {
            canShoot = true;
            Debug.Log("✅ Listo para disparar siguiente proyectil");
        }
        else
        {
            Debug.Log("🏁 Se acabaron los proyectiles");
            canShoot = false;
        }
    }

    public bool IsOutOfAmmo()
    {
        return proyectilePrefabs == null || currentProyectileIndex >= proyectilePrefabs.Length;
    }
}