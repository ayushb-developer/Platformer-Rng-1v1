using UnityEngine;

public class FlyingObstacle : Obstacle
{
    float leftLimit;
    float rightLimit;

    public float speed = 2f;

    bool movingRight = true;

    protected override void Update()
    {
        base.Update();   // run cleanup
        Patrol();
    }

    public void SetPatrolRange(float left, float right)
    {
        leftLimit = left;
        rightLimit = right;
    }

    void Patrol()
    {
        float dir = movingRight ? 1 : -1;

        transform.Translate(Vector2.right * dir * speed * Time.deltaTime);

        if (transform.position.x > rightLimit)
            movingRight = false;

        if (transform.position.x < leftLimit)
            movingRight = true;
    }

    void OnDrawGizmos()
{
    Gizmos.color = Color.red;
    Gizmos.DrawLine(
        new Vector3(leftLimit, transform.position.y),
        new Vector3(rightLimit, transform.position.y)
    );
}

}
