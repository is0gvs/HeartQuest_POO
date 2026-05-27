using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemButtons : MonoBehaviour
{
    [HideInInspector]
    public GameObject instance;
    [HideInInspector]
    public bool selected;
    public Transform soulPosition;
    public string itemName;
    public float itemHeal;

    /// <summary>
    /// Indica si este slot está vacío (sin ítem asignado).
    /// Se controla desde ItemManager.SyncFromInventory().
    /// </summary>
    [HideInInspector]
    public bool isEmpty;
}
