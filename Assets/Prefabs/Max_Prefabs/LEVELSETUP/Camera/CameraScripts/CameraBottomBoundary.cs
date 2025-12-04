using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class CameraBottomBoundary : MonoBehaviour
{
    public float yOffset = 0f;
    public float widthPadding = 1.2f;
    public float height = 1.0f;

    Camera cam;
    BoxCollider2D box;

    void Awake()
    {
        cam = Camera.main;
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
    }

    void LateUpdate()
    {
        if (!cam) return;
        float ortho = cam.orthographicSize;
        float aspect = cam.aspect;
        Vector3 c = cam.transform.position;
        transform.position = new Vector3(c.x, c.y - ortho + yOffset, 0f);
        box.size = new Vector2(2f * ortho * aspect * widthPadding, height);
        box.offset = Vector2.zero;
    }

    void OnDrawGizmos()
    {
        if (!Camera.main) return;
        var b = GetComponent<BoxCollider2D>();
        if (!b) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + (Vector3)b.offset, b.size);
    }
}
