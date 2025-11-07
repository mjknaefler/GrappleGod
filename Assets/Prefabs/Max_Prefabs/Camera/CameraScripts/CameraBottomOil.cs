using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class CameraBottomWaterFlipbook : MonoBehaviour
{
    [SerializeField] private float yOffset = 0.05f;
    [SerializeField] private float widthPadding = 1.2f;
    [SerializeField] private float screenHeightPercent = 0.35f;
    [SerializeField] private float bottomBleed = 0.05f;
    [SerializeField] private float widthBleed = 0f;
    [SerializeField] private Sprite[] frames;
    [SerializeField] private float fps = 8f;

    Camera cam;
    SpriteRenderer sr;
    float t;

    void Awake()
    {
        cam = Camera.main;
        sr = GetComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
    }

    void LateUpdate()
    {
        if (!cam || frames == null || frames.Length == 0) return;

        float ortho = cam.orthographicSize;
        float aspect = cam.aspect;
        Vector3 c = cam.transform.position;

        float targetWidth = 2f * ortho * aspect * widthPadding + widthBleed;
        float targetHeight = Mathf.Clamp01(screenHeightPercent) * 2f * ortho + bottomBleed;

        float bottomY = c.y - ortho + yOffset;
        transform.position = new Vector3(c.x, bottomY + targetHeight * 0.5f, 0f);

        if (sr.sprite != null)
        {
            float ppu = sr.sprite.pixelsPerUnit;
            float tileW = sr.sprite.rect.width / ppu;
            float tileH = sr.sprite.rect.height / ppu;
            int tilesX = Mathf.Max(1, Mathf.CeilToInt(targetWidth / Mathf.Max(0.0001f, tileW)));
            int tilesY = Mathf.Max(1, Mathf.CeilToInt(targetHeight / Mathf.Max(0.0001f, tileH)));
            sr.size = new Vector2(tilesX * tileW, tilesY * tileH);
        }
        else
        {
            sr.size = new Vector2(targetWidth, targetHeight);
        }

        t += Time.deltaTime * fps;
        int idx = (int)t % frames.Length;
        sr.sprite = frames[idx];
    }
}
