using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class HUDController : MonoBehaviour
{
    public static HUDController instance;

    [SerializeField] TMP_Text interactionText;

    private void Awake()
    {
        instance = this;
    }

    public void EnableInteractionText(string message)
    {
        interactionText.text = message + " (F) ";
        interactionText.gameObject.SetActive(true);
    }
    public void DisableInteractionText()
    {
        interactionText.gameObject.SetActive(false);
    }


}
