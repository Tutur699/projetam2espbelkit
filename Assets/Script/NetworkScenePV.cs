using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class NetworkScenePV : MonoBehaviour
{
    [Header("Scène du jeu")]
    [SerializeField] private string gameSceneName = "Terrain1 1";
    [SerializeField] private string gameSceneNameIA = "TerrainFinaleIA";

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    private bool startpartie = false;

    private void Awake()
    {
        var nm = NetworkManager.Singleton;
        if (nm == null)
        {
            Debug.LogError("no network manager");
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
        // Si on est déjà host au moment du Start (rare), on affiche le texte
        if (nm != null && nm.IsServer && statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "En attente de joueurs...";
            Debug.Log("[MATCH] Start : Host détecté");
        }
    }
    private void OnDestroy()
    {
        var nm = NetworkManager.Singleton;
        if(nm != null)
        {
            nm.OnClientConnectedCallback -= OnClientConnected;
            nm.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientID)
    {
        var nm = NetworkManager.Singleton;
        if (nm == null) return;
        if (!nm.IsServer)
        {
            return;
        }
        if(startpartie){
            return;
        }
        int playCount = nm.ConnectedClientsList.Count;
        if (statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = $"En attente de joueurs... ({playCount}/2)";
        }

        if (startpartie)
        {
            Debug.Log("[MATCH] Partie déjà démarrée, OnClientConnected ignoré.");
            return;
        }
        if(playCount == 2)
        {
            Debug.Log("[MATCH] 2 joueurs connectés, lancement de la partie.");
            if (statusText != null)
                statusText.gameObject.SetActive(false);
            StartMatch();
        }

        if (playCount > 2)
        {
            NetworkManager.Singleton.DisconnectClient(clientID);
            Debug.Log($"[MATCH] Client {clientID} déconnecté (trop de joueurs).");
        }
        


        
    }
    private void OnClientDisconnected(ulong clientId)
    {
        var nm = NetworkManager.Singleton;
        if (nm == null || !nm.IsServer) return;

        int playCount = nm.ConnectedClientsList.Count;
        Debug.Log($"[MATCH] Client déconnecté (id={clientId}). Joueurs restants = {playCount}");

        if (!startpartie && statusText != null)
        {
            statusText.gameObject.SetActive(true);
            statusText.text = "Un joueur s'est déconnecté. En attente de joueurs...";
        }
    }
    private void StartMatch()
    {
        string sceneT;
        if (startpartie)
        {
            return;
        }
        startpartie = true;
        if(Random.value < 0.5f)
        {
            sceneT = gameSceneName;

        }
        else
        {
            sceneT = gameSceneNameIA;
        }
        NetworkManager.Singleton.SceneManager.LoadScene(sceneT, LoadSceneMode.Single);
    }
}