using System.Collections;
using UnityEngine;

public class PItems : MonoBehaviour
{
    public GameObject itemObject; //The actual item object in the scene
    public bool singleUse = false;
    public float useRate = 0.1f;
    public float weaponDamage = 15; //How much damage should this weapon deal

    [HideInInspector]
    public WPManager manager;
    public void ActivateWeapon(bool activate)
    {
        itemObject.SetActive(activate);
    }

    void Start()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<WPManager>();
        }

    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) && singleUse)
        {
            Use();
        }
        if (Input.GetMouseButton(0) && !singleUse)
        {
            Use();
        }
    }
    void Use()
    {
        //Implement the use logic here
        Debug.Log("Using item: " + gameObject.name);
    }
}
