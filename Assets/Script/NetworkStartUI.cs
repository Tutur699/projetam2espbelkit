using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Linq;
using UnityEngine.SceneManagement;

public class NetworkStartUI : MonoBehaviour
{
    [SerializeField] private NetworkManager nm;   // <- glisse ton NetworkManager ici
    [SerializeField] private UnityTransport utp;  // <- et le transport
    [SerializeField] private string defaultIp = "127.0.0.1";
    [SerializeField] private ushort port = 7777;

    private string ipRuntime;

    void Start()
    {
        // Force un profil graphique spécifique (optionnel)
        int qualityIndex = QualitySettings.names.ToList().IndexOf("PC");
        if (qualityIndex >= 0)
        {
            QualitySettings.SetQualityLevel(qualityIndex, true);
            Debug.Log("[INIT] Quality level forcé sur : " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
        }

        ipRuntime = defaultIp;

        if (!nm) nm = NetworkManager.Singleton;
        if (!nm || !utp)
        {
            Debug.LogError("Référence manquante (NetworkManager/UnityTransport).");
            enabled = false;
            return;
        }

        nm.OnServerStarted += () => Debug.Log("[NET] Server/Host démarré");
        nm.OnClientConnectedCallback += id => Debug.Log($"[NET] Client connecté: {id}");
        nm.OnClientDisconnectCallback += id => Debug.Log($"[NET] Client déconnecté: {id}");
    }

    void ConfigureForServer() => utp.SetConnectionData("0.0.0.0", port);
    void ConfigureForClient() => utp.SetConnectionData(ipRuntime, port);

    void OnGUI()
    {
        if (!nm) return;

        float x = 10, y = 10, w = 220, h = 36, p = 8;
        GUI.Label(new Rect(x, y, 400, h), $"Status: {(nm.IsServer ? "Server" : nm.IsClient ? "Client" : "Idle")}");

        y += h + p;
        GUI.Label(new Rect(x, y, 80, h), "Server IP:");
        ipRuntime = GUI.TextField(new Rect(x + 80, y, w - 80, h), ipRuntime);

        y += h + p;

        if (!nm.IsClient && !nm.IsServer)
        {
            if (GUI.Button(new Rect(x, y, w, h), "Host"))
            {
                ConfigureForServer();
                nm.StartHost();

                if (nm.IsServer)
                {
                    // On reste dans la scène actuelle (aucun chargement)
                    var activeScene = SceneManager.GetActiveScene().name;
                    var sm = nm.SceneManager;
                   if (sm != null)
                    {
                        Debug.Log($"[NET] Host démarré dans la scène actuelle : {activeScene}");
                        sm.SetClientSynchronizationMode(LoadSceneMode.Single);
                        sm.LoadScene(activeScene, LoadSceneMode.Single);
                    }

                }
            }

            y += h + p;
            if (GUI.Button(new Rect(x, y, w, h), "Client"))
            {
                ConfigureForClient();
                nm.StartClient();
            }

            y += h + p;
            if (GUI.Button(new Rect(x, y, w, h), "Server"))
            {
                ConfigureForServer();
                nm.StartServer();
            }
        }
        else
        {
            if (GUI.Button(new Rect(x, y, w, h), "Shutdown"))
            {
                nm.Shutdown();
                Debug.Log("[NET] Réseau arrêté.");
            }
        }
    }
}
