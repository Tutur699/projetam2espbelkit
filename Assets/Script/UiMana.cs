/*using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class UiMana : MonoBehaviour
{
    [SerializeField] private Button startServerButton;
    [SerializeField] private Button startClientButton;
    [SerializeField] private Button startHostButton;
    [SerializeField] private TMP_InputField ipInput;
    [SerializeField] private ushort port = 7777;

    private NetworkManager nm;
    private UnityTransport transport;

    private void Awake()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        nm = NetworkManager.Singleton;
        if (nm == null) { Debug.LogError("No NetworkManager in scene."); return; }

        transport = nm.NetworkConfig.NetworkTransport as UnityTransport;
        if (transport == null) { Debug.LogError("NetworkTransport is not UnityTransport."); }
    }

    private void Start()
    {
        startServerButton.onClick.AddListener(StartServer);
        startClientButton.onClick.AddListener(StartClient);
        startHostButton.onClick.AddListener(StartHost);
    }

    private void StartServer()
    {
        if (!transport) return;

        // écoute toutes interfaces réseau
        transport.SetConnectionData(address: "0.0.0.0", port: port, listenAddress: "0.0.0.0");
        if (nm.StartServer()) Debug.Log("Started Server...");
        else Debug.Log("Failed to start Server...");
    }

    private void StartClient()
    {
        if (!transport) return;

        var ip = string.IsNullOrWhiteSpace(ipInput.text) ? "127.0.0.1" : ipInput.text.Trim();
        // côté client: juste l'IP du serveur + port
        transport.SetConnectionData(address: ip, port: port);
        if (nm.StartClient()) Debug.Log("Started Client...");
        else Debug.Log("Failed to start Client...");
    }

    private void StartHost()
    {
        if (!transport) return;

        // host = serveur + client local
        // adresse du serveur (vue par les clients) + bind sur toutes interfaces
        transport.SetConnectionData(address: "0.0.0.0", port: port, listenAddress: "0.0.0.0");
        if (nm.StartHost()) Debug.Log("Started Host...");
        else Debug.Log("Failed to start Host...");
    }
}
*/