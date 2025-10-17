using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public int slotID;
    public WPManager manager;
    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            InventoryItem itemUI = eventData.pointerDrag.GetComponent<InventoryItem>();
            itemUI.parentAfterDrag = transform;
        }
    }

}