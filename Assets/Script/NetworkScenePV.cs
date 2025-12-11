using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkScenePV : NetworkBehaviour
{
    [Header("Scènes de jeu")]
    [SerializeField] private string gameSceneNamePvp = "Terrain1 1";
    [SerializeField] private string gameSceneNameSoloIA = "TerrainFinaleIA";

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;

    private bool matchStarted = false;

    private void Awake()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            enabled = false;
            return;
        }

        nm.OnClientConnectedCallback += OnClientConnected;
        nm.OnClientDisconnectCallback += OnClientDisconnected;

        if (statusText != null)
            statusText.gameObject.SetActive(false);
    }

    private void Start()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null && nm.IsServer && statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "En attente de joueurs... (1/2)";
        }
    }

    private void OnDestroy()
    {
        var nm = NetworkManager.Singleton;
        if (nm != null)
        {
            nm.OnClientConnectedCallback -= OnClientConnected;
            nm.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientID)
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsServer) return;
        if (matchStarted) return;

        int count = nm.ConnectedClientsList.Count;

        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = $"En attente de joueurs... ({count}/2)";
        }

        if (count == 2)
        {
            matchStarted = true;

            bool pvp = Random.value < 0.5f;

            if (pvp)
            {
                if (statusText != null) statusText.gameObject.SetActive(false);
                NetworkManager.Singleton.SceneManager.LoadScene(gameSceneNamePvp, LoadSceneMode.Single);
            }
            else
            {
                if (statusText != null) statusText.text = "Lancement des parties solo...";
                StartSoloMatchesClientRpc();
            }
        }

        if (count > 2)
        {
            NetworkManager.Singleton.DisconnectClient(clientID);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsServer) return;

        if (!matchStarted && statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "Un joueur s'est déconnecté. En attente de joueurs...";
        }
    }

    [ClientRpc]
    private void StartSoloMatchesClientRpc()
    {
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.Shutdown();
        }

        SceneManager.LoadScene(gameSceneNameSoloIA, LoadSceneMode.Single);
    }
}
