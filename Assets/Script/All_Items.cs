using UnityEngine;

public abstract class All_Items : MonoBehaviour
{
    [Header("PARAMÈTRES DE L'ITEM")]
    public Items item;
    [HideInInspector] public WPManager manager;
    [HideInInspector] public bool isEquipped;
    private bool _isDynamicallyUnlocked = false;

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

    public abstract void ActivateWeapon(bool activate);
    public abstract void Use();
    
}
