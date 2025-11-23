using System.Linq;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NetworkStartUI : MonoBehaviour
{
    [Header("Netcode")]
    [SerializeField] private NetworkManager nm;
    [SerializeField] private UnityTransport utp;
    [SerializeField] private LanDiscovery lanDiscovery;

    [Header("UI")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button refreshButton;
    [SerializeField] private Button shutdownButton;
    [SerializeField] private Transform serverListContainer;   // contenu du ScrollView
    [SerializeField] private GameObject serverEntryPrefab;    // prefab avec un Button + TMP_Text
    [SerializeField] private TMP_Text statusText;

    private void Awake()
    {
        if (!nm) nm = NetworkManager.Singleton;
        if (!nm || !utp || !lanDiscovery)
        {
            Debug.LogError("[NET] Références manquantes (NetworkManager / UnityTransport / LanDiscovery).");
            enabled = false;
            return;
        }

        // Qualité "PC" si elle existe
        int qualityIndex = QualitySettings.names.ToList().IndexOf("PC");
        if (qualityIndex >= 0)
        {
            QualitySettings.SetQualityLevel(qualityIndex, true);
            Debug.Log("[INIT] Quality level forcé sur : " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
        }

        hostButton.onClick.AddListener(OnHostClicked);
        refreshButton.onClick.AddListener(RefreshServerListUI);
        shutdownButton.onClick.AddListener(OnShutdownClicked);

        nm.OnServerStarted += () => Debug.Log("[NET] Server/Host démarré");
        nm.OnClientConnectedCallback += id => Debug.Log($"[NET] Client connecté: {id}");
        nm.OnClientDisconnectCallback += id => Debug.Log($"[NET] Client déconnecté: {id}");

        // côté client, on commence directement à écouter les serveurs LAN
        lanDiscovery.StartListening();
    }

    private void Update()
    {
        UpdateStatusLabel();
        // La découverte LAN tourne déjà; on peut rafraîchir la liste automatiquement si tu veux :
        RefreshServerListUI();
    }

    private void UpdateStatusLabel()
    {
        if (!statusText) return;

        string mode = "Idle";
        if (nm.IsHost) mode = "Host";
        else if (nm.IsServer) mode = "Server";
        else if (nm.IsClient) mode = "Client";

        statusText.text = $"Status : {mode}";
    }

    // ---------- Host ----------
    private void OnHostClicked()
    {
        if (nm.IsServer || nm.IsClient)
        {
            Debug.LogWarning("[NET] Déjà connecté.");
            return;
        }

        ushort gamePort = (ushort)lanDiscovery.GetGamePort();

        // Le host écoute sur toutes les interfaces
        utp.SetConnectionData("0.0.0.0", gamePort);
        bool ok = nm.StartHost();

        if (!ok)
        {
            Debug.LogError("[NET] Impossible de démarrer le Host.");
            return;
        }

        // On broadcast le serveur sur le LAN
        lanDiscovery.StartBroadcasting();
        Debug.Log("[NET] Host démarré, découverte LAN active.");
    }

    // ---------- Shutdown ----------
    private void OnShutdownClicked()
    {
        if (!nm.IsServer && !nm.IsClient) return;

        nm.Shutdown();
        lanDiscovery.StopBroadcasting();
        Debug.Log("[NET] Réseau arrêté.");
    }

    // ---------- Liste de serveurs ----------
    private void RefreshServerListUI()
    {
        if (!serverListContainer || !serverEntryPrefab) return;

        // On vide d'abord la liste
        foreach (Transform child in serverListContainer)
        {
            Destroy(child.gameObject);
        }

        var servers = lanDiscovery.Servers;
        if (servers == null || servers.Count == 0) return;

        foreach (var s in servers)
        {
            GameObject entryGO = Instantiate(serverEntryPrefab, serverListContainer);
            var btn = entryGO.GetComponent<Button>();
            var label = entryGO.GetComponentInChildren<TMP_Text>();

            string ip = s.Address;
            int port = s.Port;

            if (label != null)
            {
                label.text = $"{ip}:{port}";
            }

            if (btn != null)
            {
                btn.onClick.AddListener(() => OnServerSelected(ip, (ushort)port));
            }
        }
    }

    private void OnServerSelected(string ip, ushort port)
    {
        if (nm.IsServer || nm.IsClient)
        {
            Debug.LogWarning("[NET] Déjà connecté, impossible de se connecter à un autre serveur.");
            return;
        }

        lanDiscovery.StartListening(); // au cas où

        utp.SetConnectionData(ip, port);
        bool ok = nm.StartClient();

        if (!ok)
        {
            Debug.LogError("[NET] Échec StartClient vers " + ip + ":" + port);
            return;
        }

        Debug.Log("[NET] Connexion au serveur " + ip + ":" + port);
    }
}
