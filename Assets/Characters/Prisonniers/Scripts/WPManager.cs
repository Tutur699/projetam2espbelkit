using UnityEngine;

public class WPManager : MonoBehaviour
{
    public Camera playerCamera;
    public PItems[] items;
    private int selectedItemIndex;

    [HideInInspector]
    public PItems selectedItems;

    // Start is called before the first frame update
    void Start()
    {
        //At the start we enable the primary weapon and disable the rest
        for (selectedItemIndex = 0; selectedItemIndex < items.Length; selectedItemIndex++)
        {
            if (selectedItemIndex == 0)
                items[selectedItemIndex].ActivateWeapon(true);
            else
                items[selectedItemIndex].ActivateWeapon(false);
            items[selectedItemIndex].manager = this;
        }
    }

    // Update is called once per frame
    /*void Update()
    {
        //Select primary weapon when pressing 1
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
         items[0].ActivateWeapon(true);
            selectedItems = items[0];
            for (selectedItemIndex = 1; selectedItemIndex < items.Length; selectedItemIndex++)
            {
             items[selectedItemIndex].ActivateWeapon(false);
            }
        }

        //Select secondary weapon when pressing 2
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
         items[1].ActivateWeapon(true);
            selectedItems = items[1];
         items[0].ActivateWeapon(false);
            for (selectedItemIndex = 2; selectedItemIndex < items.Length; selectedItemIndex++)
            {
             items[selectedItemIndex].ActivateWeapon(false);
            }
        }
        //Select third weapon when pressing 3
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
         items[2].ActivateWeapon(true);
            selectedItems = items[2];
         items[0].ActivateWeapon(false);
         items[1].ActivateWeapon(false);
            for (selectedItemIndex = 3; selectedItemIndex < items.Length; selectedItemIndex++)
            {
             items[selectedItemIndex].ActivateWeapon(false);
            }
        }
        //Select fourth weapon when pressing 4
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
         items[3].ActivateWeapon(true);
            selectedItems = items[3];
         items[0].ActivateWeapon(false);
         items[1].ActivateWeapon(false);
         items[2].ActivateWeapon(false);
            for (selectedItemIndex = 4; selectedItemIndex < items.Length; selectedItemIndex++)
            {
             items[selectedItemIndex].ActivateWeapon(false);
            }
        }
        //Select last weapon when pressing 5
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
         items[4].ActivateWeapon(true);
            selectedItems = items[4];
            for (selectedItemIndex = 0; selectedItemIndex < items.Length - 1; selectedItemIndex++)
            {
             items[selectedItemIndex].ActivateWeapon(false);
            }
        }
    }
}*/
}