using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 5;
    public Camera playerCamera;

    [Header("Lien avec le joueur")]
    public PlayerManager playerManager;   // référence au joueur pour connaître ses PV

    public List<PItems> pItems = new List<PItems>(); //List of PItems (the actual item objects in the scene)
    public List<GItems> gItems = new List<GItems>(); //List of GItems (the gun scripts)

    [Header("Inventory UI")]
    public List<Slot> slots = new List<Slot>(MAXITEMS); //List of items(Scriptable Objects)
    public GameObject slotPrefab;
    [HideInInspector] public int selectedItemIndex;
    [HideInInspector] public int selectedWeaponIndex;
    [HideInInspector] public PItems selectedItems;
    [HideInInspector] public GItems selectedWeapon;

    [HideInInspector] public int selectedSlot = -1;

    // --- Fonction utilitaire : savoir si le joueur peut utiliser ses armes ---
    private bool PlayerIsDead()
    {
        if (playerManager == null) return false;   // sécurité si non assigné
        return !playerManager.IsAlive();
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
            if(selectedWeapon != null)
            {
                selectedWeapon.ActivateWeapon(false);
                selectedWeapon.isEquipped = false;
            }
        }
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

    public void AddItem(Items newItem, PItems newItem3D) //Version pour PItems
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

    public void AddItem(Items newItem, GItems newWeapon3D) //Version pour GItems
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
                if (newWeapon3D != null)
                {
                    while (gItems.Count < slots.Count)
                        gItems.Add(null);

                    if (gItems[i] == null)
                    {
                        gItems[i] = newWeapon3D;
                    }
                    else
                    {
                        Debug.LogWarning("Overwriting existing item in pItems at index " + i);
                    }
                    newWeapon3D.manager = this;

                    // On active seulement si c’est la première arme ET joueur vivant
                    bool isFirstWeapon = (selectedWeapon == null);
                    newWeapon3D.ActivateWeapon(isFirstWeapon && !PlayerIsDead());
                    if (isFirstWeapon && !PlayerIsDead())
                    {
                        selectedWeapon = newWeapon3D;
                        selectedWeaponIndex = i;
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

    void Start()
    {
        selectedItems = null;
        selectedWeapon = null;
        Debug.Log("Toutes les armes désactivées au démarrage.");

        if (slots == null || slots.Count == 0)
        {
            Debug.LogError("Slots list is empty in WPManager!");
        }
        else
        {
            Debug.Log($"WPManager initialized with {slots.Count} slots");
        }
        SelectWeapon(0);
    }

    public void SelectItems(int index) //Méthode pour sélectionner une arme dans l'inventaire version PItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de sélectionner une arme : le joueur est mort.");
            return;
        }
        if (index < 0 || index >= pItems.Count)
        {
            Debug.LogWarning($"Index {index} invalide pour la liste d'items !");
            selectedItems = null;
            return;
        }
        if (pItems[index] == null)
        {
            Debug.LogWarning($"Aucun item trouvé à l'index {index} !");
            if (selectedItems != null)
            {
                selectedItems.isEquipped = false;
                selectedItems.ActivateWeapon(false);
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

    public void SelectWeapon(int index) //Méthode pour sélectionner une arme dans l'inventaire version GItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de sélectionner une arme : le joueur est mort.");
            return;
        }
        if (index < 0 || index >= gItems.Count)
        {
            Debug.LogWarning($"Index {index} invalide pour la liste d'armes !");
            selectedWeapon = null;
            return;
        }
        if (gItems[index] == null)
        {
            Debug.LogWarning($"Aucune arme trouvée à l'index {index} !");
            if (selectedWeapon != null)
            {
                selectedWeapon.isEquipped = false;
                selectedWeapon.ActivateWeapon(false);
            }
            //Désactivation des autres armes
            for (int i = 0; i < gItems.Count; i++)
            {
                if (gItems[i] != null)
                {
                    gItems[i].ActivateWeapon(false);
                    gItems[i].isEquipped = false;
                }
            }
            return;
        }
        //Désactivation des autres armes
        for (int i = 0; i < gItems.Count; i++)
        {
            if (i != index && gItems[i] != null)
            {
                gItems[i].ActivateWeapon(false);
            }
        }

        gItems[index].ActivateWeapon(true);
        gItems[index].isEquipped = true;
        selectedWeapon = gItems[index];
        selectedWeaponIndex = index;

        Debug.Log("Selected weapon: " + selectedWeapon.Witem.name + " at index " + index);
    }

   
    public void MoveItemSlot(int oldIndex, int newIndex) //Version pour PItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de déplacer une arme : le joueur est mort.");
            return;
        }

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

    public void MoveWeaponSlot(int oldIndex, int newIndex) //Version pour GItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de déplacer une arme : le joueur est mort.");
            return;
        }

        if (oldIndex < 0 || oldIndex >= gItems.Count || newIndex < 0 || newIndex >= gItems.Count)
            return;

        if (gItems[oldIndex] == null)
            return;

        // On déplace la référence logique
        GItems movedWeapon = gItems[oldIndex];

        while (gItems.Count <= newIndex)
            gItems.Add(null);

        gItems[newIndex] = movedWeapon;
        gItems[oldIndex] = null;

        Debug.Log($"Weapon {movedWeapon.name} déplacée de {oldIndex} vers {newIndex}");

        if (selectedSlot == oldIndex)
        {
            selectedWeapon = gItems[newIndex];
            selectedWeaponIndex = newIndex;
        }
        else if (selectedSlot == newIndex)
        {
            selectedWeapon = gItems[newIndex];
        }
    }

    public void EquipWeapon(int index) //Version pour PItems
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible d'équiper une arme : le joueur est mort.");
            return;
        }

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
        if (selectedItems != null)
        {
            selectedItems.gameObject.SetActive(true);
            selectedItems.ActivateWeapon(true);
            selectedItems.isEquipped = true;
            Debug.Log("Arme équipée : " + selectedItems.name);
        }
    }
}
    