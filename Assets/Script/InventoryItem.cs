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
            Debug.Log("Initializing item: " + item.name);
            InitializeItem(item);
        }
        if (itemImage == null)
        {
            Debug.LogError("Item image is not assigned in InventoryItem.");
        }
    }
    public void InitializeItem(Items newItem) 
    {
        if (newItem == null)
        {
            Debug.LogError("Trying to initialize with null item");
            return;
        }
        
        item = newItem;
        
        if (newItem.image != null)
        {
            itemImage.sprite = newItem.image;
            itemImage.enabled = true;
            Debug.Log("Image set to: " + newItem.image.name);
        }
        else
        {
            Debug.LogError("Item image is null for: " + newItem.name);
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
