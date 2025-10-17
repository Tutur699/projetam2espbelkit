using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 3;
    public Camera playerCamera;

    private List<PItems> pItems = new List<PItems>(MAXITEMS); //List of PItems (the actual item objects in the scene)
    
    [Header("Inventory UI")]
    public List<Slot> slots = new List<Slot>(MAXITEMS); //List of items(Scriptable Objects)
    public GameObject slotPrefab;
    [HideInInspector] public int selectedItemIndex;
    [HideInInspector] public PItems selectedItems;

    public bool addItem(Items newItem,PItems newItem3D) //Méthode pour ajouter un item dans l'inventaire
    {
        if (pItems.Count >= MAXITEMS)
        {
            Debug.LogWarning("Inventaire plein !");
            return false;
        }
        pItems.Add(newItem3D);
        newItem3D.manager = this;
        newItem3D.ActivateWeapon(false);
        foreach (Slot slot in slots)
        {
            if (slot.transform.childCount == 0)
            {
                GameObject newPItems = Instantiate(slotPrefab, slot.transform);
                InventoryItem invItem = newPItems.GetComponent<InventoryItem>();
                invItem.InitializeItem(newItem);
                invItem.itemImage.raycastTarget = true;
                return true;
            }
        }
        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        //At the start we enable the primary weapon and disable the rest
        SelectItems(0);
    }

    public void SelectItems(int index) //Méthode pour sélectionner une arme dans l'inventaire
    {
        if (index < 0 || index >= pItems.Count)
        {
            Debug.LogWarning($"Index {index} invalide pour la liste d'items !");
            return;
        }
        if (index >= 0 && index < pItems.Count && pItems[index] != null)
        {
            pItems[index].ActivateWeapon(true);
            selectedItems = pItems[index];
            selectedItemIndex = index;
            //Désactivation des autres armes
            for (int i = 0; i < pItems.Count; i++)
            {
                if (i != index && pItems[i] != null)
                {
                    pItems[i].ActivateWeapon(false);
                }
            }
        }
    }
}