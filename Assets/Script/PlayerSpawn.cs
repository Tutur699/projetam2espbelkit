using Unity.Netcode;
using UnityEngine;

public class PlayerSpawn : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        GameObject spawn = GameObject.FindWithTag("Spawn");
        if (spawn == null)
        {
            Debug.LogWarning("Aucun objet avec le tag 'Spawn' trouvé dans la scène.");
            return;
        }

        // On récupère le CharacterController pour connaître l’offset des pieds
        var cc = GetComponentInChildren<CharacterController>();
        float footOffset = 0f;

        if (cc != null)
        {
            // centre de la capsule - moitié de la hauteur = position des pieds
            footOffset = cc.center.y - cc.height * 0.5f;
        }

        // On place le joueur de façon à ce que les PIEDS soient sur le Spawn
        Vector3 targetPos = spawn.transform.position - new Vector3(0f, footOffset, 0f);
        transform.position = targetPos;
    }
}
