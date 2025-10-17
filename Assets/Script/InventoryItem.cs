using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Items item;
    [HideInInspector]public Image itemImage;
    [HideInInspector]public Transform parentAfterDrag;
    // Start is called before the first frame update
    public void InitializeItem(Items newItem)
    {
        item = newItem;
        if (itemImage == null)
        {
            itemImage = GetComponent<Image>();
        }
        itemImage.sprite = newItem.image;   

    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemImage.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        itemImage.raycastTarget = true;
        transform.SetParent(parentAfterDrag);
    }
}
