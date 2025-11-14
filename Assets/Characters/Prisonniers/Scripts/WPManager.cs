using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 5;
    public Camera playerCamera;

    public List<PItems> pItems = new List<PItems>(); //List of PItems (the actual item objects in the scene)

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
        SelectItems(newIndex);
    }

    public void AddItem(Items newItem, PItems newItem3D)
    {
        if (newItem == null)
        {
            Debug.LogError("Trying to add null item to inventory");
            return;
        }
        Debug.Log("Trying to add item: " + newItem.name);
        Debug.Log("Number of slots: " + slots.Count);
        for (int i = 0; i < slots.Count; i++)
        {
            Transform slotTransform = slots[i].transform;
            Debug.Log($"Slot {i} child count: {slotTransform.childCount}");
            if (slotTransform.childCount == 0)
            {
                // Create a new InventoryItem UI element
                GameObject newItemUIObj = Instantiate(slotPrefab, slots[i].transform);
                InventoryItem newItemUI = newItemUIObj.GetComponent<InventoryItem>();
                if (newItemUI != null)
                {
                    newItemUI.InitializeItem(newItem);
                    Debug.Log("Successfully added item: " + newItem.name + " to slot " + i);
                }
                else
                {
                    Debug.LogError("InventoryItem component missing on slotPrefab");
                }

                // Add the 3D item to the list
                if (newItem3D != null)
                {
                    while (pItems.Count < slots.Count)
                        pItems.Add(null);

                    if (pItems[i] == null)
                    {
                        pItems[i] = newItem3D; 
                    }
                    else
                    {
                        Debug.LogWarning("Overwriting existing item in pItems at index " + i); 
                    }
                    newItem3D.manager = this;
                    bool isFirstWeapon = ( selectedItems == null);
                    newItem3D.ActivateWeapon(isFirstWeapon);
                    if (isFirstWeapon)
                    {
                        selectedItems = newItem3D;
                        selectedItemIndex = i;
                        selectedSlot = i;
                        // éventuellement sélectionner le slot UI
                        slots[i].Select();
                    }
                    Debug.Log("Added item: " + newItem.name + " to slot " + i);
                }
                return;
            }
        }
        Debug.LogWarning("No empty slots available for item: " + newItem.name);
    }
    
   /* void Awake()
    {
   
        int slotCount = (slots != null && slots.Count > 0) ? slots.Count : MAXITEMS;
        pItems = new List<PItems>(slotCount);
        for (int i = 0; i < slotCount; i++)
            pItems.Add(null);
    }*/

    void Start()
    {
        // Désactive les GameObjects des armes au démarrage
        /*for (int i = 0; i < pItems.Count; i++)
        {
            if (pItems[i] != null)
            {
                pItems[i].gameObject.SetActive(false);
                pItems[i].ActivateWeapon(false);
                pItems[i].isEquipped = false;
            }
                
        }*/
            
        selectedItems = null;
        Debug.Log("Toutes les armes désactivées au démarrage.");

        if (slots == null || slots.Count == 0)
        {
            Debug.LogError("Slots list is empty in WPManager!");
        }
        else
        {
            Debug.Log($"WPManager initialized with {slots.Count} slots");
        } 
        
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
        if (oldIndex < 0 || oldIndex >= pItems.Count || newIndex < 0 || newIndex >= pItems.Count)
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
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= pItems.Count)
        {
            Debug.LogWarning("Index d'arme invalide : " + index);
            return;
        }

        // Désactive toutes les armes avant d’activer la nouvelle
        foreach (var item in pItems)
        {
            if (item != null)
                item.gameObject.SetActive(false);
        }

        // Active l’arme choisie
        selectedItems = pItems[index];
        selectedItems.gameObject.SetActive(true);

        Debug.Log(" Arme équipée : " + selectedItems.name);
    }
}