using UnityEngine;

public class PItems
{
    public GameObject itemObject;
    public WPManager manager;
    public void ActivateWeapon(bool activate)
    {
        itemObject.SetActive(activate);
    }
}
