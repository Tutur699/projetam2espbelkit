using UnityEngine;

public class Interactor : MonoBehaviour
{
    public WPManager wpManager;
    public float interactionRange = 3f;
    Interactable currentInteractable;

    private void Update()
    {
        checkInteraction();
        if (Input.GetKeyDown(KeyCode.F) && currentInteractable != null)
        {
            currentInteractable.Interact();
        }
    }

    void checkInteraction()
    {
        RaycastHit hitInfo;
        Ray ray = new Ray(wpManager.playerCamera.transform.position, wpManager.playerCamera.transform.forward);
        if (Physics.Raycast(ray, out hitInfo, interactionRange))
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
            else
            {
                DisableCurrentInteractable();
            }
            /*if (hitInfo.collider.CompareTag("Objet") && wpManager != null) //if the object hit by the ray is an item
            {
                PItems item = hitInfo.collider.GetComponent<PItems>();
                Items itemData = hitInfo.collider.GetComponent<Items>();
                Interactable newInteractable = hitInfo.collider.GetComponent<Interactable>();
                if (currentInteractable && newInteractable != currentInteractable) //if we are already looking at an interactable object but it's not the same as the new one
                {
                    DisableCurrentInteractable();
                }
                if (newInteractable.enabled)
                {
                    SetNewCurrentInteractable(newInteractable);
                }
                else //if the interactable component is disabled
                {
                    DisableCurrentInteractable();
                }
                if (Input.GetKeyDown(KeyCode.E))
                {
                    wpManager.AddItem(itemData, item);
                    Destroy(hitInfo.collider.gameObject); //destroy the item in the scene after picking it up
                }
                
            }
        }
        else
        {
            DisableCurrentInteractable();
        }*/
        }
    }

    void SetNewCurrentInteractable(Interactable newInteractable)
    {
        currentInteractable = newInteractable;
        currentInteractable.EnableOutline();
        HUDController.instance.EnableInteractionText(currentInteractable.message);
    }
    void DisableCurrentInteractable()
    {
        HUDController.instance.DisableInteractionText();
        if (currentInteractable)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
    }
}

