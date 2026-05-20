using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    PlayerMovement player;
    public Sprite brokenSoul;
    public AudioClip soulBreakSfx;
    public AudioClip gameOverMusic;
    public AudioSource sfxPlayer;
    public AudioSource musicPlayer;
    public ParticleSystem deathParticles;
    public GameObject battleObjects;
    private SpriteRenderer playerSprite;
    public GameObject gameOverScreen;
    bool initiating;
    void Start()
    {
        player = FindAnyObjectByType<PlayerMovement>();
        if (player != null) playerSprite = player.GetComponent<SpriteRenderer>();
        EnsureGameOverScreen();
    }
    void Update()
    {
        PlayerVars vars = PlayerVars.instance;
        if (vars == null && player != null) vars = player.GetComponent<PlayerVars>();

        if (vars != null && vars.health <= 0 && !initiating)
        {
            StartCoroutine(DeathSequence());
        }    
    }
    IEnumerator DeathSequence()
    {
        initiating = true;
        HeartMinigame.instance.StopMinigame();

        PlayerVars vars = PlayerVars.instance;
        if (player != null)
        {
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null) rb.linearVelocity = Vector2.zero;
            player.enabled = false;
        }

        if (playerSprite != null && vars != null)
        {
            playerSprite.color = vars.soulOriginal;
        }

        Time.timeScale = 1f;

        initiating = true;
        if (battleObjects != null) battleObjects.SetActive(false);
        if (musicPlayer != null) musicPlayer.clip = null;

        yield return new WaitForSecondsRealtime(0.35f);

        if (playerSprite != null && brokenSoul != null) playerSprite.sprite = brokenSoul;
        if (sfxPlayer != null && soulBreakSfx != null)
        {
            sfxPlayer.clip = soulBreakSfx;
            sfxPlayer.Play();
        }

        yield return new WaitForSecondsRealtime(0.75f);

        if (playerSprite != null) playerSprite.enabled = false;
        if (deathParticles != null) deathParticles.Play();

        yield return new WaitForSecondsRealtime(0.6f);

        if (musicPlayer != null && gameOverMusic != null)
        {
            musicPlayer.clip = gameOverMusic;
            musicPlayer.Play();
        }

        EnsureGameOverScreen();
        gameOverScreen.SetActive(true);
        yield return new WaitForSecondsRealtime(2.2f);

        SceneManager.LoadScene("MainMenu");
    }

    private void EnsureGameOverScreen()
    {
        if (gameOverScreen == null)
        {
            gameOverScreen = new GameObject("GameOverScreen_Runtime");
        }
        gameOverScreen.transform.SetParent(null, true);

        SpriteRenderer bg = gameOverScreen.GetComponent<SpriteRenderer>();
        if (bg == null) bg = gameOverScreen.AddComponent<SpriteRenderer>();
        bg.sprite = MakeSolidSprite();
        bg.drawMode = SpriteDrawMode.Sliced;
        bg.size = new Vector2(20f, 12f);
        bg.color = Color.black;
        bg.sortingOrder = 100;
        gameOverScreen.transform.position = Vector3.zero;

        TextMeshPro title = gameOverScreen.GetComponentInChildren<TextMeshPro>();
        if (title == null)
        {
            GameObject textObj = new GameObject("GameOverText", typeof(TextMeshPro));
            textObj.transform.SetParent(gameOverScreen.transform, false);
            title = textObj.GetComponent<TextMeshPro>();
        }

        title.text = "GAME OVER\n<color=#ff4d6d>Tu corazon se rompio</color>\n<color=#ffffff>Volviendo al menu...</color>";
        title.alignment = TextAlignmentOptions.Center;
        title.fontStyle = FontStyles.Bold;
        title.fontSize = 1.25f;
        title.enableAutoSizing = false;
        title.color = Color.white;
        title.sortingOrder = 101;
        title.rectTransform.sizeDelta = new Vector2(12f, 4f);
        title.transform.localPosition = new Vector3(0f, -0.25f, 0f);

        Transform heart = gameOverScreen.transform.Find("BrokenHeartPixel");
        SpriteRenderer heartSr;
        if (heart == null)
        {
            GameObject heartObj = new GameObject("BrokenHeartPixel");
            heartObj.transform.SetParent(gameOverScreen.transform, false);
            heartSr = heartObj.AddComponent<SpriteRenderer>();
        }
        else
        {
            heartSr = heart.GetComponent<SpriteRenderer>();
            if (heartSr == null) heartSr = heart.gameObject.AddComponent<SpriteRenderer>();
        }

        heartSr.sprite = MakeBrokenHeartSprite();
        heartSr.sortingOrder = 102;
        heartSr.transform.localPosition = new Vector3(0f, 2.25f, 0f);
        heartSr.transform.localScale = Vector3.one * 0.28f;

        gameOverScreen.SetActive(false);
    }

    private static Sprite solidSprite;
    private static Sprite MakeSolidSprite()
    {
        if (solidSprite != null) return solidSprite;

        Texture2D tex = new Texture2D(8, 8);
        Color[] pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        tex.filterMode = FilterMode.Point;
        tex.SetPixels(pixels);
        tex.Apply();
        solidSprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 1f);
        return solidSprite;
    }

    private static Sprite brokenHeartSprite;
    private static Sprite MakeBrokenHeartSprite()
    {
        if (brokenHeartSprite != null) return brokenHeartSprite;

        string[] rows =
        {
            "0011001100",
            "0111111110",
            "1111011111",
            "1110001111",
            "0111011110",
            "0011111100",
            "0001111000",
            "0000110000"
        };

        int width = rows[0].Length;
        int height = rows.Length;
        Texture2D tex = new Texture2D(width, height);
        tex.filterMode = FilterMode.Point;

        for (int y = 0; y < height; y++)
        {
            string row = rows[height - 1 - y];
            for (int x = 0; x < width; x++)
            {
                tex.SetPixel(x, y, row[x] == '1' ? new Color(1f, 0f, 0.45f, 1f) : Color.clear);
            }
        }

        tex.Apply();
        brokenHeartSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 1f);
        return brokenHeartSprite;
    }
}
