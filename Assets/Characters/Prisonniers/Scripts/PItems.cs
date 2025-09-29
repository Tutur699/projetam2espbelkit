using System.Collections;
using UnityEngine;

public class PItems : MonoBehaviour
{
    public GameObject item; //The actual item object in the scene
    public bool singleUse = false;
    public float useRate = 0.1f;
    public float weaponDamage = 15; //How much damage should this weapon deal
    public void ActivateWeapon(bool activate)
    {
        item = this.gameObject;
        item.SetActive(activate);
    }

    [HideInInspector]
    public WPManager manager;
    float nextUseTime = 0;
    bool canUse = true;
 

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
        if (canUse)
        {
            if (Time.time > nextUseTime)
            {
                nextUseTime = Time.time + useRate;
                //Implement the use logic here
                Debug.Log("Using item: " + gameObject.name);
                //RaycastHit hit;


            }
        }
    }
}
