using UnityEngine;

public class FlyingObstacle : Obstacle
{
    float leftLimit;
    float rightLimit;

    public float speed = 2f;

    bool movingRight = true;

    public void SetPatrolRange(float left, float right)
    {
        leftLimit = left;
        rightLimit = right;
    }

    void Update()
    {
        float dir = movingRight ? 1 : -1;

        transform.Translate(Vector2.right * dir * speed * Time.deltaTime);

        if (transform.position.x > rightLimit)
            movingRight = false;

        if (transform.position.x < leftLimit)
            movingRight = true;
    }
}