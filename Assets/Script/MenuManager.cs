using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInputField;
    public void StartGame()
    {
        string nom = nameInputField ? nameInputField.text.Trim() : "";
        if (string.IsNullOrEmpty(nom))
        {
            nom = "Player" + Random.Range(1, 9999);
        }
        PlayerProfile.PlayerNom = nom;
        Debug.Log("[MENU] Nom du joueur défini sur : " + PlayerProfile.PlayerNom);
        SceneManager.LoadScene("Terrain1 1"); 
    }
}
