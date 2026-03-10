using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float smoothSpeed = 5f;

    void LateUpdate()
    {
        Vector3 desiredPosition = new Vector3(
            target.position.x,
            target.position.y + 2,
            -10
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}