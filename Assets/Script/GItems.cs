using System.Collections;
using UnityEngine;
using Unity.Netcode; 

[RequireComponent(typeof(AudioSource))]

public class GItems : All_Items
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int reserveAmmo = 179;
    public int bulletsPerMagazine = 30;
    public int maxclipSize = 30;
    public float timeToReload = 1.5f;
    public AudioClip fireAudio;
    public AudioClip reloadAudio;
    AudioSource audioSource;
    public AudioClip emptyClickAudio;

    void Awake()
    {
        if (manager == null) {
        manager = FindFirstObjectByType<WPManager>();
    }
        bulletsPerMagazine = maxclipSize;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        //Make sound 3D
        audioSource.spatialBlend = 1f;
    }

    public override void ActivateWeapon(bool activate)
    {
        StopAllCoroutines();
        isReloading = false;
        gameObject.SetActive(activate);
    }

    public override void Use()
    {
        if(bulletsPerMagazine <= 0)
        {
            if(!isReloading && reserveAmmo > 0)
            {
                ReloadWeapon();
            }
            else if(!isReloading && reserveAmmo <= 0)
            {
                //Play empty click sound
                if(emptyClickAudio! != null && !audioSource.isPlaying)
                {
                    audioSource.PlayOneShot(emptyClickAudio); 
                }
            }
            return;
        }
        if(isReloading)
        {
            return;
        }
        Fire();
    }

    public override void ReloadWeapon()
    {
        if (!gameObject.activeInHierarchy) return;
        // On ne recharge pas si : déjà en cours OU chargeur plein OU pas de réserve
        if (isReloading || bulletsPerMagazine >= maxclipSize || reserveAmmo <= 0) return;

        StartCoroutine(ReloadCoroutine());
    }

    IEnumerator ReloadCoroutine()
    {
        isReloading = true;
        if(reloadAudio! != null)
        {
            audioSource.clip = reloadAudio;
            audioSource.Play();  
        }
        

        yield return new WaitForSeconds(timeToReload);
        int ammoNeeded = maxclipSize - bulletsPerMagazine;
        int ammoToTake = Mathf.Min(reserveAmmo, ammoNeeded);

        bulletsPerMagazine += ammoToTake;
        reserveAmmo -= ammoToTake;

        isReloading = false;
    }

    public void Fire()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if(!IsOwner) return; // S'assure que seul le propriétaire tire
        FireServerRpc(manager.aimPoint.position, manager.aimPoint.rotation);
        if (item !=null)
        {
            if (manager.aimPoint == null) // ou aimPoint selon ton nom de variable
            {
                Debug.LogError("Erreur critique : L'IA n'a pas de point de visée (AimSource) !");
                return;
            }
            Transform aimTransform = manager.aimPoint;
            //Point fire point at the current center of Camera
            Vector3 firePointPointerPosition = aimTransform.transform.position + aimTransform.transform.forward  * 100;
            RaycastHit hit;
            if (Physics.Raycast(aimTransform.transform.position, aimTransform.transform.forward, out hit, 100))
            {
                firePointPointerPosition = hit.point;
            }
            firePoint.LookAt(firePointPointerPosition);
            //Fire
            GameObject bulletObject = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            BulletScript bullet = bulletObject.GetComponent<BulletScript>();
            //Set bullet damage according to weapon damage value
            bullet.SetDamage(item.weaponDamage);
            bulletsPerMagazine--;
            audioSource.clip = fireAudio;
            audioSource.Play();
        } 
    }
    [ServerRpc]
    void FireServerRpc(Vector3 position, Quaternion rotation)
    {
        GameObject bulletObject = Instantiate(bulletPrefab, position, rotation);
        BulletScript bullet = bulletObject.GetComponent<BulletScript>();
        //Set bullet damage according to weapon damage value
        bullet.SetDamage(item.weaponDamage);
        // Spawn the bullet on the network
        bulletObject.GetComponent<NetworkObject>().Spawn();
    }
    public override int GetCurrentAmmo() {return bulletsPerMagazine;}
    public override int GetReserveAmmo() { return reserveAmmo;}
}
