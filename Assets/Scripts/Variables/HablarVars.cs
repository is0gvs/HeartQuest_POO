using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data component attached to each HABLAR submenu button.
/// Holds rotating dialogue lines and mercy increment values.
/// </summary>
public class HablarVars : MonoBehaviour
{
    /// <summary>Rotating list of player dialogue lines for this option.</summary>
    public List<string> dialogueTxt = new List<string>();

    /// <summary>Rotating list of mercy increment values per use.</summary>
    public List<int> mercyValue = new List<int>();

    /// <summary>Returns the current (first) dialogue line.</summary>
    public string GetDialogue() => dialogueTxt.Count > 0 ? dialogueTxt[0] : string.Empty;

    /// <summary>Returns the current (first) mercy value.</summary>
    public int GetMercy() => mercyValue.Count > 0 ? mercyValue[0] : 0;

    /// <summary>
    /// Rotates both lists: moves the first element to the end,
    /// so the next use shows a different line and mercy value.
    /// </summary>
    public void RotateLists()
    {
        if (dialogueTxt.Count > 1)
        {
            string first = dialogueTxt[0];
            dialogueTxt.RemoveAt(0);
            dialogueTxt.Add(first);
        }
        if (mercyValue.Count > 1)
        {
            int first = mercyValue[0];
            mercyValue.RemoveAt(0);
            mercyValue.Add(first);
        }
    }
}
