using UnityEngine;

// White plate behind a light-family ship. Uses the same sprite and URP 2D
// material as the body — a runtime-generated sprite with the default material
// is dropped by the 2D renderer, which is why the arrow player never showed
// a backing under split-screen cameras.
public class WhiteTriangleBacking : MonoBehaviour
{
    [SerializeField] private Vector3 offset = new Vector3(0.04f, 0.03f, 0.08f);
    [SerializeField] private float scale = 1.28f;

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
            existing.SyncNow();
            return existing;
        }

        return target.AddComponent<WhiteTriangleBacking>();
    }

    private void Awake()
    {
        body = GetComponent<SpriteRenderer>();
        BuildBacking();
        SyncNow();
    }

    private void LateUpdate()
    {
        SyncNow();
    }

    public void SyncNow()
    {
        if (body == null)
        {
            body = GetComponent<SpriteRenderer>();
        }

        if (backing == null || body == null)
        {
            return;
        }

        bool show = body.enabled && body.sprite != null;
        backing.enabled = show;
        if (!show)
        {
            return;
        }

        backing.sprite = body.sprite;
        backing.sharedMaterial = body.sharedMaterial;
        backing.color = Color.white;
        backing.flipX = body.flipX;
        backing.flipY = body.flipY;
        backing.drawMode = body.drawMode;
        backing.sortingLayerID = body.sortingLayerID;
        backing.sortingOrder = body.sortingOrder;
        backing.renderingLayerMask = body.renderingLayerMask;
        backing.maskInteraction = body.maskInteraction;
        backing.allowOcclusionWhenDynamic = false;

        backing.transform.localPosition = offset;
        backing.transform.localScale = new Vector3(scale, scale, 1f);
        backing.transform.localRotation = Quaternion.identity;
    }

    private void BuildBacking()
    {
        if (backing != null)
        {
            return;
        }

        GameObject child = new GameObject("WhiteBacking");
        child.layer = gameObject.layer;
        child.transform.SetParent(transform, false);
        child.transform.localPosition = offset;
        child.transform.localScale = new Vector3(scale, scale, 1f);
        backing = child.AddComponent<SpriteRenderer>();
        if (body != null)
        {
            backing.sharedMaterial = body.sharedMaterial;
            backing.sprite = body.sprite;
        }

        backing.color = Color.white;
        backing.allowOcclusionWhenDynamic = false;
    }
}
