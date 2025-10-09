using UnityEngine;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 3;
    public Camera playerCamera;
    public PItems[] items = new PItems[MAXITEMS];
    public int selectedItemIndex;

    [HideInInspector]
    public PItems selectedItems;

    public void addItem(PItems newItem) //Méthode pour ajouter un item dans l'inventaire
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = newItem;
                newItem.manager = this;
                break;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //At the start we enable the primary weapon and disable the rest
        for (selectedItemIndex = 0; selectedItemIndex < items.Length; selectedItemIndex++)
        {
            if (selectedItemIndex == 0)
            {
                items[selectedItemIndex].ActivateWeapon(true);
                selectedItems = items[selectedItemIndex];
                items[selectedItemIndex].manager = this;
            }
            else
            {
                items[selectedItemIndex].ActivateWeapon(false);
                items[selectedItemIndex].manager = this;
            }
        }
    }
    public void SelectItems(int index) //Méthode pour sélectionner une arme dans l'inventaire
    {
        if (index < items.Length && items[index] != null)
        {
            items[index].ActivateWeapon(true);
            selectedItems = items[index];
            for (selectedItemIndex = 0; selectedItemIndex < items.Length; selectedItemIndex++)
            {
                if (selectedItemIndex != index && items[selectedItemIndex] != null)
                {
                    items[selectedItemIndex].ActivateWeapon(false);
                }
            }
        }
    }  
}