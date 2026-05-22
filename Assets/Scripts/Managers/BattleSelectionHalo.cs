using UnityEngine;

/// <summary>
/// Resaltado por contorno/halo para los botones de los submenús de batalla
/// (APOYAR y MOCHILA). Mantiene el color base del botón y dibuja un halo
/// brillante detrás de la opción seleccionada.
/// </summary>
public static class BattleSelectionHalo
{
    private const string HaloName = "SelectHalo";
    private static readonly Color HaloColor = new Color(1f, 0.95f, 0.3f, 1f);

    /// <summary>Activa o desactiva el halo de selección sobre el botón dado.</summary>
    public static void Apply(Transform option, bool selected)
    {
        if (option == null) return;

        Transform halo = option.Find(HaloName);

        if (selected)
        {
            SpriteRenderer sr = option.GetComponent<SpriteRenderer>();
            if (sr == null || sr.sprite == null) return;

            if (halo == null)
            {
                GameObject go = new GameObject(HaloName);
                go.transform.SetParent(option, false);
                go.transform.localPosition = Vector3.zero;

                SpriteRenderer hsr = go.AddComponent<SpriteRenderer>();
                hsr.sprite = sr.sprite;
                hsr.drawMode = sr.drawMode;
                hsr.color = HaloColor;
                hsr.sortingLayerID = sr.sortingLayerID;
                hsr.sortingOrder = sr.sortingOrder - 1; // detrás del botón

                if (sr.drawMode == SpriteDrawMode.Simple)
                {
                    go.transform.localScale = Vector3.one * 1.18f;
                }
                else
                {
                    hsr.size = sr.size + new Vector2(0.25f, 0.25f);
                }

                halo = go.transform;
            }

            halo.gameObject.SetActive(true);
        }
        else if (halo != null)
        {
            halo.gameObject.SetActive(false);
        }
    }
}
