using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 5;
    public Camera playerCamera;

    [Header("Lien avec le joueur")]
    public PlayerManager playerManager;   // référence au joueur pour connaître ses PV

    public List<All_Items> allItems = new List<All_Items>(MAXITEMS); //Liste de toutes les armes (allItems et GItems)

    [Header("Inventory UI")]
    public List<Slot> slots = new List<Slot>(MAXITEMS); //List of items(Scriptable Objects)
    public GameObject slotPrefab;
    [HideInInspector] public int selectedItemIndex;
    [HideInInspector] public All_Items selectedItems;

    [HideInInspector] public int selectedSlot = -1;


    void Start()
    {
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
        SelectItems(0);
    }

    private void Update()
    {
        // Si tu veux, tu peux ici rajouter un comportement spécial
        // mais comme PlayerManager gère déjà la mort, on n'est pas obligé
        if (PlayerIsDead())
        {
            // On peut s'assurer que l'arme équipée est désactivée
            if (selectedItems != null)
            {
                selectedItems.ActivateWeapon(false);
                selectedItems.isEquipped = false;
            }
        }
    }

    // --- Fonction utilitaire : savoir si le joueur peut utiliser ses armes ---
    private bool PlayerIsDead()
    {
        if (playerManager == null) return false;   // sécurité si non assigné
        return !playerManager.IsAlive();
    }


    public void ChangeSelectedSlot(int newIndex)
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de changer de slot : le joueur est mort.");
            return;
        }

        if (selectedSlot >= 0)
        {
            slots[selectedSlot].Deselect();
        }
        slots[newIndex].Select();
        selectedSlot = newIndex;
        SelectItems(newIndex);
    }

    public void AddItem(Items newItem, All_Items newItem3D) //Version pour allItems
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
                    while (allItems.Count < slots.Count)
                        allItems.Add(null);

                    if (allItems[i] == null)
                    {
                        allItems[i] = newItem3D;
                    }
                    else
                    {
                        Debug.LogWarning("Overwriting existing item in allItems at index " + i);
                    }
                    newItem3D.manager = this;

                    // On active seulement si c’est la première arme ET joueur vivant
                    bool isFirstWeapon = (selectedItems == null);
                    newItem3D.ActivateWeapon(isFirstWeapon && !PlayerIsDead());
                    if (isFirstWeapon && !PlayerIsDead())
                    {
                        selectedItems = newItem3D;
                        selectedItemIndex = i;
                        selectedSlot = i;
                        slots[i].Select();
                    }
                    Debug.Log("Added item: " + newItem.name + " to slot " + i);
                }
                return;
            }
        }
        Debug.LogWarning("No empty slots available for item: " + newItem.name);
    }

    public void SelectItems(int index) //Méthode pour sélectionner une arme dans l'inventaire version allItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de sélectionner une arme : le joueur est mort.");
            return;
        }
        if (index < 0 || index >= allItems.Count)
        {
            Debug.LogWarning($"Index {index} invalide pour la liste d'items !");
            selectedItems = null;
            return;
        }
        if (allItems[index] == null)
        {
            Debug.LogWarning($"Aucun item trouvé à l'index {index} !");
            if (selectedItems != null)
            {
                selectedItems.isEquipped = false;
                selectedItems.ActivateWeapon(false);
            }
            //Désactivation des autres armes
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] != null)
                {
                    allItems[i].ActivateWeapon(false);
                    allItems[i].isEquipped = false;
                }
            }
            return;
        }
        //Désactivation des autres armes
        for (int i = 0; i < allItems.Count; i++)
        {
            if (i != index && allItems[i] != null)
            {
                allItems[i].ActivateWeapon(false);
            }
        }

        allItems[index].ActivateWeapon(true);
        allItems[index].isEquipped = true;
        selectedItems = allItems[index];
        selectedItemIndex = index;

        Debug.Log("Selected item: " + selectedItems.item.name + " at index " + index);
    }



   
    public void MoveItemSlot(int oldIndex, int newIndex) //Version pour allItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de déplacer une arme : le joueur est mort.");
            return;
        }

        if (oldIndex < 0 || oldIndex >= allItems.Count || newIndex < 0 || newIndex >= allItems.Count)
            return;

        if (allItems[oldIndex] == null)
            return;

        // On déplace la référence logique
        All_Items movedItem = allItems[oldIndex];

        while (allItems.Count <= newIndex)
            allItems.Add(null);

        allItems[newIndex] = movedItem;
        allItems[oldIndex] = null;

        Debug.Log($"Item {movedItem.name} déplacé de {oldIndex} vers {newIndex}");

        if (selectedSlot == oldIndex)
        {
            selectedItems = allItems[newIndex];
            selectedItemIndex = newIndex;
        }
        else if (selectedSlot == newIndex)
        {
            selectedItems = allItems[newIndex];
        }
    }


    public void EquipWeapon(int index) //Version pour allItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible d'équiper une arme : le joueur est mort.");
            return;
        }

        if (index < 0 || index >= allItems.Count)
        {
            Debug.LogWarning("Index d'arme invalide : " + index);
            return;
        }

        // Désactive toutes les armes avant d’activer la nouvelle
        foreach (var item in allItems)
        {
            if (item != null)
                item.gameObject.SetActive(false);
        }

        // Active l’arme choisie
        selectedItems = allItems[index];
        if (selectedItems != null)
        {
            selectedItems.gameObject.SetActive(true);
            selectedItems.ActivateWeapon(true);
            selectedItems.isEquipped = true;
            Debug.Log("Arme équipée : " + selectedItems.name);
        }
    }
}
    