using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 3;
    public Camera playerCamera;
    public List<PItems> items = new List<PItems>(MAXITEMS);
    public int selectedItemIndex;

    [HideInInspector]
    public PItems selectedItems;

    public void addItem(PItems newItem) //Méthode pour ajouter un item dans l'inventaire
    {
        if (items.Count < MAXITEMS)
        {
            items.Add(newItem);
            newItem.manager = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        //At the start we enable the primary weapon and disable the rest
        SelectItems(0);
    }

    public void SelectItems(int index) //Méthode pour sélectionner une arme dans l'inventaire
    {
        if (index < 0 || index >= items.Count)
        {
            Debug.LogWarning($"Index {index} invalide pour la liste d'items !");
            return;
        }
        if (index >= 0 && index < items.Count && items[index] != null)
        {
            items[index].ActivateWeapon(true);
            selectedItems = items[index];
            selectedItemIndex = index;
            //Désactivation des autres armes
            for (int i = 0; i < items.Count; i++)
            {
                if (i != index && items[i] != null)
                {
                    items[i].ActivateWeapon(false);
                }
            }
        }
    }
}