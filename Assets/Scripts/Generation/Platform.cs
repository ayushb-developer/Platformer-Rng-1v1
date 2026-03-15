using UnityEngine;

public class Platform : MonoBehaviour
{
    GameObject visual;
    SpriteRenderer spriteRenderer;
    void Awake()
    {
        visual = transform.GetChild(0).gameObject;
        spriteRenderer = visual.GetComponent<SpriteRenderer>();
    }
    public void SetVisual()
    {
        float width = transform.localScale.x;
        visual.transform.localScale = new Vector3(1/width, 1, 1);
        spriteRenderer.size = new Vector2(width, spriteRenderer.size.y);
    }
}
