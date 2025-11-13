using Unity.Netcode;
using UnityEngine;

public class PlayerVisibility : NetworkBehaviour
{
    public Camera PlayerCamera;
    public GameObject WorldModel;
    public GameObject LocalOnly;

    void Awake()
    {
        // Sécurise : désactive tout par défaut
        if (PlayerCamera) PlayerCamera.gameObject.SetActive(false);
        if (LocalOnly) LocalOnly.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        // Au spawn réseau, configure selon le propriétaire
        if (IsOwner)
        {
            if (PlayerCamera) PlayerCamera.gameObject.SetActive(true);
            if (LocalOnly) LocalOnly.SetActive(true);
        }

        if (WorldModel) WorldModel.SetActive(true); // visible pour tous
    }
}
