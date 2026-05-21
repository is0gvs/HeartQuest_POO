using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    [HideInInspector]
    public bool shouldTalk;
    public TextMeshPro text;
    public GameObject enemyTextBackground;
    public TextMeshPro textEnemy;
    public string dialogueTxt;
    public string enemyTxt;
    public AudioClip clip;
    public AudioClip enemyClip;
    private GameObject audioHolder;
    private List<AudioSource> sources;
    public bool done = false;
    private bool canNarrate;
    public float talkingSpeed = 0.1f;
    [HideInInspector]
    public static DialogueManager instance;
    void Awake() => instance = this;

    public void Talking(Action talkAction)
    {
        if (canNarrate)
        {
            ApplyBattleBoxTextLayout();
            StartCoroutine(StartTalking(talkAction));
        }
        
    }
    void Start()
    {
        sources = new List<AudioSource>();
        audioHolder = new GameObject("Audio Holder");
        audioHolder.transform.parent = transform;
        canNarrate = true;

        ApplyBattleBoxTextLayout();

        // Fix: same off-screen pivot issue as ActingText
        if (text != null)
        {
            text.enableAutoSizing = false;
            text.enableWordWrapping = true;
            text.alignment = TextAlignmentOptions.TopLeft;
        }

        if (textEnemy != null)
        {
            textEnemy.fontSize = 1.85f;
            textEnemy.enableAutoSizing = false;
            textEnemy.enableWordWrapping = true;
        }

        StartCoroutine(StartTalking(null));
    }

    private void ApplyBattleBoxTextLayout()
    {
        if (text == null) return;

        text.rectTransform.pivot = new Vector2(0f, 1f);
        text.rectTransform.sizeDelta = new Vector2(7.65f, 1.75f);
        text.fontSize = 1.75f;
        text.lineSpacing = -2f;
        text.enableAutoSizing = false;
        text.enableWordWrapping = true;
        text.alignment = TextAlignmentOptions.TopLeft;

        BattleManager bm = BattleManager.battleInstance;
        if (bm != null && bm.battleBox != null)
        {
            Vector3 center = bm.battleBox.transform.position;
            Vector2 size = bm.battleBox.size;
            text.transform.position = center + new Vector3(-size.x * 0.43f, size.y * 0.34f, 0f);
        }
        else
        {
            text.transform.position = new Vector3(-3.8f, -0.45f, 0f);
        }
    }
    IEnumerator StartTalking(Action action)
    {
        done = false;
        canNarrate = false;
        char[] chars = dialogueTxt.ToCharArray();
        text.text = "";
        for (int i = 0; i < chars.Length; i++)
        {
            AudioSource s = audioHolder.AddComponent<AudioSource>();
            text.text += chars[i];
            s.clip = clip;
            s.pitch = UnityEngine.Random.Range(0.99f,1);
            s.Play();
            sources.Add(s);
            yield return new WaitForSeconds(talkingSpeed);

        }
        
        int playingSources = 0;
        do
        {
            playingSources = 0;

            for (int i = 0; i < sources.Count; i++)
            {
                if (!sources[i].isPlaying)
                {
                    Destroy(sources[i]);
                    sources.RemoveAt(i);
                    i--;
                }
                else
                    playingSources++;
            }
            yield return null;
        }
        while (playingSources > 0);
        if (shouldTalk)
        {
            StartCoroutine(EnemyTalking(action));
            text.text = "";
        }
        else
        {
            done = true;
            canNarrate = true;
            enemyTextBackground.SetActive(false);
            action?.Invoke();
        }
        
    }

    IEnumerator EnemyTalking(Action action)
    {
        done = false;
        canNarrate = false;
        enemyTextBackground.SetActive(true);
        char[] chars = enemyTxt.ToCharArray();
        textEnemy.text = "";
        for (int i = 0; i < chars.Length; i++)
        {
            AudioSource s = audioHolder.AddComponent<AudioSource>();
            textEnemy.text += chars[i];
            s.clip = clip;
            s.pitch = UnityEngine.Random.Range(0.99f, 1);
            s.Play();
            sources.Add(s);
            yield return new WaitForSeconds(talkingSpeed);

        }
        int playingSources = 0;
        do
        {
            playingSources = 0;

            for (int i = 0; i < sources.Count; i++)
            {
                if (!sources[i].isPlaying)
                {
                    Destroy(sources[i]);
                    sources.RemoveAt(i);
                    i--;
                }
                else
                    playingSources++;
            }
            yield return null;
        }
        while (playingSources > 0);
        done = true;
        canNarrate = true;
        enemyTextBackground.SetActive(false);
        action?.Invoke();
    }
}
