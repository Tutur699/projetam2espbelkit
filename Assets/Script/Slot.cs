using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler
{
    public Image slotImage;
    public Color normalColor, selectedColor;

     public void Awake()
     {
         Deselect();
     }

     public void Select()
     {
        slotImage.color = selectedColor;
     }
     public void Deselect()
     {
        slotImage.color = normalColor;
     }
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            GameObject droopedItem = eventData.pointerDrag;
            InventoryItem itemUI = droopedItem.GetComponent<InventoryItem>();
            itemUI.parentAfterDrag = transform;
        }

    }
}