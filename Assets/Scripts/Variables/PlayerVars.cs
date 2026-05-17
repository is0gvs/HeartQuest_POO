using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVars : MonoBehaviour
{
    public float atkValue;
    public float defValue;

    public float health;
    public float maxHealth = 20;

    [SerializeField] Color soulFlashing;
    public Color soulOriginal;

    float soulAlpha;
    float soulAlphaFlash;
    float time;
    public float maxTime;

    [SerializeField] private SpriteRenderer soulSprite;

    bool invincible;

    [HideInInspector]
    public static PlayerVars instance;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        soulAlpha = soulSprite.color.a;
        soulAlphaFlash = soulAlpha / 2;
        soulOriginal = soulSprite.color;
        time = maxTime;
    }

    public void TakeDamage(float damageTaken)
    {
        if (!invincible)
        {
            health -= damageTaken;
            AudioManager.instance.takingDamage();
            invincible = true;
        }
    }

    
    public void Heal(float amount)
    {
        health += amount;

        if (health > maxHealth)
            health = maxHealth;
    }

    void FlashSoul()
    {
        if (time > 0)
        {
            soulAlphaFlash = soulAlphaFlash * -1;
            soulSprite.color = soulFlashing;
        }
        else
        {
            soulSprite.color = soulOriginal;
            time = maxTime;
            invincible = false;
        }
    }

    void Update()
    {
        time -= Time.deltaTime;

        if (invincible)
        {
            FlashSoul();
        }
    }
}