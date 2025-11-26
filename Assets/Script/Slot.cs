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
    private Items currentItem;


     public void Awake()
     {
         Deselect();
     }

     public void Select()
     {
        Debug.Log("Slot selected");
        slotImage.color = selectedColor;
     }
     public void Deselect()
     {
        Debug.Log("Slot deselected");
        slotImage.color = normalColor;
     }
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            InventoryItem itemUI = eventData.pointerDrag.GetComponent<InventoryItem>();
            if (!IsEmpty())
            {
                return;
            }
            Transform oldParent = itemUI.parentAfterDrag;
            itemUI.parentAfterDrag = transform;

            int oldIndex = oldParent.GetSiblingIndex();
            int newIndex = transform.GetSiblingIndex();
            manager.MoveItemSlot(oldIndex, newIndex);
            Debug.Log("Moved item from slot " + oldIndex + " to slot " + newIndex);
        }

    }

    public void SetItem(Items newItem)
    {
        currentItem = newItem;
        InventoryItem itemUI = GetComponentInChildren<InventoryItem>();
        if (itemUI != null)
        {
            itemUI.InitializeItem(newItem);
        }
    }
     public bool IsEmpty()
    {
        return currentItem == null;
    }
}