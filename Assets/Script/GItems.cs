using System.Collections;
using UnityEngine;

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

    void Start()
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
        if (item !=null)
        {
            //Point fire point at the current center of Camera
            Vector3 firePointPointerPosition = manager.playerCamera.transform.position + manager.playerCamera.transform.forward * 100;
            RaycastHit hit;
            if (Physics.Raycast(manager.playerCamera.transform.position, manager.playerCamera.transform.forward, out hit, 100))
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
    public override int GetCurrentAmmo() {return bulletsPerMagazine;}
    public override int GetReserveAmmo() { return reserveAmmo;}
}
