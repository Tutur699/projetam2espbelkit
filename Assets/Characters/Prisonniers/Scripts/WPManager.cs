using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 3;
    public Camera playerCamera;

    public List<PItems> pItems = new List<PItems>(MAXITEMS); //List of PItems (the actual item objects in the scene)

    [Header("Inventory UI")]
    public List<Slot> slots = new List<Slot>(MAXITEMS); //List of items(Scriptable Objects)
    public GameObject slotPrefab;
    [HideInInspector] public int selectedItemIndex;
    [HideInInspector] public PItems selectedItems;

    [HideInInspector] public int selectedSlot = -1;

    public void ChangeSelectedSlot(int newIndex)
    {
        if (selectedSlot >= 0)
        {
            slots[selectedSlot].Deselect();
        }
        slots[newIndex].Select();
        selectedSlot = newIndex;
    }

    public bool AddItem(Items newItem, PItems newItem3D) //Méthode pour ajouter un item dans l'inventaire
    {   // Find an empty slot
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i].transform.childCount == 0)
            {
                // Create a new InventoryItem UI element
                GameObject newItemUIObj = Instantiate(slotPrefab, slots[i].transform);
                InventoryItem newItemUI = newItemUIObj.GetComponent<InventoryItem>();
                newItemUI.InitializeItem(newItem);

                // Add the 3D item to the list
                pItems.Add(newItem3D);
                newItem3D.manager = this;
                newItem3D.ActivateWeapon(false);

                return true; // Item added successfully
            }
        }
        Debug.Log("Inventory full! Cannot add item: " + newItem.name);
        return false; // Inventory full

    }

    // Start is called before the first frame update
    void Start()
    {
        //At the start we enable the primary weapon and disable the rest
        SelectItems(0);
        ChangeSelectedSlot(0);
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
        if (index >= 0 && index < pItems.Count && pItems[index] == null) //Si l'index est valide mais qu'il n'y a pas d'item
        {
            Debug.LogWarning($"Aucun item trouvé à l'index {index} !");
            selectedItemIndex = -1;
            //Désactivation des autres armes
            for (int i = 0; i < pItems.Count; i++)
            {
                if (pItems[i] != null)
                {
                    pItems[i].ActivateWeapon(false);
                }
            }
        }
    }
}