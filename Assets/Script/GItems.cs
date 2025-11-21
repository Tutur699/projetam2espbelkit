using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]

public class GItems : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public int bulletsPerMagazine = 30;
    public float timeToReload = 1.5f;
    public AudioClip fireAudio;
    public AudioClip reloadAudio;
    public Items Witem;
    bool canFire = true;
    float nextFireTime = 0;
    int bulletsPerMagazineDefault = 0;
    AudioSource audioSource;
    [HideInInspector] public WPManager manager;
    [HideInInspector] public bool isEquipped = false;

    void Start()
    {
        if (manager == null) {
        manager = FindFirstObjectByType<WPManager>();
    }
        bulletsPerMagazineDefault = bulletsPerMagazine;
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        //Make sound 3D
        audioSource.spatialBlend = 1f;
    }

    public void ActivateWeapon(bool activate)
    {
        StopAllCoroutines();
        canFire = true;
        gameObject.SetActive(activate);
    }

    IEnumerator Reload()
    {
        canFire = false;

        audioSource.clip = reloadAudio;
        audioSource.Play();

        yield return new WaitForSeconds(timeToReload);

        bulletsPerMagazine = bulletsPerMagazineDefault;

        canFire = true;
    }

    public void Fire()
    {
        if (canFire && Witem !=null)
        {
            if (Time.time > nextFireTime)
            {
                nextFireTime = Time.time + Witem.useRate;

                if (bulletsPerMagazine > 0)
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
                    bullet.SetDamage(Witem.weaponDamage); 
                    bulletsPerMagazine--;
                    audioSource.clip = fireAudio;
                    audioSource.Play();
                }
                else
                {
                    StartCoroutine(Reload());
                }
            }
        }
    }



}
