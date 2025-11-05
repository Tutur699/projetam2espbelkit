using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
public class UiMana : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private TextMeshProUGUI PlayerNameText;
    [SerializeField] private TextMeshProUGUI PlayerInGameText;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private ushort port = 7777;
    private void Awake()
    {
        Cursor.visible = true;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startServerButton.onClick.AddListener(() =>
        {
            var transport = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            transport.SetConnectionData("0.0.0.0", port); // serveur uniquement

            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Started Server...");
            }
            else
            {
                Debug.Log("Failed to start Server...");
            }
        });

        startClientButton.onClick.AddListener(() =>
        {
            var transport = (Unity.Netcode.Transports.UTP.UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
            var ip = string.IsNullOrWhiteSpace(ipInput.text) ? "127.0.0.1" : ipInput.text.Trim();
            transport.SetConnectionData(ip, port);
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
