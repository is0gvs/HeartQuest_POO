using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour
{
    [HideInInspector]
    public GameObject instance;
    [HideInInspector]
    public Sprite currentSprite;
    [HideInInspector]
    public Sprite instanceSprite;
    public Sprite buttonDeselected;
    public Sprite buttonSelected;
    public bool selected;
    public Transform soulPosition;
    private SpriteRenderer sr;

    void Awake()
    {
        instance = this.gameObject;
        sr = instance.GetComponent<SpriteRenderer>();
        instanceSprite = sr.sprite;
        currentSprite = instanceSprite;
    }

    void Update()
    {
        sr.sprite = currentSprite;
    }
}
