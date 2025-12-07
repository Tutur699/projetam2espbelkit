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

    [Header("Lien avec le contrôleur")]
    public PlayerManager playerManager;   // référence au joueur pour connaître ses PV
    public IAEnemy enemyManager;   // référence à l'ennemi pour connaître ses PV
    public List<All_Items> weaponLibrary; //Liste de TOUS les items du jeu
    private bool _deathHandled = false;

    public List<All_Items> allItems = new List<All_Items>(MAXITEMS); //Liste des items dans l'inventaire

    /*[Header("Sway & Camera")]
    public GameObject weaponSwayHolder;*/

    [Header("Inventory UI")]
    public GameObject hudPrefab;
    [HideInInspector] public GameObject myHUDInstance;
    public List<Slot> slots = new List<Slot>(MAXITEMS); //List of items(Scriptable Objects)
    public GameObject slotPrefab;
    [HideInInspector] public int selectedItemIndex;
    [HideInInspector] public All_Items selectedItems;

    [HideInInspector] public int selectedSlot = -1;

    [Header("UI References")]
    public TextMeshProUGUI ammoText;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();


        // --- SÉCURITÉ MULTIJOUEUR ---
        // Si je ne suis pas le propriétaire de ce perso, je ne crée pas d'interface
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
            
            // Pour le joueur, la source de visée C'EST la caméra
            if (playerCamera != null)
            {
                aimPoint = playerCamera.transform;
            }
        }

        if (isAI)
        {
            // 1. Si la case est vide, on cherche "EyesPos" partout dans les enfants
            if (aimPoint == null)
            {
                Transform[] children = GetComponentsInChildren<Transform>();
                foreach (Transform t in children)
                {
                    if (t.name == "EyesPos") // Vérifie bien l'orthographe !
                    {
                        aimPoint = t;
                        break;
                    }
                }
            }

            // 2. ULTIME SECOURS : Si toujours vide, on prend l'IA elle-même
            if (aimPoint == null)
            {
                Debug.LogWarning($"[WPManager] {name} : Impossible de trouver 'EyesPos'. Je vise avec mon corps.");
                aimPoint = transform; // Au moins ça ne crashera pas
            }
        }
        else if (IsOwner) // Pour le joueur
        {
             // ... (Recherche Caméra) ...
             if (playerCamera != null) aimPoint = playerCamera.transform;
        }

       

        /*if (playerCamera != null && weaponSwayHolder != null)
        {
            // A. On active le script de Sway
            WeaponSway swayScript = weaponSwayHolder.GetComponent<WeaponSway>();
            if (swayScript != null) swayScript.enabled = true;

            // B. On détache le Holder du Joueur et on le colle sous la Caméra
            weaponSwayHolder.transform.SetParent(playerCamera.transform);

            // C. On réinitialise sa position pour qu'il soit bien au centre de la vue
            weaponSwayHolder.transform.localPosition = Vector3.zero;
            weaponSwayHolder.transform.localRotation = Quaternion.identity;

            Debug.Log("Armes attachées à la caméra avec succès !");
        }
        else
        {
            Debug.LogError("Impossible d'attacher les armes : Caméra ou SwayHolder manquant !");
        }*/
        
        


        // --- ÉTAPE 1 : CRÉATION DE L'INTERFACE ---
        if (hudPrefab != null)
        {
            myHUDInstance = Instantiate(hudPrefab);
            myHUDInstance.name = "HUD_LocalPlayer"; // Petit nom pour le retrouver
        }
        else
        {
            Debug.LogError("HUD Prefab manquant dans WPManager !");
            return;
        }


        // --- ÉTAPE 2 : AUTO-RECHERCHE CIBLÉE ---
        // On ne cherche pas dans toute la scène, mais JUSTE dans le HUD qu'on vient de créer
        if (myHUDInstance != null)
        {
            // true = on inclut les enfants inactifs au cas où
            Slot[] foundScripts = myHUDInstance.GetComponentsInChildren<Slot>(true); 
            
            slots = new List<Slot>(foundScripts);

            // TRI CRUCIAL : On trie selon l'ordre dans la hiérarchie (Slot 1, Slot 2...)
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


        // --- ÉTAPE 3 : INITIALISATION & NETTOYAGE VISUEL ---
        // (Le code qu'on avait fait pour nettoyer les carrés blancs du prefab)
        if (slots != null)
        {
            // On initialise la liste logique interne
             while (allItems.Count < MAXITEMS) allItems.Add(null);

            foreach (var slotScript in slots)
            {
                if (slotScript != null)
                {
                    // A. On se connecte
                    slotScript.manager = this; 

                    // B. On détruit les "carrés blancs" par défaut du prefab
                    List<GameObject> childrenToKill = new List<GameObject>();
                    foreach (Transform child in slotScript.transform)
                    {
                        childrenToKill.Add(child.gameObject);
                    }
                    foreach (GameObject child in childrenToKill)
                    {
                        Destroy(child);
                    }
                    
                    // C. On reset la couleur
                    slotScript.Deselect();
                }
            }
        }

        // --- ÉTAPE 4 : INITIALISATION DES ARMES ---
        // Cache la bibliothèque 3D
        InitDefaultWeapons();
    }

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
        
        // Sélectionne le premier slot
        if (allItems[0] != null) ChangeSelectedSlot(0);
        }

    public void HandleDeath()
    {
        if (_deathHandled) return; // On l'a déjà fait, on arrête
        _deathHandled = true;

        Debug.Log("Gestion de la mort du joueur dans WPManager...");

        // 1. Désactiver l'arme en main
        if (selectedItems != null)
        {
            selectedItems.ActivateWeapon(false);
            selectedItems.isEquipped = false;
            selectedItems.gameObject.SetActive(false); // On cache le modèle 3D
            selectedItems = null; // On vide la référence
            selectedItemIndex = -1;
        }

        // 2. Désactiver le HUD (Optionnel, si tu veux cacher l'inventaire quand on meurt)
        if (myHUDInstance != null)
        {
            myHUDInstance.SetActive(false);
        }

        // 3. Désactiver le Weapon Sway (pour éviter que la caméra bouge bizarrement)
        WeaponSway sway = GetComponentInChildren<WeaponSway>();
        if (sway != null) sway.enabled = false;
        
        // 4. Désélectionner visuellement les slots
        if (selectedSlot >= 0 && selectedSlot < slots.Count)
        {
            slots[selectedSlot].Deselect();
            selectedSlot = -1;
        }
    }


    private void Update()
    {
        // Si tu veux, tu peux ici rajouter un comportement spécial
        // mais comme PlayerManager gère déjà la mort, on n'est pas obligé
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
            // (Si tu as un couteau, pas besoin d'afficher "0/0")
            if (selectedItems is GItems) 
            {
                ammoText.gameObject.SetActive(true); // On affiche le texte
                
                // On récupère les valeurs via les méthodes qu'on a créées dans All_Items
                int current = selectedItems.GetCurrentAmmo();
                int reserve = selectedItems.GetReserveAmmo();
                
                ammoText.text = $"{current} / {reserve}";
            }
            else
            {
                // C'est un couteau ou autre chose -> on cache le texte
                ammoText.gameObject.SetActive(false);
            }
        }
        else
        {
            // Rien dans les mains -> on cache le texte
            ammoText.gameObject.SetActive(false);
        }
    }

    // --- Fonction utilitaire : savoir si le joueur peut utiliser ses armes ---
    private bool PlayerIsDead()
    {
        if (isAI)
        {
            if (enemyManager == null) return false; // sécurité si non assigné
            return !enemyManager.isAlive();
        }
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

        if (isAI)
        {
            selectedSlot = newIndex;
            SelectItems(newIndex); // On active l'arme 3D
            return; // STOP ! Ne pas toucher à la liste 'slots'
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
        // Sécurité index
        if (libraryIndex < 0 || libraryIndex >= weaponLibrary.Count) 
        {
            Debug.LogError($"[DEBUG] Index {libraryIndex} hors limite dans la Library !");
            return false;
        }
        
        All_Items weaponToEquip = weaponLibrary[libraryIndex];

        // --- TEST DU COUPABLE N°1 : L'ARME ---
        if (weaponToEquip == null)
        {
            Debug.LogError($"[DEBUG] L'objet arme à l'index {libraryIndex} est NULL dans la WeaponLibrary !");
            return false;
        }

        if (weaponToEquip.item == null)
        {
            // C'EST SOUVENT LUI LE COUPABLE !
            Debug.LogError($"[DEBUG] ALERTE ROUGE : L'arme '{weaponToEquip.gameObject.name}' existe, MAIS son champ 'Item' (ScriptableObject) est VIDE ! Vérifie le préfab du Player !");
            return false;
        }
        // -------------------------------------

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

        // --- TEST DU COUPABLE N°2 : L'UI ---
        Slot targetSlot = slots[slotIndex];
        
        // Nettoyage UI
        for (int i = targetSlot.transform.childCount - 1; i >= 0; i--)
        {
             DestroyImmediate(targetSlot.transform.GetChild(i).gameObject);
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