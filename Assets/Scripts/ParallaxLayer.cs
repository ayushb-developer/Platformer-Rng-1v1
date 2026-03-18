using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] Transform target;
    [SerializeField] float parallaxMultiplier = 0.3f;

    Transform[] tiles;

    float spriteWidth;
    float lastTargetX;

    void Start()
    {
        tiles = new Transform[transform.childCount];

        for (int i = 0; i < tiles.Length; i++)
            tiles[i] = transform.GetChild(i);

        spriteWidth = tiles[0].GetComponent<SpriteRenderer>().bounds.size.x;

        lastTargetX = target.position.x;
    }

    void LateUpdate()
    {
        float deltaX = target.position.x - lastTargetX;

        transform.position += new Vector3(deltaX * parallaxMultiplier, 0, 0);

        lastTargetX = target.position.x;

        RepositionTiles();
    }

    void RepositionTiles()
    {
        Transform leftTile = tiles[0];
        Transform rightTile = tiles[1];

        if (leftTile.position.x + spriteWidth < target.position.x)
        {
            leftTile.position = new Vector3(
                rightTile.position.x + spriteWidth,
                leftTile.position.y,
                leftTile.position.z
            );

            SwapTiles();
        }
    }

    void SwapTiles()
    {
        Transform temp = tiles[0];
        tiles[0] = tiles[1];
        tiles[1] = temp;
    }
}
