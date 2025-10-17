using UnityEngine;

public class Interactor : MonoBehaviour
{
    public WPManager wpManager;
    //private float interactionRange = 3f;
    Interactable currentInteractable;
    private Vector3 _raycastOffset = new Vector3(0, 1f, 0);

    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (DoInteractionTest(out IInteractable interactable))
            {
                if (interactable.CanInteract())
                {
                    interactable.Interact(this);
                }
            }
        }
    }*/

    /*void checkInteraction()
    {
        RaycastHit hitInfo;
        Ray ray = new Ray(transform.position + _raycastOffset, transform.forward);
        if (Physics.Raycast(ray, out hitInfo, interactionRange))
        {
            if (hitInfo.collider.CompareTag("Interactable")) //if the object hit by the ray is an interactable object
            {
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
            }
            else if (hitInfo.collider.CompareTag("Items") && wpManager != null) //if the object hit by the ray is an item
            {
                PItems item = hitInfo.collider.GetComponent<PItems>();
                if (item != null)
                {
                    wpManager.addItem(item);
                    hitInfo.collider.gameObject.SetActive(false); //disable the item in the scene after picking it up
                }
                DisableCurrentInteractable(); //disable any interactable outline if we were looking at one before picking up the item
            }
            else
            { //if the object hit by the ray is not an interactable object
                DisableCurrentInteractable();
            }
        }

        else
        { //if nothing is hit by the ray
            DisableCurrentInteractable();
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
    }*/
}
