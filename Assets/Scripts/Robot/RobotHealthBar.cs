using UnityEngine;
using UnityEngine.UI;

public class RobotHealthBar : MonoBehaviour
{
    [SerializeField] private RobotScript robot;
    [SerializeField] private Image healthFill;

    [Header("Posición de la barra")]
    [SerializeField] private float heightOffset = 1.2f; // ahora es una altura en unidades, no un multiplicador

    private Transform robotTransform;

    private void Start()
    {
        if (robot == null)
            robot = GetComponentInParent<RobotScript>();

        if (robot != null)
            robotTransform = robot.transform;

        UpdateHealthBar(1f);

        robot.OnHealthChanged += UpdateHealthBar;
    }

    private void LateUpdate()
    {
        if (robotTransform != null)
        {
            Vector3 pos = robotTransform.position + Vector3.up * heightOffset;
            transform.position = pos;
            transform.rotation = Quaternion.identity;
        }
    }

    private void UpdateHealthBar(float healthPercent)
    {
        if (healthFill != null)
            healthFill.fillAmount = healthPercent;
    }

    private void OnDestroy()
    {
        if (robot != null)
            robot.OnHealthChanged -= UpdateHealthBar;
    }
}
