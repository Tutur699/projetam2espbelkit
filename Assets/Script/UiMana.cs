using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
public class UiMana : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerInGameText;
    private void Awake()
    {
        Cursor.visible = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Started Host...");
            }
            else
            {
                Debug.Log("Failed to start Host...");
            }
        });

        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Started Client...");
            }
            else
            {
                Debug.Log("Failed to start Client...");
            }
        });

        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Started Host...");
            }
            else
            {
                Debug.Log("Failed to start Host...");
            }
        });



    }

    // Update is called once per frame
    void Update()
    {
        //PlayerNameText.text = "Player Name: " + NetworkManagerCustom.Singleton.PlayerName;
        //PlayerInGameText.text = "Players In Game: " + NetworkManagerCustom.Singleton.PlayerInGame + "/" + NetworkManagerCustom.Singleton.MaxPlayers;

    }
}
