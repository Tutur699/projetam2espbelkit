using Unity.Netcode;
using UnityEngine;

public class GameWorldSeed : NetworkBehaviour
{
    public int seedInEditor = 1337;
    private NetworkVariable<int> seed = new(writePerm: NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            seed.Value = seedInEditor; // ou Random.Range fixe côté serveur
            GenerateAll(seed.Value);
            GenerateAllClientRpc(seed.Value); // force le client à générer
        }
    }

    [ClientRpc]
    void GenerateAllClientRpc(int s)
    {
        GenerateAll(s);
    }

    void GenerateAll(int s)
    {
        var gens = FindObjectsOfType<RandomMurPorte>(true);
        Debug.Log($"[WORLD] Seed={s} | generators={gens.Length}");
        foreach (var g in gens) g.Generate(s);
    }
}
