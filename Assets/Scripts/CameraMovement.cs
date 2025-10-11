using UnityEngine;

public class CameraMovement : MonoBehaviour
{

    [SerializeField] private Transform ProyectileTransform;
    [SerializeField] private float xStopPosition;
    private Vector3 startPosition;
    void Start()
    {
        startPosition = transform.position;
    }
    void Update()
    {

    }

    private void LateUpdate()
    {
        if (ProyectileTransform == null) return;

        if (ProyectileTransform.position.x > transform.position.x && transform.position.x < xStopPosition)
        {
            transform.position = new Vector3(ProyectileTransform.position.x, transform.position.y, transform.position.z);
        }
    }

    public void ResetPositionCamera()
    {
        transform.position = startPosition;
    }

    public void SetTarget(Transform newTarget)
    {
        ProyectileTransform = newTarget;
    }
}
