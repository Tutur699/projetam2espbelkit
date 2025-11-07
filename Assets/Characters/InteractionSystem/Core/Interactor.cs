using UnityEngine;

public class Interactor : MonoBehaviour
{
    public WPManager wpManager;
    public Raycast_player raycastPlayer;
    Interactable currentInteractable;
    ItemPickable currentPickable;

    private void Update()
    {
        checkInteraction();
        if (Input.GetKeyDown(raycastPlayer.key) && currentPickable != null)
        {
            currentPickable.Interact();
        }
    }
    void checkInteraction()
    {
        RaycastHit hitInfo;
        Ray ray = new Ray(wpManager.playerCamera.transform.position, wpManager.playerCamera.transform.forward);
        if (Physics.Raycast(ray, out hitInfo, raycastPlayer.distmax))
        {
            if (hitInfo.collider.tag == "Interactable") //if the object hit by the ray is an interactable object
            {
                Interactable newInteractable = hitInfo.collider.GetComponent<Interactable>();
                if (currentInteractable && newInteractable != currentInteractable) //if we are already looking at an interactable object but it's not the same as the new one
                {
                    currentInteractable.DisableOutline();
                }
                if (newInteractable.enabled)
                {
                    SetNewCurrentInteractable(newInteractable);
                }
                else //if the interactable component is disabled
                {
                    DisableCurrentInteractable();
                }
            }
            if (hitInfo.collider.tag == "Pickable")
            {
                ItemPickable itemPick = hitInfo.collider.GetComponent<ItemPickable>();
                Debug.Log($"Pickable = {itemPick.name}, Scriptable = {itemPick.itemScriptable}, sprite = {itemPick.itemScriptable.image}");
                if (currentPickable && itemPick != currentPickable)
                {
                    currentPickable.DisableOutline();
                }
                if (itemPick.enabled)
                {
                    SetNewCurrentInteractable(itemPick);
                }
                if (itemPick != null)
                {
                    if (Input.GetKeyDown(raycastPlayer.key))
                    {
                        wpManager.AddItem(hitInfo.collider.GetComponent<ItemPickable>().itemScriptable, hitInfo.collider.GetComponent<ItemPickable>().itemP);
                        itemPick.PickItem();
                    }
                }
                else //if the pickable component is disabled
                {
                    DisableCurrentInteractable();
                }

            }
            else
            {
                DisableCurrentInteractable();
            }
        }
    }

    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        currentInteractable = newInteractable;
        currentInteractable.EnableOutline();
        HUDController.instance.EnableInteractionText(currentInteractable.message);
    }

    void SetNewCurrentInteractable(ItemPickable item)
    {
        currentPickable = item;
        currentPickable.EnableOutline();
        HUDController.instance.EnableInteractionText(currentPickable.message);
    }

    void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();
        if (currentInteractable)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
        if (currentPickable)
        {
            currentPickable.DisableOutline();
            currentPickable = null;
        }
    }
}

