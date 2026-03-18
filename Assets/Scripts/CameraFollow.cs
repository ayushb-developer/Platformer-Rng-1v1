using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] float smoothSpeed = 5f;
    Transform target;
    Rigidbody2D targetRb;

    void LateUpdate()
    {
        if (target == null) return;

        float lookAhead = targetRb.linearVelocity.x * 0.3f;
        float yDamped = target.position.y + 2;
        // float yDamped = Mathf.Lerp(transform.position.y, target.position.y + 2, 2f * Time.deltaTime);
        Vector3 desiredPosition = new(
            target.position.x + lookAhead,
            yDamped,
            -10
        );

        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
    public void SetTarget(Transform transform)
    {
        target = transform;
        targetRb = transform.GetComponent<Rigidbody2D>();
    }
}