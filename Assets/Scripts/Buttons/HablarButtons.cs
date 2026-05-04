using UnityEngine;

/// <summary>
/// Visual state component for each button in the HABLAR submenu.
/// Highlights the SpriteRenderer yellow when this button is selected.
/// </summary>
public class HablarButtons : MonoBehaviour
{
    /// <summary>World-space position where the soul sprite should sit when this button is selected.</summary>
    public Transform soulPosition;

    /// <summary>Whether this button is currently selected by the player.</summary>
    [HideInInspector]
    public bool selected;

    private SpriteRenderer sr;
    private Color normalColor;
    private static readonly Color selectedColor = new Color(1f, 0.92f, 0.016f); // yellow

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            normalColor = sr.color;
    }

    void Update()
    {
        if (sr == null) return;
        sr.color = selected ? selectedColor : normalColor;
    }
}
