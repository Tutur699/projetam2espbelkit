using System.Collections;
using UnityEngine;

public class PItems : MonoBehaviour
{
    public GameObject item; //The actual item object in the scene
    public bool singleUse = false;
    public float useRate = 0.1f;
    public float weaponDamage = 5; //How much damage should this weapon deal
    public Transform HitPoint;
    public void ActivateWeapon(bool activate) //Méthode pour récupérer l'item + faire en sorte que WPManager ne bloque pas
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
                Vector3 hitPointPointerPosition = manager.playerCamera.transform.position + manager.playerCamera.transform.forward * 100;
                RaycastHit hit;
                if (Physics.Raycast(manager.playerCamera.transform.position, manager.playerCamera.transform.forward, out hit, 100))
                {
                    hitPointPointerPosition = hit.point;
                }
                if (hit.transform.CompareTag("Enemy")) //Enemy tag here is important because an item can be used on anything
                {
                    Debug.DrawLine(HitPoint.position, HitPoint.position + HitPoint.forward * 3f, Color.cyan);

                    IEntity enemy = hit.transform.GetComponent<IEntity>();
                    enemy.ApplyDamage(weaponDamage);
                }
            }

        }
    }
}
