using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            Debug.Log("[PlayerSpawn] Pas le owner, je ne bouge pas ce joueur.");
            return;
        }

        GameObject spawn = GameObject.FindWithTag("Spawn");
        if (spawn == null)
        {
            Debug.LogWarning("[PlayerSpawn] Aucun objet avec le tag 'Spawn' trouvé dans la scène.");
            return;
        }

        Debug.Log("[PlayerSpawn] Spawn trouvé à " + spawn.transform.position);

        // VERSION SIMPLE : on met le joueur légèrement au-dessus du spawn
        Vector3 targetPos = spawn.transform.position + Vector3.up * 1.5f;
        transform.position = targetPos;

        Debug.Log("[PlayerSpawn] Joueur déplacé à " + transform.position);
    }
}
