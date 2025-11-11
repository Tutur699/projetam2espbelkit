using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class GameWorldSeed : NetworkBehaviour
{
    public static GameWorldSeed Instance;
    [SerializeField] int seedInEditor = 1337; // visible dans l’inspecteur

    // seed vu par tous, écrit par le serveur
    public readonly NetworkVariable<int> Seed = new(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    // Tous les générateurs qui se registrent
    readonly List<RandomMurPorte> generators = new();

    void Awake() { Instance = this; }

    public void Register(RandomMurPorte g)
    {
        generators.Add(g);
        // Si on connaît déjà le seed, génère tout de suite
        if (Seed.Value != 0 && g != null) g.Generate(Seed.Value);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Choix du seed côté serveur
            int s = seedInEditor != 0 ? seedInEditor : UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            Seed.Value = s;
        }

        // Quand le seed change (arrivée d’un client ou init), on (re)génère
        Seed.OnValueChanged += (_, __) => GenerateAll();
        if (Seed.Value != 0) GenerateAll();
    }

    void GenerateAll()
    {
        // Rattrapage : enregistre tout ce qui existerait déjà dans la scène
        foreach (var g in FindObjectsOfType<RandomMurPorte>(true))
            if (!generators.Contains(g)) generators.Add(g);

        foreach (var g in generators)
            if (g) g.Generate(Seed.Value);
    }

}
