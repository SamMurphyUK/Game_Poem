using UnityEngine;

// Draws a white triangle behind this sprite so light-family ships keep the
// offset backing the dark sprites already have painted into the PNG.
public class WhiteTriangleBacking : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0.02f, 0.015f, 0.05f);
    [SerializeField] private float scale = 1.2f;
    [SerializeField] private int sortingOffset = 0;

    private static Sprite sharedTriangle;

    private SpriteRenderer body;
    private SpriteRenderer backing;

    public static WhiteTriangleBacking Ensure(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        WhiteTriangleBacking existing = target.GetComponent<WhiteTriangleBacking>();
        if (existing != null)
        {
            return existing;
        }

        return target.AddComponent<WhiteTriangleBacking>();
    }

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        BuildBacking();
    }

    private void LateUpdate()
    {
        if (backing == null || body == null)
        {
            return;
        }

        backing.enabled = body.enabled && body.sprite != null;
        if (!backing.enabled)
        {
            return;
        }

        backing.sortingLayerID = body.sortingLayerID;
        // Stay on the same sort as the ship so the hearts tilemap cannot cover the
        // white plate. Z offset keeps the plate just behind the colored triangle.
        backing.sortingOrder = body.sortingOrder + sortingOffset;
        backing.color = Color.white;

        Vector2 size = body.sprite.bounds.size;
        backing.transform.localPosition = offset;
        backing.transform.localScale = new Vector3(size.x * scale, size.y * scale, 1f);
        backing.transform.localRotation = Quaternion.identity;
    }

    private void BuildBacking()
    {
        if (backing != null)
        {
            return;
        }

        GameObject child = new GameObject("WhiteBacking");
        child.transform.SetParent(transform, false);
        child.transform.localPosition = offset;
        backing = child.AddComponent<SpriteRenderer>();
        backing.sprite = SharedTriangle();
        backing.color = Color.white;
    }

    private static Sprite SharedTriangle()
    {
        if (sharedTriangle != null)
        {
            return sharedTriangle;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;

        Color clear = new Color(1f, 1f, 1f, 0f);
        Color solid = Color.white;
        for (int y = 0; y < size; y++)
        {
            // SpriteRenderer: y=0 is the bottom. Wide base there, point at the top (/_\).
            float t = 1f - (y + 0.5f) / size;
            float half = t * 0.5f;
            float left = 0.5f - half;
            float right = 0.5f + half;
            for (int x = 0; x < size; x++)
            {
                float u = (x + 0.5f) / size;
                texture.SetPixel(x, y, u >= left && u <= right ? solid : clear);
            }
        }

        texture.Apply();
        sharedTriangle = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        sharedTriangle.name = "WhiteTriangle";
        return sharedTriangle;
    }
}
