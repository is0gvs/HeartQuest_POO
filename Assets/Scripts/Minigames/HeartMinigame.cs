using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Self-contained heart dodge minigame with GRAVITY.
/// The soul falls downward and can jump (press-only) off the bottom wall.
/// Left/Right movement is free. Auto-creates if no instance exists.
/// </summary>
public class HeartMinigame : MonoBehaviour
{
    /// <summary>Singleton instance. Auto-creates on first access.</summary>
    public static HeartMinigame instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("HeartMinigame_Runtime");
                _instance = go.AddComponent<HeartMinigame>();
            }
            return _instance;
        }
    }
    private static HeartMinigame _instance;

    // ── Config (Inspector-tunable) ────────────────────────────────────────
    [Header("Gravity")]
    public float gravityForce    = 9f;
    public float jumpForce       = 7f;
    public float horizontalSpeed = 4f;

    [Header("Pellets")]
    public float pelletSpeed      = 3.5f;
    public float spawnInterval    = 1.2f;
    public float minigameDuration = 6f;
    public int   pelletDamage     = 1;

    // ── Runtime ───────────────────────────────────────────────────────────
    private bool      isActive;
    private Action    onComplete;
    private Coroutine spawnCo;
    private Coroutine timerCo;
    private Transform soulTransform;
    private Rect      bounds;
    private Vector2   velocity;      // manual physics velocity

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
    }

    void Update()
    {
        if (!isActive || soulTransform == null) return;
        MoveSoul();
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the dodge minigame. Auto-finds the soul and calculates
    /// the battle bounds from BattleManager.battleBox.
    /// </summary>
    public void StartMinigame(Action callback)
    {
        if (isActive) return;

        // ── Find soul ────────────────────────────────────────────────────
        BattleManager bm = BattleManager.battleInstance;
        if (bm != null && bm.soul != null)
        {
            soulTransform = bm.soul.transform;
        }
        else
        {
            PlayerVars pv = PlayerVars.instance;
            if (pv != null) soulTransform = pv.transform;
        }

        if (soulTransform == null)
        {
            Debug.LogWarning("HeartMinigame: no se encontró el alma. Saltando minijuego.");
            callback?.Invoke();
            return;
        }

        // ── Calculate bounds from the battle box ─────────────────────────
        if (bm != null && bm.battleBox != null)
        {
            Vector2 center = bm.battleBox.transform.position;
            Vector2 size   = bm.battleBox.size;
            bounds = new Rect(center.x - size.x / 2f, center.y - size.y / 2f, size.x, size.y);
        }
        else
        {
            bounds = new Rect(-1.5f, -1.2f, 3f, 2.4f);
        }

        onComplete = callback;
        isActive   = true;
        velocity   = Vector2.zero;

        // Enable and place the soul at the bottom-center (grounded)
        SpriteRenderer sr = soulTransform.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingOrder = 5; // render above the battle box
        }
        soulTransform.position = new Vector3(bounds.center.x, bounds.yMin, 0f);

        spawnCo = StartCoroutine(SpawnLoop());
        timerCo = StartCoroutine(Timer());

        Debug.Log("HeartMinigame: ¡minijuego iniciado! Esquiva los ataques.");
    }

    /// <summary>Immediately stops without calling onComplete.</summary>
    public void StopMinigame() => End(false);

    // ── Internal — Gravity-based movement ─────────────────────────────────

    private void MoveSoul()
    {
        float dt = Time.deltaTime;

        // ── Gravity (always pulling down) ────────────────────────────────
        velocity.y -= gravityForce * dt;

        // ── Horizontal input ─────────────────────────────────────────────
        float h = Input.GetAxisRaw("Horizontal");
        velocity.x = h * horizontalSpeed;

        // ── Jump (press-only, when grounded) ─────────────────────────────
        bool grounded = soulTransform.position.y <= bounds.yMin + 0.05f;
        if (grounded && (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(KeyCode.JoystickButton0)))
        {
            velocity.y = jumpForce;
        }

        // ── Apply velocity ───────────────────────────────────────────────
        Vector3 pos = soulTransform.position;
        pos.x += velocity.x * dt;
        pos.y += velocity.y * dt;

        // ── Clamp inside battleBounds ────────────────────────────────────
        // Bottom wall
        if (pos.y <= bounds.yMin)
        {
            pos.y = bounds.yMin;
            velocity.y = 0f;
        }
        // Top wall
        if (pos.y >= bounds.yMax)
        {
            pos.y = bounds.yMax;
            velocity.y = 0f;
        }
        // Left wall
        if (pos.x <= bounds.xMin)
        {
            pos.x = bounds.xMin;
            velocity.x = 0f;
        }
        // Right wall
        if (pos.x >= bounds.xMax)
        {
            pos.x = bounds.xMax;
            velocity.x = 0f;
        }

        soulTransform.position = pos;
    }

    private IEnumerator SpawnLoop()
    {
        while (isActive)
        {
            SpawnPellet();
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSeconds(minigameDuration);
        End(true);
    }

    private void SpawnPellet()
    {
        if (soulTransform == null) return;

        GameObject go = new GameObject("WordPellet");

        SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = MakeSquareSprite();
        sr.color  = new Color(1f, 0.3f, 0.3f);
        sr.sortingOrder = 10;

        CircleCollider2D col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius    = 0.15f;
        go.transform.localScale = Vector3.one * 0.35f;

        Vector2 spawnPos = RandomEdge();
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

        Vector2 dir = ((Vector2)soulTransform.position - spawnPos).normalized;

        WordPellet wp = go.AddComponent<WordPellet>();
        wp.Init(dir, pelletSpeed, bounds, pelletDamage, soulTransform);
    }

    private Vector2 RandomEdge()
    {
        int edge = UnityEngine.Random.Range(0, 4);
        return edge switch
        {
            0 => new Vector2(UnityEngine.Random.Range(bounds.xMin, bounds.xMax), bounds.yMax),
            1 => new Vector2(UnityEngine.Random.Range(bounds.xMin, bounds.xMax), bounds.yMin),
            2 => new Vector2(bounds.xMin, UnityEngine.Random.Range(bounds.yMin, bounds.yMax)),
            _ => new Vector2(bounds.xMax, UnityEngine.Random.Range(bounds.yMin, bounds.yMax)),
        };
    }

    private void End(bool callComplete)
    {
        if (!isActive) return;
        isActive = false;
        velocity = Vector2.zero;

        if (spawnCo != null) StopCoroutine(spawnCo);
        if (timerCo != null) StopCoroutine(timerCo);

        foreach (WordPellet wp in FindObjectsByType<WordPellet>(FindObjectsSortMode.None))
            Destroy(wp.gameObject);

        Debug.Log("HeartMinigame: minijuego terminado.");
        if (callComplete) onComplete?.Invoke();
    }

    // ── Sprite helper ─────────────────────────────────────────────────────
    private static Sprite _squareSprite;
    private static Sprite MakeSquareSprite()
    {
        if (_squareSprite != null) return _squareSprite;
        Texture2D tex = new Texture2D(16, 16);
        Color[] pixels = new Color[256];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.SetPixels(pixels);
        tex.Apply();
        _squareSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
        return _squareSprite;
    }
}
