using UnityEngine;

public class Interactor : MonoBehaviour
{
    private float interactionRange = 3f;
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

    void checkInteraction()
    {
        RaycastHit hitInfo;
        Ray ray = new Ray(transform.position + _raycastOffset, transform.forward);
        if (Physics.Raycast(ray, out hitInfo, interactionRange))
        {
            if (hitInfo.collider.CompareTag("Items") || hitInfo.collider.CompareTag("Interactable")) //if the object hit by the ray is an interactable object
            {
                Interactable newInteractable = hitInfo.collider.GetComponent<Interactable>();
                if(currentInteractable && newInteractable != currentInteractable) //if we are already looking at an interactable object but it's not the same as the new one
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
    }
    void DisableCurrentInteractable()
    {
        if (currentInteractable)
        {
            currentInteractable.DisableOutline();
            currentInteractable = null;
        }
    }
}
