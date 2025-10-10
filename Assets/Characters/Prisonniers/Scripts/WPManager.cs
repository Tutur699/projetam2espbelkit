using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class WPManager : MonoBehaviour
{
    public const int MAXITEMS = 3;
    public Camera playerCamera;
    public List<PItems> items = new List<PItems>(MAXITEMS);
    public int selectedItemIndex;

    public Image[] itemIcons = new Image[5]; //Array to hold the UI icons for the items
    public Image[] itemBackgrounds = new Image[5]; //Array to hold the UI backgrounds for the item icons
    public Sprite defaultIcon; //Default icon to use when no item is equipped


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

    public void Update() //Mise à jour de l'interface utilisateur
    {
        /*for (int i = 0; i < 5; i++)
        {
            if (i < items.Count && items[i] != null)
            {
                itemIcons[i].sprite = items[i].icon;
                itemIcons[i].enabled = true;
                itemBackgrounds[i].enabled = true;
            }
            else
            {
                itemIcons[i].sprite = defaultIcon;
                itemIcons[i].enabled = false;
                itemBackgrounds[i].enabled = false;
            }
        }*/
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