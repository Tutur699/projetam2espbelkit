using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Slot : MonoBehaviour, IDropHandler
{
    public Image slotImage;
    public Color normalColor, selectedColor;
    [HideInInspector] public WPManager manager;
    //private Items currentItem;


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
            manager.UpdateItemAtSlot(itemUI.item, transform.GetSiblingIndex());
        }

    }
    /*public void SetItem(Items newItem)
    {
        currentItem = newItem;
        slotImage.sprite = newItem.image;
        slotImage.enabled = true;
    }
    public void ClearSlot()
    {
        currentItem = null;
        slotImage.sprite = null;
        slotImage.enabled = false;
    }*/
}