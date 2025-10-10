using UnityEngine;
using System;

public class RandomMurPorte : MonoBehaviour
{
    //X ->, Y ^, Z <-
    [Header("Fenêtre")]
    public GameObject Fenetre;
    public Material materialFenetre;
    public bool loadWindowFromResources = false;
    public string resourcesWindowPath = "WALL/MyWindow";

    public float lfenetre = 2.0f;  
    public float hfenetre = 1.5f;  
    public float Lfenetre = 0.2f;  
    public float margeFenetre = 0.5f;

    [Range(0f,1f)] public float probaFenetre = 1f; // 1=toujours, 0.5=une fois sur deux

    [Header("Prefabs")]
    public GameObject Mur;
    public GameObject Porte; // porte

    [Header("Matérials")]
    public Material materialMur;
    public Material materialPorte;
    public string resourcesWallPath = "WALL/MyWall";
    public string resourcesDoorPath = "WALL/MyDoor"; // chemin pour la porte
    public bool loadWallFromResources = false;
    public bool loadDoorFromResources = false;



    [Header("Mur")]
    public float hmur = 5f;     // hauteur
    public float Lmur = 0.5f;   // épaisseur mur
    public string choixWall = "";   
    

    [Header("Tirage aléatoire des dimensions au sol")]
    public float minLongueurX = 10f;  
    public float maxLongueurX = 30f;  
    public float minLargeurZ  = 8f;   
    public float maxLargeurZ  = 20f;  
    private static System.Random rnd = new System.Random(); //création du random à utiliser via rnd. etc

    [Header("Dimensions porte")]
    public float lporte = 2f;   // largeur (X ou Z selon mur)
    public float hporte = 3f;   // hauteur
    public float Lporte = 0.3f; // épaisseur
    public float marge  = 0.5f; // marge par rapport aux bords du mur
    

    private Material _resWallMat;
    private Material _resDoorMat;
    private Material _resWindowMat;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _resWallMat = EstMatOK();//évitez des répétitions
        _resDoorMat = ResolveDoorMaterial();
        _resWindowMat = ResolveWindowMaterial();
        Build();
    }

    float MyRandom(System.Random r, float min, float max)
    {
        return min + (float)r.NextDouble() * (max - min);
    }
    Material ResolveWindowMaterial()
    {
        Material mat = materialFenetre;
        if (loadWindowFromResources)
        {
            mat = Resources.Load<Material>(resourcesWindowPath);
            if (mat == null)
                Debug.LogError("[RandomMurPorte] Material fenêtre introuvable : " + resourcesWindowPath);
        }
        return mat;
    }
    Material EstMatOK()
    {
        Material mat = materialMur;

        if (loadWallFromResources)
        {
            string fullPath = string.IsNullOrEmpty(choixWall)
                ? resourcesWallPath
                : resourcesWallPath + "/" + choixWall;

            mat = Resources.Load<Material>(fullPath);
            if (mat == null)
            {
                Debug.LogError("[RandomMurPorte] Material introuvable via Resources à ce chemin : " + fullPath);
            }
        }

        if (mat == null)
        {
            Debug.LogWarning("[RandomMurPorte] Aucun material de mur défini. Les murs utiliseront le material par défaut du prefab.");
        }

        return mat;
    }
    Material ResolveDoorMaterial()
    {
        Material mat = materialPorte;

        if (loadDoorFromResources)
        {
            mat = Resources.Load<Material>(resourcesDoorPath);
            if (mat == null)
            {
                Debug.LogError("[RandomMurPorte] Material porte introuvable : " + resourcesDoorPath);
            }
        }

        return mat;
    }
    // wallIndex : 0=bas (Z=0), 1=haut (Z=lZ), 2=gauche (X=0), 3=droit (X=lX)
void ComputeOnWall(
        int wallIndex, float lX, float lZ,
        float largeur, float hauteur, float epaisseur, float marge,
        out Vector3 pos, out Vector3 scale, out Quaternion rot)
    {
        float px, pz, py;
        rot = Quaternion.identity;

        switch (wallIndex)
        {
            case 0: // bas, le mur est horizontal le long de X, face vers -Z / +Z
                px = MyRandom(rnd, marge, lX - (largeur + marge));
                py = MyRandom(rnd, marge, hmur - (hauteur + marge));
                pos = new Vector3(px + largeur/2f, py + hauteur/2f, 0f);
                scale = new Vector3(largeur, hauteur, epaisseur);
                rot   = Quaternion.identity;
                break;

            case 1: // haut
                px = MyRandom(rnd, marge, lX - (largeur + marge));
                py = MyRandom(rnd, marge, hmur - (hauteur + marge));
                pos = new Vector3(px + largeur/2f, py + hauteur/2f, lZ);
                scale = new Vector3(largeur, hauteur, epaisseur);
                rot   = Quaternion.identity;
                break;

            case 2: // gauche (mur tourné 90°)
                pz = MyRandom(rnd, marge, lZ - (largeur + marge));
                py = MyRandom(rnd, marge, hmur - (hauteur + marge));
                pos = new Vector3(0f, py + hauteur/2f, pz + largeur/2f);
                scale = new Vector3(epaisseur, hauteur, largeur);
                rot   = Quaternion.Euler(0f, 90f, 0f);
                break;

            default: // 3 droit
                pz = MyRandom(rnd, marge, lZ - (largeur + marge));
                py = MyRandom(rnd, marge, hmur - (hauteur + marge));
                pos = new Vector3(lX, py + hauteur/2f, pz + largeur/2f);
                scale = new Vector3(epaisseur, hauteur, largeur);
                rot   = Quaternion.Euler(0f, 90f, 0f);
                break;
        }
    }


    void Tilingdumaterial(GameObject go, float tilesParMetre_U, float tilesParMetre_V)
    {
        var rend = go.GetComponent<Renderer>();
        if (!rend)
        {
            return;
        }
        var mat = rend.material;
        Vector3 p = go.transform.localScale;
        float u = Mathf.Max(0.001f, p.x * tilesParMetre_U);
        float v = Mathf.Max(0.001f, p.y * tilesParMetre_V);
        mat.SetTextureScale("_BaseMap", new Vector2(u, v));
    }

    void ApplyMaterialRecursivement(GameObject go, Material mat)
    {   
        if (mat == null)
        {
            return;
        }
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (var rend in renderers)
        {
            rend.material = mat; // instance par renderer
        }
    }
    void Build()
    {
        float tU = 0.5f;
        float tV = 0.5f;
        float lX = MyRandom(rnd, minLongueurX, maxLongueurX);
        float lZ = MyRandom(rnd, minLargeurZ, maxLargeurZ);
        var murbas = Instantiate(Mur, transform);
        murbas.name = "MurBas";
        murbas.transform.localScale = new Vector3(lX, hmur, Lmur);
        murbas.transform.localPosition = new Vector3(lX / 2f, hmur / 2f, 0f);
        ApplyMaterialRecursivement(murbas, _resWallMat);
        var murgauche = Instantiate(Mur, transform);
        murgauche.name = "MurGauche";
        murgauche.transform.localScale = new Vector3(lZ, hmur, Lmur);
        murgauche.transform.localPosition = new Vector3(0f, hmur / 2f, lZ / 2f);
        murgauche.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        ApplyMaterialRecursivement(murgauche, _resWallMat);
        var murhaut = Instantiate(Mur, transform);
        murhaut.name = "MurHaut";
        murhaut.transform.localScale = new Vector3(lX, hmur, Lmur);
        murhaut.transform.localPosition = new Vector3(lX / 2f, hmur / 2f, lZ);
        ApplyMaterialRecursivement(murhaut, _resWallMat);
        var murdroit = Instantiate(Mur, transform);
        murdroit.name = "MurDroit";
        murdroit.transform.localScale = new Vector3(lZ, hmur, Lmur);
        murdroit.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        murdroit.transform.localPosition = new Vector3(lX, hmur / 2f, lZ / 2f);
        ApplyMaterialRecursivement(murdroit, _resWallMat);
        Tilingdumaterial(murbas, tU, tV);
        Tilingdumaterial(murdroit, tU, tV);
        Tilingdumaterial(murgauche, tU, tV);
        Tilingdumaterial(murhaut, tU, tV);


        int murChoisi = rnd.Next(0, 4); // 0=bas,1=haut,2=gauche,3=droit
        Vector3 posPorte = new Vector3(0, 0, 0);
        Vector3 scalePorte = new Vector3(0, 0, 0);

        if (murChoisi == 0) // BAS
        {
            float px = MyRandom(rnd, marge, lX - (lporte + marge));
            float py = MyRandom(rnd, marge, hmur - (hporte + marge));
            posPorte = new Vector3(px + lporte / 2f, py + hporte / 2f, 0f);
            scalePorte = new Vector3(lporte, hporte, Lporte);
        }
        else if (murChoisi == 1) // HAUT
        {
            float px = MyRandom(rnd, marge, lX - (lporte + marge));
            float py = MyRandom(rnd, marge, hmur - (hporte + marge));
            posPorte = new Vector3(px + lporte / 2f, py + hporte / 2f, lZ);
            scalePorte = new Vector3(lporte, hporte, Lporte);
        }
        else if (murChoisi == 2) // GAUCHE
        {
            float pz = MyRandom(rnd, marge, lZ - (lporte + marge));
            float py = MyRandom(rnd, marge, hmur - (hporte + marge));
            posPorte = new Vector3(0f, py + hporte / 2f, pz + lporte / 2f);
            scalePorte = new Vector3(Lporte, hporte, lporte);
        }
        else if (murChoisi == 3) // DROIT
        {
            float pz = MyRandom(rnd, marge, lZ - (lporte + marge));
            float py = MyRandom(rnd, marge, hmur - (hporte + marge));
            posPorte = new Vector3(lX, py + hporte / 2f, pz + lporte / 2f);
            scalePorte = new Vector3(Lporte, hporte, lporte);
        }

        var portecreer = Instantiate(Porte, transform);
        portecreer.name = "PorteSortie";
        portecreer.transform.localScale = scalePorte;
        portecreer.transform.localPosition = posPorte;
        ApplyMaterialRecursivement(portecreer, _resDoorMat);

        if (Fenetre != null && UnityEngine.Random.value <= probaFenetre)
        {
            int murPorte = murChoisi; // pour éviter chevauchement
            int murFenetre = rnd.Next(0, 4);

            float margeFenetreEffective =
                (murFenetre == murPorte) ? Mathf.Max(margeFenetre, marge + lporte) : margeFenetre;

            ComputeOnWall(murFenetre, lX, lZ, lfenetre, hfenetre, Lfenetre, margeFenetreEffective,
                        out Vector3 posFen, out Vector3 scaleFen, out Quaternion rotFen);

            var fen = Instantiate(Fenetre, transform);
            fen.name = "Fenetre";
            fen.transform.localScale    = scaleFen;
            fen.transform.localPosition = posFen;
            fen.transform.localRotation = rotFen;
            ApplyMaterialRecursivement(fen, _resWindowMat);
        }
    }

        // Update is called once per frame
    void Update()
    {
        
    }
}
