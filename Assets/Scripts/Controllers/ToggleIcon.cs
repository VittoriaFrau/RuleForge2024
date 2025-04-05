using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToggleIcon : MonoBehaviour
{
    public Sprite expandedIcon;
    public Sprite collapsedIcon;
    private Image iconImage;

    private void Awake()
    {
        iconImage = GetComponent<Image>();
    }

    public void SetState(bool isExpanded)
    {
        if (iconImage != null)
        {
            iconImage.sprite = isExpanded ? expandedIcon : collapsedIcon;
            // Opzionale: ruota l'icona
            transform.rotation = Quaternion.Euler(0, 0, isExpanded ? 180 : 0);
        }
    }
}

