using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;


public class WPManager : NetworkBehaviour
{
    public const int MAXITEMS = 5;
    public Camera playerCamera;
    public Transform aimPoint;

    public bool isAI = false;
    public bool isSoloPlayer = false;

    [Header("Lien avec le contrôleur")]
    public PlayerManager playerManager;   // référence au joueur pour connaître ses PV
    public PlayerManagerSolo playerManagerSolo; // référence au joueur solo pour connaître ses PV
    public IAEnemy enemyManager;   // référence à l'ennemi pour connaître ses PV
    public List<All_Items> weaponLibrary; //Liste de TOUS les items du jeu
    private bool _deathHandled = false;

    public List<All_Items> allItems = new List<All_Items>(MAXITEMS); //Liste des items dans l'inventaire

    [Header("Inventory UI")]
    public GameObject hudPrefab;
    [HideInInspector] public GameObject myHUDInstance;
    public List<Slot> slots = new List<Slot>(MAXITEMS); //Liste des items (Scriptable Objects)
    public GameObject slotPrefab;
    [HideInInspector] public int selectedItemIndex;
    [HideInInspector] public All_Items selectedItems;

    [HideInInspector] public int selectedSlot = -1;

    [Header("UI References")]
    public TextMeshProUGUI ammoText;

    /**
        * Méthode appelée lors de la création en réseau de l'objet
    */
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if(isSoloPlayer && GetComponent<PlayerManager>() != null) return;

        // --- SÉCURITÉ MULTIJOUEUR ---
        if (!IsOwner) return;
         if(isAI)
        {
            while (allItems.Count < MAXITEMS) allItems.Add(null);
            InitDefaultWeapons();
            return;
        }

        

        if (IsOwner && GetComponent<PlayerManager>() != null) // Si c'est un joueur humain
        {
            if (playerCamera == null) playerCamera = FindFirstObjectByType<Camera>();
            
            if (playerCamera != null)
            {
                aimPoint = playerCamera.transform;
            }
        }

        if (isAI) // Pour l'ia
        {
            if (aimPoint == null)
            {
                Transform[] children = GetComponentsInChildren<Transform>();
                foreach (Transform t in children)
                {
                    if (t.name == "EyesPos")
                    {
                        aimPoint = t;
                        break;
                    }
                }
            }

            // Si par tout hasard on n'a pas EyesPos, on prend l'IA elle-même
            if (aimPoint == null)
            {
                Debug.LogWarning($"[WPManager] {name} : Impossible de trouver 'EyesPos'. Je vise avec mon corps.");
                aimPoint = transform;
            }
        }
        else if (IsOwner)
        {
             if (playerCamera != null) aimPoint = playerCamera.transform;
        }

          


        //--- CRÉATION DE L'INTERFACE ---
        if (hudPrefab != null)
        {
            myHUDInstance = Instantiate(hudPrefab);
            myHUDInstance.name = "HUD_LocalPlayer";
        }
        else
        {
            Debug.LogError("HUD Prefab manquant dans WPManager !");
            return;
        }


        //-+-- AUTO-RECHERCHE CIBLÉE DES SLOTS D'INVENTAIRE ---
        if (myHUDInstance != null)
        {
            Slot[] foundScripts = myHUDInstance.GetComponentsInChildren<Slot>(true); 
            
            slots = new List<Slot>(foundScripts);

            // Tri des slots par ordre d'apparition dans la hiérarchie
            slots.Sort((a, b) => a.transform.GetSiblingIndex().CompareTo(b.transform.GetSiblingIndex()));
            
            Debug.Log($"WPManager a configuré {slots.Count} slots depuis le HUD instancié.");

            var allTexts = myHUDInstance.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach(var t in allTexts)
            {
                if(t.gameObject.name == "AmmoText")
                {
                    ammoText = t;
                    break;
                }
            }
            
            // Si on n'a pas trouvé par nom, on prend le premier venu (secours)
            if (ammoText == null && allTexts.Length > 0) ammoText = allTexts[0];
        }


        // --- INITIALISATION & NETTOYAGE VISUEL ---
        if (slots != null)
        {
             while (allItems.Count < MAXITEMS) allItems.Add(null);

            foreach (var slotScript in slots)
            {
                if (slotScript != null)
                {
                    slotScript.manager = this; 

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
        foreach (var weapon in weaponLibrary)
        {
            if(weapon != null) weapon.gameObject.SetActive(false);
        }

        // --- INITIALISATION DES ARMES ---
        InitDefaultWeapons();
    }

    /**
        * Initialise les armes par défaut au début de la partie
    */

    public void InitDefaultWeapons()

    {  foreach (var weapon in weaponLibrary)
        {
            if (weapon != null)
            {
                weapon.gameObject.SetActive(false);
                weapon.ActivateWeapon(false);
                weapon.isEquipped = false;
            }
        }

        // Équipe les armes par défaut (IsDefaultItem)
        for (int i = 0; i < weaponLibrary.Count; i++)
        {
            if (weaponLibrary[i] != null && weaponLibrary[i].IsOwned) 
            {
                EquipItemFromLibrary(i); 
            }
        }
        
        if (allItems[0] != null) ChangeSelectedSlot(0);
        }

    /*
        * Gère la mort du joueur : déséquipe les armes, désactive le sway, etc.
        */

    public void HandleDeath()
    {
        if (_deathHandled) return;
        _deathHandled = true;

        Debug.Log("Gestion de la mort du joueur dans WPManager...");

    
        if (selectedItems != null)
        {
            selectedItems.ActivateWeapon(false);
            selectedItems.isEquipped = false;
            selectedItems.gameObject.SetActive(false); 
            selectedItems = null;
            selectedItemIndex = -1;
        }
    
    
        if (selectedSlot >= 0 && selectedSlot < slots.Count)
        {
            slots[selectedSlot].Deselect();
            selectedSlot = -1;
        }
}


    private void Update()
    {
        if (PlayerIsDead())
        {
            HandleDeath();
            return;
        }
        else
        {
           if (_deathHandled) _deathHandled = false;
        }
        if (isAI) return;

        if (!IsOwner || ammoText == null) return;

        if (selectedItems != null)
        {
            // On vérifie si l'arme est une arme à feu (GItems) pour afficher les balles
            if (selectedItems is GItems) 
            {
                ammoText.gameObject.SetActive(true);
                
                int current = selectedItems.GetCurrentAmmo();
                int reserve = selectedItems.GetReserveAmmo();
                
                ammoText.text = $"{current} / {reserve}";
            }
            else
            {
                ammoText.gameObject.SetActive(false);
            }
        }
        else
        {
            ammoText.gameObject.SetActive(false);
        }
    }

    /*
    * Fonction utilitaire : savoir si le joueur peut utiliser ses armes 
    */
    private bool PlayerIsDead()
    {
        if (isAI)
        {
            if (enemyManager == null) return false; 
            return !enemyManager.isAlive();
        }
        if( isSoloPlayer)
        {
            if (playerManagerSolo == null) return false;
            return !playerManagerSolo.IsAlive();
        }
        if (playerManager == null) return false;
        return !playerManager.IsAlive();
    }

    /*
    * Change l'arme sélectionnée en fonction de l'index du slot
    *@param newIndex L'index du nouveau slot à sélectionner
    */
    public void ChangeSelectedSlot(int newIndex)
    {
        if (PlayerIsDead())
        {
            Debug.Log("Impossible de changer de slot : le joueur est mort.");
            return;
        }

        if (isAI)
        {
            selectedSlot = newIndex;
            SelectItems(newIndex);
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

    /**
        * Ajoute un item dans le premier slot vide de l'inventaire => utile pour le ramassage
        *@param newItem L'item (ScriptableObject) à ajouter
        *@param newItem3D L'item 3D (All_Items) à ajouter
        */
    public void AddItem(Items newItem, All_Items newItem3D)
    {
        if(PlayerIsDead())
        {
            Debug.Log("Impossible d'ajouter un item : le joueur est mort.");
            return;
        }

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
                // on crée l'UI de l'item
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

                // On ajoute l'item 3D dans l'inventaire
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


    /**
    * Sélectionne un item dans l'inventaire en fonction de l'index du slot
    @param index L'index du slot à sélectionner
    */

    public void SelectItems(int index)
    {
        if (PlayerIsDead()){ 
            Debug.Log("Impossible de sélectionner une arme : le joueur est mort.");
            return;
        }

        if (index < 0 || index >= allItems.Count) return;

        // --- RESET COMPLET (On éteint tout le monde) ---
        foreach (var weapon in allItems)
        {
            if (weapon != null)
            {
                weapon.ActivateWeapon(false);      
                weapon.isEquipped = false;          
                weapon.gameObject.SetActive(false);
            }
        }

        // --- GESTION DE LA CASE VIDE ---
        if (allItems[index] == null)
        {
            selectedItems = null;
            selectedItemIndex = -1; 
            Debug.Log("Case vide sélectionnée : mains nues.");
            return;
        }

        // ---  VÉRIFICATION DE PROPRIÉTÉ ---
        if (allItems[index].IsOwned == false)
        {
            Debug.Log("Tu ne possèdes pas encore cette arme !");
            selectedItems = null; 
            return;
        }

        // --- ACTIVATION DE LA NOUVELLE ARME ---
        All_Items newWeapon = allItems[index];
        
        newWeapon.gameObject.SetActive(true);
        newWeapon.ActivateWeapon(true);
        newWeapon.isEquipped = true;
        
        selectedItems = newWeapon;
        selectedItemIndex = index;

        Debug.Log("Arme équipée : " + newWeapon.item.name);
    }



   /**
   * Déplace un item d'un slot à un autre dans l'inventaire
   *@param oldIndex L'index du slot source
    *@param newIndex L'index du slot destination
    */
    public void MoveItemSlot(int oldIndex, int newIndex)
    {
        if (PlayerIsDead()) return;

        // Vérifications de sécurité
        if (oldIndex < 0 || oldIndex >= allItems.Count || newIndex < 0 || newIndex >= allItems.Count)
            return;

        if (allItems[oldIndex] == null) return;

        //--- DÉPLACEMENT DANS LA LISTE ---
        All_Items movedItem = allItems[oldIndex];
        
        All_Items targetItem = allItems[newIndex];

        allItems[newIndex] = movedItem;
        allItems[oldIndex] = targetItem; 

        Debug.Log($"Item déplacé de {oldIndex} vers {newIndex}");

        //--- MISE À JOUR DE L'UI ---
        
        if (selectedSlot != -1)
        {
            SelectItems(selectedSlot);
        }
    }

    /**
    * Équipe une arme en fonction de son index dans l'inventaire
    *@param index L'index de l'arme à équiper
    */

    public void EquipWeapon(int index)
    {
        if (PlayerIsDead())
        {
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

    /**
    * Équipe un item depuis la bibliothèque d'armes vers un slot spécifique
    *@param libraryIndex L'index de l'arme dans la bibliothèque
    *@param slotIndex L'index du slot où équiper l'arme (optionnel)
    *@return true si l'arme a été équipée avec succès, false sinon
    */

   public bool EquipItemFromLibrary(int libraryIndex, int slotIndex = -1)
    {
        // Sécurité index
        if (libraryIndex < 0 || libraryIndex >= weaponLibrary.Count) 
        {
            Debug.LogError($"[DEBUG] Index {libraryIndex} hors limite dans la Library !");
            return false;
        }
        
        All_Items weaponToEquip = weaponLibrary[libraryIndex];

        // --- TEST DE L'ARME ---
        if (weaponToEquip == null)
        {
            Debug.LogError($"[DEBUG] L'objet arme à l'index {libraryIndex} est NULL dans la WeaponLibrary !");
            return false;
        }

        if (weaponToEquip.item == null)
        {
            Debug.LogError($"[DEBUG] ALERTE ROUGE : L'arme '{weaponToEquip.gameObject.name}' existe, MAIS son champ 'Item' (ScriptableObject) est VIDE ! Vérifie le préfab du Player !");
            return false;
        }


        if (allItems.Contains(weaponToEquip)) return false;

        if (slotIndex == -1)
        {
            for (int i = 0; i < allItems.Count; i++)
            {
                if (allItems[i] == null) { slotIndex = i; break; }
            }
        }

        if (slotIndex == -1 || slotIndex >= MAXITEMS) return false;
        weaponToEquip.manager = this;
        // Logique Backend
        weaponToEquip.UnlockDynamically();
        allItems[slotIndex] = weaponToEquip;

        if (isAI)
        {
            return true;
        }

        NetworkBehaviour netBev = GetComponent<NetworkBehaviour>();
        if(netBev != null && !netBev.IsOwner)
        {
            return true;
        }

        // --- TEST DE L'UI ---
        Slot targetSlot = slots[slotIndex];
        
        // Nettoyage UI
        for (int i = targetSlot.transform.childCount - 1; i >= 0; i--)
        {
             Destroy(targetSlot.transform.GetChild(i).gameObject);
        }

        if (slotPrefab == null)
        {
            Debug.LogError("[DEBUG] SlotPrefab est manquant dans WPManager !");
            return false;
        }

        GameObject newItemUIObj = Instantiate(slotPrefab, targetSlot.transform);
        InventoryItem newItemUI = newItemUIObj.GetComponent<InventoryItem>();

        if (newItemUI != null)
        {
            Debug.Log($"[DEBUG] Succès ! J'envoie l'image '{weaponToEquip.item.name}' vers le slot {slotIndex}");
            newItemUI.InitializeItem(weaponToEquip.item);
        }
        else
        {
            Debug.LogError("[DEBUG] Le SlotPrefab n'a pas de script InventoryItem !");
        }

        return true;
    }
}