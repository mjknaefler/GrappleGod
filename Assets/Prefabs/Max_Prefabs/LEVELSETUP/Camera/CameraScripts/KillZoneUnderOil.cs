using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class KillZoneUnderOil : MonoBehaviour
{
    [SerializeField] private CameraBottomWaterFlipbook water;
    [SerializeField] private float grace = 0.2f;
    [SerializeField] private float colliderHeight = 0.25f;
    [SerializeField] private float widthPadding = 1.2f;

    Camera cam;
    BoxCollider2D box;
    SpriteRenderer waterSR;

    void Awake()
    {
        cam = Camera.main;
        box = GetComponent<BoxCollider2D>();
        box.isTrigger = true;
        if (water) waterSR = water.GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (!cam || !water || !waterSR) return;

        float ortho = cam.orthographicSize;
        float aspect = cam.aspect;
        Vector3 c = cam.transform.position;

        float waterTopY = water.transform.position.y + waterSR.size.y * 0.5f;
        float centerY = waterTopY - grace - colliderHeight * 0.5f;

        transform.position = new Vector3(c.x, centerY, 0f);
        box.size = new Vector2(2f * ortho * aspect * widthPadding, colliderHeight);
        box.offset = Vector2.zero;
    }
}
