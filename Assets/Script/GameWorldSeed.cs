using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkObject))]
public class GameWorldSeed : NetworkBehaviour
{
    private readonly NetworkVariable<int> netSeed =
        new(writePerm: NetworkVariableWritePermission.Server);

    private bool generated;

    public override void OnNetworkSpawn()
    {
        // HOST/SERVER
        if (IsServer)
        {
            // Si aucun seed encore, on en crée un
            if (netSeed.Value == 0)
                netSeed.Value = Random.Range(1, int.MaxValue);

            GenerateAll(netSeed.Value); // ✅ génère tout de suite côté Host
            generated = true;
        }
        else
        {
            // CLIENT : seed déjà connu ?
            if (netSeed.Value != 0)
            {
                GenerateAll(netSeed.Value);
                generated = true;
            }

            // sinon on attend la synchro du seed
            netSeed.OnValueChanged += (_, s) =>
            {
                if (!generated && s != 0)
                {
                    GenerateAll(s);
                    generated = true;
                }
            };
        }
    }

    private void GenerateAll(int seed)
    {
        var gens = FindObjectsOfType<RandomMurPorte>(true);
        Debug.Log($"[WORLD] Seed={seed} | generators found={gens.Length}");
        foreach (var g in gens) g.Generate(seed);
    }
}
