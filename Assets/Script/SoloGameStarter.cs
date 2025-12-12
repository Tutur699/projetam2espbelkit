using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Collections;

public class SoloSimpleStart : MonoBehaviour
{
    IEnumerator Start()
    {
        // On attend que le NOUVEAU NetworkManager s'initialise
        yield return null;

        if (NetworkManager.Singleton != null)
        {
            // On force le port 7779
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null) transport.SetConnectionData("127.0.0.1", 7779);

            Debug.Log("Démarrage du Solo sur le port 7779...");
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.LogError("Pas de NetworkManager trouvé ! As-tu bien mis le prefab dans la scène Solo ?");
        }
    }
}