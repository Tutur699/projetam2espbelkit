using UnityEngine;

public class WPManager : MonoBehaviour
{
    public Camera playerCamera;
    public PWeapon[] weapons;
    private int selectedWeaponIndex;

    [HideInInspector]
    public PWeapon selectedWeapon;

    // Start is called before the first frame update
    void Start()
    {
        //At the start we enable the primary weapon and disable the rest
        for(selectedWeaponIndex = 0; selectedWeaponIndex < weapons.Length; selectedWeaponIndex++)
        {
            if (selectedWeaponIndex == 0)
                weapons[selectedWeaponIndex].ActivateWeapon(true);
            else
                weapons[selectedWeaponIndex].ActivateWeapon(false);
            weapons[selectedWeaponIndex].manager = this;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Select primary weapon when pressing 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            weapons[0].ActivateWeapon(true);
            selectedWeapon = weapons[0];
            for (selectedWeaponIndex = 1; selectedWeaponIndex < weapons.Length; selectedWeaponIndex++)
            {
                weapons[selectedWeaponIndex].ActivateWeapon(false);
            }
        }

        //Select secondary weapon when pressing 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            weapons[1].ActivateWeapon(true);
            selectedWeapon = weapons[1];
            weapons[0].ActivateWeapon(false);
            for (selectedWeaponIndex = 2; selectedWeaponIndex < weapons.Length; selectedWeaponIndex++)
            {
                weapons[selectedWeaponIndex].ActivateWeapon(false);
            }
        }
        //Select third weapon when pressing 3
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            weapons[2].ActivateWeapon(true);
            selectedWeapon = weapons[2];
            weapons[0].ActivateWeapon(false);
            weapons[1].ActivateWeapon(false);
            for (selectedWeaponIndex = 3; selectedWeaponIndex < weapons.Length; selectedWeaponIndex++)
            {
                weapons[selectedWeaponIndex].ActivateWeapon(false);
            }
        }
        //Select fourth weapon when pressing 4
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            weapons[3].ActivateWeapon(true);
            selectedWeapon = weapons[3];
            weapons[0].ActivateWeapon(false);
            weapons[1].ActivateWeapon(false);
            weapons[2].ActivateWeapon(false);
            for (selectedWeaponIndex = 4; selectedWeaponIndex < weapons.Length; selectedWeaponIndex++)
            {
                weapons[selectedWeaponIndex].ActivateWeapon(false);
            }
        }
        //Select last weapon when pressing 5
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            weapons[4].ActivateWeapon(true);
            selectedWeapon = weapons[4];
            for (selectedWeaponIndex = 0; selectedWeaponIndex < weapons.Length - 1; selectedWeaponIndex++)
            {
                weapons[selectedWeaponIndex].ActivateWeapon(false);
            }
        }
    }
}
