using UnityEngine;
using Unity.Netcode;

public abstract class All_Items : NetworkBehaviour
{
    [Header("PARAMÈTRES DE L'ITEM")]
    public Items item;
    [HideInInspector] public WPManager manager;
    [HideInInspector] public bool isEquipped;
    private bool _isDynamicallyUnlocked = false;
    public bool isReloading = false;

    public bool IsOwned
    {
        // Une arme est "possédée" si elle est marquée par défaut dans le SO
        // OU si le manager l'a débloquée plus tard (par le ramassage)
        get { return item.isDefaultItem || _isDynamicallyUnlocked; }
    }

    public void UnlockDynamically()
    {
        _isDynamicallyUnlocked = true;
    }


    public virtual void ReloadWeapon()
    {
        // Méthode virtuelle à surcharger dans les armes qui se rechargent
    }

    public virtual int GetCurrentAmmo() { return 0; }
    public virtual int GetReserveAmmo() { return 0; }
    public abstract void ActivateWeapon(bool activate);
    public abstract void Use();

    
    
}
