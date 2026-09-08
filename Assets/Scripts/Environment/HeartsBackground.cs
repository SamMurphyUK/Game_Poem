using UnityEngine;
using UnityEngine.Tilemaps;

// Fills the scene Tilemap with the hearts tileset, repeating the 7x7 sheet
// behind the walls and actors. Tiles are painted around every active camera
// so the whole test level is covered as you move without baking hundreds of
// thousands of cells into the scene file.
public class HeartsBackground : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private TileBase[] heartsTiles;
    [SerializeField] private Vector2Int patternSize = new Vector2Int(7, 7);
    [SerializeField] private float margin = 12f;
    [SerializeField] private int chunkPad = 8;

    private BoundsInt painted;

    private void Awake()
    {
        if (tilemap == null)
        {
            tilemap = GetComponent<Tilemap>();
        }
    }

    private void Start()
    {
        PaintAroundCameras();
    }

    private void LateUpdate()
    {
        PaintAroundCameras();
    }

    private void PaintAroundCameras()
    {
        if (tilemap == null || heartsTiles == null || heartsTiles.Length == 0)
        {
            return;
        }

        BoundsInt need = VisibleCells();
        if (painted.size.x > 0 && Contains(painted, need))
        {
            return;
        }

        Paint(Expand(need, chunkPad));
    }

    private BoundsInt VisibleCells()
    {
        float minX = float.PositiveInfinity;
        float minY = float.PositiveInfinity;
        float maxX = float.NegativeInfinity;
        float maxY = float.NegativeInfinity;
        bool any = false;

        Camera[] cameras = Camera.allCameras;
        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (camera == null || !camera.isActiveAndEnabled)
            {
                continue;
            }

            any = true;
            GetViewBounds(camera, out Vector3 min, out Vector3 max);
            minX = Mathf.Min(minX, min.x);
            minY = Mathf.Min(minY, min.y);
            maxX = Mathf.Max(maxX, max.x);
            maxY = Mathf.Max(maxY, max.y);
        }

        if (!any)
        {
            minX = -24f;
            minY = -24f;
            maxX = 24f;
            maxY = 24f;
        }

        minX -= margin;
        minY -= margin;
        maxX += margin;
        maxY += margin;

        Vector3Int a = tilemap.WorldToCell(new Vector3(minX, minY, 0f));
        Vector3Int b = tilemap.WorldToCell(new Vector3(maxX, maxY, 0f));
        int xMin = Mathf.Min(a.x, b.x);
        int yMin = Mathf.Min(a.y, b.y);
        int xMax = Mathf.Max(a.x, b.x);
        int yMax = Mathf.Max(a.y, b.y);
        return new BoundsInt(xMin, yMin, 0, xMax - xMin + 1, yMax - yMin + 1, 1);
    }

    private static void GetViewBounds(Camera camera, out Vector3 min, out Vector3 max)
    {
        float height = camera.orthographic ? camera.orthographicSize : 12f;
        float width = height * camera.aspect;
        Vector3 center = camera.transform.position;
        min = new Vector3(center.x - width, center.y - height, 0f);
        max = new Vector3(center.x + width, center.y + height, 0f);
    }

    private void Paint(BoundsInt area)
    {
        int width = area.size.x;
        int height = area.size.y;
        if (width <= 0 || height <= 0)
        {
            return;
        }

        TileBase[] block = new TileBase[width * height];
        int patternWidth = Mathf.Max(1, patternSize.x);
        int patternHeight = Mathf.Max(1, patternSize.y);
        int i = 0;

        for (int y = area.yMin; y < area.yMax; y++)
        {
            for (int x = area.xMin; x < area.xMax; x++)
            {
                int column = Mod(x, patternWidth);
                int rowFromTop = Mod(-y, patternHeight);
                int index = rowFromTop * patternWidth + column;
                if (index >= 0 && index < heartsTiles.Length)
                {
                    block[i] = heartsTiles[index];
                }

                i++;
            }
        }

        tilemap.SetTilesBlock(area, block);
        painted = area;
    }

    private static bool Contains(BoundsInt outer, BoundsInt inner)
    {
        return inner.xMin >= outer.xMin
            && inner.yMin >= outer.yMin
            && inner.xMax <= outer.xMax
            && inner.yMax <= outer.yMax;
    }

    private static BoundsInt Expand(BoundsInt area, int pad)
    {
        return new BoundsInt(
            area.xMin - pad,
            area.yMin - pad,
            0,
            area.size.x + pad * 2,
            area.size.y + pad * 2,
            1);
    }

    private static int Mod(int value, int modulus)
    {
        int remainder = value % modulus;
        return remainder < 0 ? remainder + modulus : remainder;
    }
}
