using UnityEngine;

public abstract class All_Items : MonoBehaviour
{
    public Items item;
    [HideInInspector] public WPManager manager;
    [HideInInspector] public bool isEquipped;

    public abstract void ActivateWeapon(bool activate);
    public abstract void Use();
    
}
