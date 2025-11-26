using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 5;
    public Camera playerCamera;

    [Header("Lien avec le joueur")]
    public PlayerManager playerManager;   // référence au joueur pour connaître ses PV
    public List<All_Items> weaponLibrary; //Liste de TOUS les items du jeu

    public List<All_Items> allItems = new List<All_Items>(MAXITEMS); //Liste des items dans l'inventaire
    [Header("Inventory UI")]
    public List<Slot> slots = new List<Slot>(MAXITEMS); //List of items(Scriptable Objects)
    public GameObject slotPrefab;
    [HideInInspector] public int selectedItemIndex;
    [HideInInspector] public All_Items selectedItems;

    [HideInInspector] public int selectedSlot = -1;


    void Start()
{
    // Nettoyage des slots UI au démarrage
    if (slots != null)
        {
            foreach (var slotScript in slots)
            {
                if (slotScript != null)
                {
                    // IMPORTANT : Assigner le manager au slot pour les interactions futures
                    slotScript.manager = this; 
                    
                    // Nettoyage des enfants (anciens items)
                    List<GameObject> childrenToKill = new List<GameObject>();
                    foreach (Transform child in slotScript.transform)
                    {
                        childrenToKill.Add(child.gameObject);
                    }
                    foreach (GameObject child in childrenToKill)
                    {
                        Destroy(child);
                    }
                    slotScript.Deselect();
                }
            }
        }
 
    //Initialisation des slots vides
    while (allItems.Count < MAXITEMS) allItems.Add(null);

    // Initialisation de la bibliothèque
    for (int i = 0; i < weaponLibrary.Count; i++)
    {
        if (weaponLibrary[i] != null)
        {
            weaponLibrary[i].gameObject.SetActive(false);
            weaponLibrary[i].ActivateWeapon(false);      
            weaponLibrary[i].isEquipped = false;
            
        }
    }
    // Équiper les armes par défaut
    for (int i = 0; i < weaponLibrary.Count; i++)
    {
        if (weaponLibrary[i] != null && weaponLibrary[i].item.isDefaultItem)
        {
            EquipItemFromLibrary(i); 
        }
    }
    
    if (allItems[0] != null)
    {
        ChangeSelectedSlot(0);
    }
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

    public void SelectItems(int index)
    {
        if (PlayerIsDead()){ 
            Debug.Log("Impossible de sélectionner une arme : le joueur est mort.");
            return;
        }

        if (index < 0 || index >= allItems.Count) return;

        // --- ETAPE 1 : RESET COMPLET (On éteint tout le monde) ---
        // On parcourt toute la bibliothèque (ou allItems) pour être sûr que RIEN ne traîne
        foreach (var weapon in allItems)
        {
            if (weapon != null)
            {
                weapon.ActivateWeapon(false);      
                weapon.isEquipped = false;          
                weapon.gameObject.SetActive(false);
            }
        }

        // --- ETAPE 2 : GESTION DE LA CASE VIDE ---
        if (allItems[index] == null)
        {
            // Si la case est vide, on ne fait rien de plus
            selectedItems = null; // TRES IMPORTANT : On dit au code qu'on a rien en main
            selectedItemIndex = -1; 
            Debug.Log("Case vide sélectionnée : mains nues.");
            return;
        }

        // --- ETAPE 3 : VÉRIFICATION DE PROPRIÉTÉ ---
        if (allItems[index].IsOwned == false)
        {
            Debug.Log("Tu ne possèdes pas encore cette arme !");
            selectedItems = null; // On s'assure de ne rien avoir en main
            return;
        }

        // --- ETAPE 4 : ACTIVATION DE LA NOUVELLE ARME ---
        All_Items newWeapon = allItems[index];
        
        newWeapon.gameObject.SetActive(true);
        newWeapon.ActivateWeapon(true);
        newWeapon.isEquipped = true;
        
        selectedItems = newWeapon;            // Mise à jour de la référence actuelle
        selectedItemIndex = index;

        Debug.Log("Arme équipée : " + newWeapon.item.name);
    }



   
    public void MoveItemSlot(int oldIndex, int newIndex)
    {
        if (PlayerIsDead()) return;

        // Vérifications de sécurité
        if (oldIndex < 0 || oldIndex >= allItems.Count || newIndex < 0 || newIndex >= allItems.Count)
            return;

        if (allItems[oldIndex] == null) return;

        // --- 1. LE DÉPLACEMENT DANS LA LISTE (BACKEND) ---
        // On échange ou on déplace les références dans la liste
        All_Items movedItem = allItems[oldIndex];
        
        // Si la destination n'est pas vide (Swap), on gère l'échange
        All_Items targetItem = allItems[newIndex];

        allItems[newIndex] = movedItem;
        allItems[oldIndex] = targetItem; // Sera null si la case cible était vide, ou l'autre arme si échange

        Debug.Log($"Item déplacé de {oldIndex} vers {newIndex}");


        // --- 2. LE RAFRAÎCHISSEMENT IMMÉDIAT (LA SOLUTION) ---
        
        // On vérifie simplement : "Qu'est-ce qu'il y a DÉSORMAIS dans le slot que je regarde ?"
        // Si selectedSlot vaut 0, et que j'ai bougé mon arme du slot 0 au 4 :
        // Le code va relancer SelectItems(0).
        // SelectItems(0) va voir que c'est vide -> Il va désactiver l'arme (mains vides).
        
        if (selectedSlot != -1)
        {
            SelectItems(selectedSlot);
        }
    }


    public void EquipWeapon(int index)
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
    public bool EquipItemFromLibrary(int libraryIndex, int slotIndex = -1)
    {
        // 1. Sécurités de base
        if (libraryIndex < 0 || libraryIndex >= weaponLibrary.Count) return false;
        
        All_Items weaponToEquip = weaponLibrary[libraryIndex];

        // Vérifie si déjà possédé
        if (allItems.Contains(weaponToEquip))
        {
            Debug.Log("Arme déjà dans l'inventaire !");
            return false;
        }

        // Trouve un slot vide si nécessaire
        if (slotIndex == -1)
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] == null)
                {
                    slotIndex = i;
                    break;
                }
            }
        }

        if (slotIndex == -1 || slotIndex >= MAXITEMS) return false;

        // --- 2. LOGIQUE (Backend) ---
        allItems[slotIndex] = weaponToEquip;
        weaponToEquip.UnlockDynamically();


        // --- 3. VISUEL UI (Frontend) --- 
        
        // On nettoie le slot au cas où il y aurait un vieux truc
        Slot targetSlot = slots[slotIndex];
        foreach(Transform child in targetSlot.transform)
        {
            Destroy(child.gameObject);
        }

        // On instancie le Prefab "InventoryItem" (celui qui a l'image)
        GameObject newItemUIObj = Instantiate(slotPrefab, targetSlot.transform);
        
        // On configure l'image
        InventoryItem newItemUI = newItemUIObj.GetComponent<InventoryItem>();
        if (newItemUI != null)
        {
            // On envoie les données du ScriptableObject (Items) à l'UI
            newItemUI.InitializeItem(weaponToEquip.item);
        }
        else
        {
            Debug.LogError("Le slotPrefab n'a pas de script InventoryItem !");
        }

        Debug.Log($"Arme {weaponToEquip.name} ajoutée (Logic + UI) au slot {slotIndex}");
        return true;
    }
}