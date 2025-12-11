using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;
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
        if(!IsOwner) return; // S'assure que seul le propriétaire utilise l'item
        if (canUse && item != null)
        {
            if (Time.time > nextUseTime)
            {
                nextUseTime = Time.time + item.useRate;
                Vector3 hitPointPointerPosition = manager.aimPoint.transform.position + manager.aimPoint.transform.forward * 100;
                RaycastHit hit;
                if (Physics.Raycast(manager.aimPoint.transform.position, manager.aimPoint.transform.forward, out hit, 100))
                {
                    hitPointPointerPosition = hit.point;
                }
                if (hit.transform.CompareTag("Enemy")|| hit.transform.CompareTag("Player")) //Enemy tag here is important because an item can be used on anything
                {
                    Debug.DrawLine(HitPoint.position, HitPoint.position + HitPoint.forward * 3f, Color.cyan);

                    IEntity enemy = hit.transform.GetComponent<IEntity>();
                    enemy.ApplyDamage(item.weaponDamage);
                }
            }

        }
    }
}
