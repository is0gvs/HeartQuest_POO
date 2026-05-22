using System;
using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Self-contained heart dodge minigame.
/// The soul moves freely in four directions inside the battle box.
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
    [Header("Movement")]
    public float moveSpeed = 4.25f;

    [Header("Pellets")]
    public float pelletSpeed      = 2.65f;
    public float spawnInterval    = 0.4f;
    public float minigameDuration = 6.5f;
    public int   pelletDamage     = 1;

    [Header("Márgenes del área de juego")]
    [Tooltip("Margen interno horizontal (izq/der) — reduce el área jugable respecto a la BattleBox")]
    public float boundsMarginX = 0.30f;
    [Tooltip("Margen interno vertical (arr/abj) — reduce el área jugable respecto a la BattleBox")]
    public float boundsMarginY = 0.25f;
    private readonly string[] attackWords = { "Burla", "Rumor", "Insulto", "Empujon", "Amenaza", "Risa" };

    // ── Runtime ───────────────────────────────────────────────────────────
    private bool      isActive;
    private Action    onComplete;
    private Coroutine spawnCo;
    private Coroutine timerCo;
    private Transform soulTransform;
    private Rect      bounds;
    private Vector2   velocity;
    private float     soulRadius = 0.22f;
    private PlayerVars playerVars;

    void Awake()
    {
        if (_instance == null) _instance = this;
        else if (_instance != this) { Destroy(gameObject); return; }
    }

    void Update()
    {
        if (!isActive || soulTransform == null) return;
        RecomputeBounds();
        MoveSoul();
    }

    /// <summary>
    /// Recalcula el área jugable a partir del tamaño ACTUAL de la BattleBox.
    /// Se llama cada frame para que el corazón siga a la caja mientras se anima
    /// (encoge/crece), evitando que quede fuera de los límites visibles.
    /// </summary>
    private void RecomputeBounds()
    {
        BattleManager bm = BattleManager.battleInstance;
        if (bm == null || bm.battleBox == null) return;

        // Usamos los bounds REALES del SpriteRenderer (AABB mundial): contemplan
        // pivote, escala y posición, así el área jugable coincide con la caja visible
        // y el corazón no se sale de ella.
        Bounds b = bm.battleBox.bounds;
        bounds = new Rect(
            b.min.x + boundsMarginX,
            b.min.y + boundsMarginY,
            Mathf.Max(0.5f, b.size.x - boundsMarginX * 2f),
            Mathf.Max(0.5f, b.size.y - boundsMarginY * 2f));
    }

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Starts the dodge minigame. Auto-finds the soul and calculates
    /// the battle bounds from BattleManager.battleBox.
    /// </summary>
    public void StartMinigame(Action callback)
    {
        if (isActive)
        {
            End(false);
        }

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
        bounds = new Rect(-1.5f, -1.2f, 3f, 2.4f); // fallback inicial
        RecomputeBounds();

        onComplete = callback;
        isActive   = true;
        velocity   = Vector2.zero;
        playerVars = soulTransform.GetComponent<PlayerVars>();
        if (playerVars != null) playerVars.SetSoulColorLock(true);

        Rigidbody2D rb = soulTransform.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Enable and place the soul at the center of the dodge box.
        SpriteRenderer sr = soulTransform.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingOrder = 20; // render above the battle box
            sr.color = playerVars != null ? new Color(playerVars.soulOriginal.r, playerVars.soulOriginal.g, playerVars.soulOriginal.b, 1f) : sr.color;
            soulTransform.localScale = Vector3.one * 0.24f;
        }

        CircleCollider2D soulCollider = soulTransform.GetComponent<CircleCollider2D>();
        if (soulCollider != null)
        {
            soulRadius = 0.18f;
            soulCollider.radius = soulRadius;
            soulCollider.isTrigger = true;
        }

        soulTransform.position = new Vector3(bounds.center.x, bounds.center.y, 0f);

        spawnCo = StartCoroutine(SpawnLoop());
        timerCo = StartCoroutine(Timer());

        Debug.Log("HeartMinigame: ¡minijuego iniciado! Esquiva los ataques.");
    }

    /// <summary>Immediately stops without calling onComplete.</summary>
    public void StopMinigame() => End(false);

    // ── Internal movement ────────────────────────────────────────────────

    private void MoveSoul()
    {
        float dt = Time.deltaTime;

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        velocity = new Vector2(h, v);
        if (velocity.sqrMagnitude > 1f) velocity.Normalize();
        velocity *= moveSpeed;

        Vector3 pos = soulTransform.position;
        pos.x += velocity.x * dt;
        pos.y += velocity.y * dt;

        pos.x = Mathf.Clamp(pos.x, bounds.xMin + soulRadius, bounds.xMax - soulRadius);
        pos.y = Mathf.Clamp(pos.y, bounds.yMin + soulRadius, bounds.yMax - soulRadius);

        soulTransform.position = pos;
    }

    private IEnumerator SpawnLoop()
    {
        while (isActive)
        {
            SpawnPellet();
            yield return new WaitForSecondsRealtime(spawnInterval);
        }
    }

    private IEnumerator Timer()
    {
        yield return new WaitForSecondsRealtime(minigameDuration);
        End(true);
    }

    private void SpawnPellet()
    {
        if (soulTransform == null) return;

        GameObject go = new GameObject("WordPellet");

        TextMeshPro text = go.AddComponent<TextMeshPro>();
        text.text = attackWords[UnityEngine.Random.Range(0, attackWords.Length)];
        text.fontSize = 1.45f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = new Color(1f, 0.9f, 0.25f);
        text.fontStyle = FontStyles.Bold;
        text.sortingOrder = 18;
        text.enableAutoSizing = false;
        text.rectTransform.sizeDelta = new Vector2(3.1f, 0.9f);

        BoxCollider2D col = go.AddComponent<BoxCollider2D>();
        col.isTrigger = true;
        col.size = new Vector2(1.45f, 0.46f);

        Vector2 spawnPos = RandomEdge();
        go.transform.position = new Vector3(spawnPos.x, spawnPos.y, 0f);

        Vector2 target = new Vector2(
            UnityEngine.Random.Range(bounds.xMin + 0.4f, bounds.xMax - 0.4f),
            UnityEngine.Random.Range(bounds.yMin + 0.3f, bounds.yMax - 0.3f)
        );
        Vector2 dir = (target - spawnPos).normalized;

        WordPellet wp = go.AddComponent<WordPellet>();
        wp.Init(dir, pelletSpeed, bounds, pelletDamage, soulTransform);
    }

    private Vector2 RandomEdge()
    {
        int edge = UnityEngine.Random.Range(0, 4);
        float margin = 1.35f;
        return edge switch
        {
            0 => new Vector2(UnityEngine.Random.Range(bounds.xMin, bounds.xMax), bounds.yMax + margin),
            1 => new Vector2(UnityEngine.Random.Range(bounds.xMin, bounds.xMax), bounds.yMin - margin),
            2 => new Vector2(bounds.xMin - margin, UnityEngine.Random.Range(bounds.yMin, bounds.yMax)),
            _ => new Vector2(bounds.xMax + margin, UnityEngine.Random.Range(bounds.yMin, bounds.yMax)),
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

        if (playerVars != null)
        {
            playerVars.SetSoulColorLock(false);
            playerVars = null;
        }

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
