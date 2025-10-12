using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField] private Transform ProyectileTransform;
    private float xStartFollowing = 0;
    [SerializeField] private float xStopPosition = 15f;
    private Vector3 startPosition;
    private bool isFollowing = false;

    private void LateUpdate()
    {
        if (ProyectileTransform == null) return;

        if (!isFollowing && ProyectileTransform.position.x > xStartFollowing)
        {
            isFollowing = true;
        }

        if (isFollowing && transform.position.x < xStopPosition)
        {
            if (ProyectileTransform.position.x > transform.position.x)
            {
                transform.position = new Vector3(
                    ProyectileTransform.position.x,
                    transform.position.y,
                    transform.position.z
                );
            }
        }
    }

    public void SetStartPosition(Vector3 position)
    {
        startPosition = position;
        transform.position = position;
        Debug.Log("✅ CameraSpawnPoint position: " + position);
        Debug.Log("✅ Transform de cámara después de setear: " + transform.position);
        Debug.Log("📷 ¿Son iguales? " + (transform.position == position));
    }

    public void ResetPositionCamera()
    {
        isFollowing = false;
        transform.position = startPosition;
        Debug.Log("🔄 Cámara reseteada a: " + startPosition);
    }
    public void SetTarget(Transform newTarget)
    {
        ProyectileTransform = newTarget;
        isFollowing = false;
    }
}