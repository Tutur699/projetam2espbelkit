using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log($"[PlayerSpawn] OnNetworkSpawn sur {name}, IsOwner={IsOwner}");

        // On ne bouge que le joueur local
        if (!IsOwner) return;

        GameObject spawn = GameObject.FindWithTag("Spawn");
        Debug.Log($"[PlayerSpawn] Spawn trouvé ? {spawn != null}");

        if (spawn != null)
        {
            transform.position = spawn.transform.position;
            Debug.Log($"[PlayerSpawn] Nouvelle position : {transform.position}");
        }
        else
        {
            Debug.LogWarning("Aucun objet avec le tag 'Spawn' trouvé dans la scène.");
        }
    }
}
