using System.Collections.Generic;
using UnityEngine;

// Puffs a short rocket exhaust out of the back of whatever it is attached to while
// that object is moving. Puffs are pooled sprites kept in a scene level holder, so
// they stay where they were emitted instead of riding along with the player, and
// they are sorted one step behind the player so they only show past its silhouette.
public class PlayerExhaust : MonoBehaviour
{
    [Header("Emission")]
    [SerializeField] private float puffsPerSecond = 10f;
    [SerializeField] private float minimumSpeed = 0.25f;
    [SerializeField] private float topSpeed = 0f;
    [SerializeField] private float spawnOffset = 0.9f;

    [Header("Puffs")]
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float size = 0.14f;
    [SerializeField] private float sizeVariation = 0.35f;
    [SerializeField] private float growth = 1.7f;
    [SerializeField] private float ejectSpeed = 0.45f;
    [SerializeField] private float sideSpread = 0.2f;
    [SerializeField] private float drag = 3.5f;

    [Header("Colour")]
    [SerializeField] private Color startColor = new Color(1f, 0.76f, 0.45f, 0.75f);
    [SerializeField] private Color endColor = new Color(0.85f, 0.3f, 0.18f, 0f);
    [SerializeField] private int sortingOrderOffset = -1;

    private const int MaxPuffsPerFrame = 4;

    private class Puff
    {
        public GameObject instance;
        public Transform tr;
        public SpriteRenderer sprite;
        public Vector2 velocity;
        public float age;
        public float size;
    }

    private static Sprite ballSprite;

    private readonly List<Puff> live = new List<Puff>();
    private readonly Stack<Puff> pool = new Stack<Puff>();

    private SpriteRenderer bodyRenderer;
    private Transform holder;
    private Vector3 lastPosition;
    private float pendingPuffs;

    private void Awake()
    {
        bodyRenderer = GetComponent<SpriteRenderer>();
        lastPosition = transform.position;

        if (topSpeed <= 0f)
        {
            PlayerController controller = GetComponent<PlayerController>();
            topSpeed = controller != null ? controller.GetMoveSpeed() : 5f;
        }
    }

    private void OnEnable()
    {
        lastPosition = transform.position;
        pendingPuffs = 0f;
    }

    private void OnDestroy()
    {
        if (holder != null)
        {
            Destroy(holder.gameObject);
        }
    }

    private void Update()
    {
        float deltaTime = Time.deltaTime;

        if (deltaTime <= 0f)
        {
            return;
        }

        Vector2 movement = transform.position - lastPosition;
        lastPosition = transform.position;

        // A scene load or a snap to a new position must not dump a whole trail at once.
        bool teleported = movement.magnitude > topSpeed * deltaTime * 8f;

        Emit(teleported ? Vector2.zero : movement / deltaTime, deltaTime);
        AgePuffs(deltaTime);
    }

    private void Emit(Vector2 velocity, float deltaTime)
    {
        float speed = velocity.magnitude;

        if (speed < minimumSpeed)
        {
            pendingPuffs = 0f;
            return;
        }

        float speedFactor = Mathf.Clamp01(speed / topSpeed);
        pendingPuffs += puffsPerSecond * speedFactor * deltaTime;

        int count = Mathf.FloorToInt(pendingPuffs);
        pendingPuffs -= count;

        Vector2 back = -velocity / speed;
        Vector2 side = new Vector2(-back.y, back.x);

        for (int i = 0; i < Mathf.Min(count, MaxPuffsPerFrame); i++)
        {
            Spawn(back, side, speed, speedFactor);
        }
    }

    private void Spawn(Vector2 back, Vector2 side, float speed, float speedFactor)
    {
        Puff puff = Take();

        Vector3 origin = transform.position + (Vector3)(back * BodyExtentAlong(back) * spawnOffset);
        origin.z = transform.position.z;
        puff.tr.position = origin;

        puff.velocity = back * (speed * ejectSpeed * Random.Range(0.75f, 1.25f))
            + side * (speed * sideSpread * Random.Range(-1f, 1f));

        puff.size = BodyShortSide() * size
            * Random.Range(1f - sizeVariation, 1f + sizeVariation)
            * Mathf.Lerp(0.6f, 1f, speedFactor);

        puff.age = 0f;
        puff.tr.localScale = new Vector3(puff.size, puff.size, 1f);
        puff.sprite.color = startColor;

        live.Add(puff);
    }

    private void AgePuffs(float deltaTime)
    {
        for (int i = live.Count - 1; i >= 0; i--)
        {
            Puff puff = live[i];
            puff.age += deltaTime;

            float t = puff.age / lifetime;

            if (t >= 1f)
            {
                puff.instance.SetActive(false);
                pool.Push(puff);
                live.RemoveAt(i);
                continue;
            }

            puff.velocity *= Mathf.Exp(-drag * deltaTime);
            puff.tr.position += (Vector3)(puff.velocity * deltaTime);

            float scale = puff.size * Mathf.Lerp(1f, growth, t);
            puff.tr.localScale = new Vector3(scale, scale, 1f);
            puff.sprite.color = Color.Lerp(startColor, endColor, t);
        }
    }

    private Puff Take()
    {
        if (pool.Count > 0)
        {
            Puff pooled = pool.Pop();
            pooled.instance.SetActive(true);
            return pooled;
        }

        GameObject instance = new GameObject("Puff");
        instance.transform.SetParent(Holder(), worldPositionStays: false);

        SpriteRenderer sprite = instance.AddComponent<SpriteRenderer>();
        sprite.sprite = BallSprite();

        if (bodyRenderer != null)
        {
            // Borrowing the body's material keeps the puffs shaded like the rest of the art.
            sprite.sharedMaterial = bodyRenderer.sharedMaterial;
            sprite.sortingLayerID = bodyRenderer.sortingLayerID;
            sprite.sortingOrder = bodyRenderer.sortingOrder + sortingOrderOffset;
        }
        else
        {
            sprite.sortingOrder = sortingOrderOffset;
        }

        return new Puff
        {
            instance = instance,
            tr = instance.transform,
            sprite = sprite
        };
    }

    private Transform Holder()
    {
        if (holder == null)
        {
            holder = new GameObject(name + " Exhaust").transform;
        }

        return holder;
    }

    // Half the body's width along the given direction, so puffs start at its trailing edge.
    private float BodyExtentAlong(Vector2 direction)
    {
        if (bodyRenderer == null)
        {
            return 0f;
        }

        Vector3 extents = bodyRenderer.bounds.extents;
        return Mathf.Abs(direction.x) * extents.x + Mathf.Abs(direction.y) * extents.y;
    }

    // Puff size follows the art rather than a fixed world size, so it stays in
    // proportion when the sprite is swapped for a combined one.
    private float BodyShortSide()
    {
        if (bodyRenderer == null || bodyRenderer.sprite == null)
        {
            return 1f;
        }

        Vector2 spriteSize = bodyRenderer.sprite.bounds.size;
        Vector3 scale = transform.lossyScale;

        return Mathf.Min(Mathf.Abs(spriteSize.x * scale.x), Mathf.Abs(spriteSize.y * scale.y));
    }

    private static Sprite BallSprite()
    {
        if (ballSprite != null)
        {
            return ballSprite;
        }

        const int resolution = 32;
        const float edgeSoftness = 0.18f;

        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.hideFlags = HideFlags.HideAndDontSave;
        texture.wrapMode = TextureWrapMode.Clamp;

        float radius = resolution * 0.5f;

        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float dx = x + 0.5f - radius;
                float dy = y + 0.5f - radius;
                float distance = Mathf.Sqrt(dx * dx + dy * dy) / radius;
                float alpha = Mathf.Clamp01((1f - distance) / edgeSoftness);

                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();

        ballSprite = Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution), new Vector2(0.5f, 0.5f), resolution);
        ballSprite.hideFlags = HideFlags.HideAndDontSave;

        return ballSprite;
    }
}
