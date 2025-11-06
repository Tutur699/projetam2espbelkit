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
                //UpdateUI();
                newItem3D.manager = this;
                newItem3D.ActivateWeapon(false);
                Debug.Log("Added item: " + newItem.name + " to slot " + i);

                return true; // Item added successfully
            }
        }
        Debug.Log("Inventory full! Cannot add item: " + newItem.name);
        return false; 

    }

    void Start()
    {
        if (pItems.Count > 0 && pItems[0] != null)
            SelectItems(0);
        else
            selectedItems = null;
        ChangeSelectedSlot(0);
        UpdateUI();
    }

    public void SelectItems(int index) //Méthode pour sélectionner une arme dans l'inventaire
    {
        if (index < 0 || index >= pItems.Count) //Si l'index n'est pas valide
        {
            Debug.LogWarning($"Index {index} invalide pour la liste d'items !");
            selectedItems = null;
            return;
        }
        if (pItems[index] == null) //Si l'index est valide mais qu'il n'y a pas d'item
        {
            Debug.LogWarning($"Aucun item trouvé à l'index {index} !");
            if (selectedItems != null)
            {
                selectedItems.isEquipped = false;
                selectedItems.ActivateWeapon(false);
                //selectedItems = null;
                //selectedItemIndex = -1;
            }
            //Désactivation des autres armes
            for (int i = 0; i < pItems.Count; i++)
            {
                if (pItems[i] != null)
                {  
                    pItems[i].ActivateWeapon(false);
                    pItems[i].isEquipped = false;
                }
            }
            return;
        }
        //Désactivation des autres armes
        for (int i = 0; i < pItems.Count; i++)
        {
            if (i != index && pItems[i] != null)
            {
                pItems[i].ActivateWeapon(false);
            }

        }
        pItems[index].ActivateWeapon(true);
        pItems[index].isEquipped = true;
        selectedItems = pItems[index];
        selectedItemIndex = index;
        
        Debug.Log("Selected item: " + selectedItems.item.name + " at index " + index);
    
    }

    public void MoveItemSlot(int oldIndex, int newIndex)
    {
    if (oldIndex < 0 || oldIndex >= pItems.Count || newIndex < 0 || newIndex >= pItems.Count )
        return;

    
    if (pItems[oldIndex] == null)
        return;

    // On déplace la référence logique
    PItems movedItem = pItems[oldIndex];    

    
         while (pItems.Count <= newIndex)
            pItems.Add(null);

        pItems[newIndex] = movedItem;
        pItems[oldIndex] = null; 

        Debug.Log($"Item {movedItem.name} déplacé de {oldIndex} vers {newIndex}");

        UpdateUI();
        if (selectedSlot == oldIndex)
        {
            selectedItems = pItems[newIndex];
            selectedItemIndex = newIndex;
        }
        else if (selectedSlot == newIndex)
        {
            selectedItems = pItems[newIndex];
        }
    }
    public void UpdateUI()
    {
        /*for (int i = 0; i < slots.Count; i++)
        {
            if (i < pItems.Count && pItems[i] != null)
            {
                slots[i].SetItem(pItems[i].item);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }*/
    }
}