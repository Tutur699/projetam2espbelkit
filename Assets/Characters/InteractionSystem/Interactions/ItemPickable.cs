using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Events;

public class ItemPickable : MonoBehaviour, IPickable
{
    Outline outline;
    public string message;
    public UnityEvent interactAction;
    public Items itemScriptable;
    public PItems itemP;

    void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutline();
    }

    public void Interact()
    {
        interactAction.Invoke();
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }
    public void EnableOutline()
    {
        outline.enabled = true;
    }
    public void PickItem()
    {
        Debug.Log("Picking item: " + gameObject.name);
        if (outline != null)
        {
            outline.enabled = false;
        }
        Destroy(this.gameObject);
    }
}
