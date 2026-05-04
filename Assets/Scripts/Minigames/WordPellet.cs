using UnityEngine;

/// <summary>
/// A word-pellet fired by the bully during the heart dodge minigame.
/// Moves in a straight line, deals damage on contact with the soul, and
/// self-destructs when it leaves the battle bounds or after 5 seconds.
/// </summary>
public class WordPellet : MonoBehaviour
{
    private Vector2   direction;
    private float     speed;
    private Rect      bounds;
    private int       damage;
    private Transform soulTransform;
    private bool      ready;

    /// <summary>
    /// Called by HeartMinigame immediately after instantiation to configure movement.
    /// </summary>
    public void Init(Vector2 dir, float spd, Rect battleBounds, int dmg, Transform soul)
    {
        direction     = dir.normalized;
        speed         = spd;
        bounds        = battleBounds;
        damage        = dmg;
        soulTransform = soul;
        ready         = true;
        Destroy(gameObject, 5f);
    }

    void Update()
    {
        if (!ready) return;
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        Vector3 p = transform.position;
        if (p.x < bounds.xMin - 1f || p.x > bounds.xMax + 1f ||
            p.y < bounds.yMin - 1f || p.y > bounds.yMax + 1f)
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (soulTransform == null) return;
        if (other.transform == soulTransform)
        {
            PlayerVars pv = other.GetComponent<PlayerVars>();
            if (pv != null) pv.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
