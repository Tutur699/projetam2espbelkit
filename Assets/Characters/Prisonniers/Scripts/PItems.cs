using System.Collections;
using UnityEngine;
using System.Collections.Generic;
public class PItems : All_Items
{
    public Transform HitPoint;
    
    private float nextUseTime = 0;
    private bool canUse = true;

    void Start()
    {
        if (manager == null)
        {
            manager = FindFirstObjectByType<WPManager>();
        }

    }

    public override void ActivateWeapon(bool activate) //Méthode pour récupérer l'item + faire en sorte que WPManager ne bloque pas
    {
        gameObject.SetActive(activate);
    }
    public override void Use()
    {
        if (canUse && item != null)
        {
            if (Time.time > nextUseTime)
            {
                nextUseTime = Time.time + item.useRate;
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
                    enemy.ApplyDamage(item.weaponDamage);
                }
            }

        }
    }
}
