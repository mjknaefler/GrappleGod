using UnityEngine;

[RequireComponent(typeof(EdgeCollider2D))]
public class CameraEdgeFollower2D : MonoBehaviour
{
    public enum Side { Left, Right, Top, Bottom }
    [SerializeField] private Side side = Side.Left;
    [SerializeField] private float inset = 0f;
    [SerializeField] private float extraLength = 0.5f;

    Camera cam;
    EdgeCollider2D edge;

    void Awake()
    {
        cam = Camera.main;
        edge = GetComponent<EdgeCollider2D>();
    }

    void LateUpdate()
    {
        if (!cam) return;

        float ortho = cam.orthographicSize;
        float aspect = cam.aspect;
        Vector3 c = cam.transform.position;

        float left = c.x - ortho * aspect;
        float right = c.x + ortho * aspect;
        float top = c.y + ortho;
        float bottom = c.y - ortho;

        Vector3 a, b;

        switch (side)
        {
            case Side.Left:
                a = new Vector3(left + inset, bottom - extraLength, 0f);
                b = new Vector3(left + inset, top + extraLength, 0f);
                break;
            case Side.Right:
                a = new Vector3(right - inset, bottom - extraLength, 0f);
                b = new Vector3(right - inset, top + extraLength, 0f);
                break;
            case Side.Top:
                a = new Vector3(left - extraLength, top - inset, 0f);
                b = new Vector3(right + extraLength, top - inset, 0f);
                break;
            default: // Bottom
                a = new Vector3(left - extraLength, bottom + inset, 0f);
                b = new Vector3(right + extraLength, bottom + inset, 0f);
                break;
        }

        Vector2 p0 = transform.InverseTransformPoint(a);
        Vector2 p1 = transform.InverseTransformPoint(b);
        edge.points = new Vector2[] { p0, p1 };
    }
}
