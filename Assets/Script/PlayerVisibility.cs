using Unity.Netcode;
using UnityEngine;

public class PlayerVisibility : NetworkBehaviour
{
    [Header("Assigne dans le PREFAB")]
    public Camera playerCamera;   // Caméra du joueur (désactivée par défaut dans le prefab)
    public GameObject localOnly;  // bras/armes FPS (optionnel)
    public GameObject worldModel; // le mesh visible par les autres

    public override void OnNetworkSpawn()
    {
        // Caméra uniquement pour le propriétaire
        if (playerCamera) playerCamera.gameObject.SetActive(IsOwner);

        if (localOnly)   localOnly.SetActive(IsOwner);

        // Le modèle monde doit être visible pour tout le monde
        if (worldModel)  worldModel.SetActive(true);

        // Sécurité : s'assurer que les MeshRenderer sont enabled
        if (worldModel)
        {
            foreach (var r in worldModel.GetComponentsInChildren<Renderer>(true))
                r.enabled = true;
        }
    }
}
