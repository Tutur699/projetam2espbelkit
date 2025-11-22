using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Items item;
    public Image itemImage;
    [HideInInspector] public Transform parentAfterDrag;
    // Start is called before the first frame update
    
    private void Start()
    {
        if (item != null)
        {
            InitializeItem(item);
        }
    }
    public void InitializeItem(Items newItem)
    {
        if (newItem == null)
        {
            Debug.LogWarning("InitializeItem() received a null item.");
            return;
        }
        item = newItem;
        if (newItem.image != null)
        {
            itemImage.sprite = newItem.image;
            itemImage.enabled = true;
        }
        else
        {
            Debug.LogWarning("Item image is null for item: " + newItem.name);
            itemImage.enabled = false;
        }
    }
        

    public void OnBeginDrag(PointerEventData eventData)
    {
        itemImage.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
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
