using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;

    void OnEnable()
    {
        // Optionnel : si la caméra est sur un autre objet
        if (playerCamera != null) playerCamera.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        // On n’active la caméra que pour le propriétaire local
        if (IsOwner && playerCamera != null)
            playerCamera.gameObject.SetActive(true);

    }
}

