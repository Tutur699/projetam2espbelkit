using UnityEngine;
using System;

public class RandomMurPorte : MonoBehaviour
{
    
    [Header("Fenêtre")]
    public GameObject Fenetre;                 
    public Material materialFenetre;
    public bool loadWindowFromResources = false;
    public string resourcesWindowPath = "WALL/MyWindow";

    public float lfenetre = 2.0f;              // largeur ouverture
    public float hfenetre = 1.5f;              // hauteur ouverture
    public float Lfenetre = 0.2f;              // épaisseur du visuel fenêtre
    public float margeFenetre = 0.5f;          // marge latérale / verticale
    [Range(0f,1f)] public float probaFenetre = 1f; // 1=toujours

    
    [Header("Prefabs")]
    public GameObject Mur;                     // prefab d’un mur “bloc” (cube)
    public GameObject Porte;                   // prefab visuel de porte (cube/mesh)

    [Header("Matériaux")]
    public Material materialMur;
    public Material materialPorte;
    public string resourcesWallPath = "WALL/MyWall";
    public string resourcesDoorPath = "WALL/MyDoor";
    public bool loadWallFromResources = false;
    public bool loadDoorFromResources = false;

    // === Paramètres mur / bâtiment =========================================
    [Header("Mur")]
    public float hmur = 5f;       // hauteur des murs
    public float Lmur = 0.5f;     // épaisseur des murs
    public string choixWall = "";

    [Header("Tirage aléatoire des dimensions au sol")]
    public float minLongueurX = 10f;
    public float maxLongueurX = 30f;
    public float minLargeurZ  = 8f;
    public float maxLargeurZ  = 20f;
    private static System.Random rnd = new System.Random();

    [Header("Dimensions porte")]
    public float lporte = 2f;     // largeur ouverture
    public float hporte = 3f;     // hauteur ouverture
    public float Lporte = 0.3f;   // épaisseur du visuel porte
    public float marge  = 0.5f;   // marge latérale / verticale

    
    private Material _resWallMat;
    private Material _resDoorMat;
    private Material _resWindowMat;

    void Start()
    {
        _resWallMat   = ResolveWallMaterial();
        _resDoorMat   = ResolveDoorMaterial();
        _resWindowMat = ResolveWindowMaterial();
        Build();
    }


    Material ResolveWallMaterial()
    {
        Material mat = materialMur;
        if (loadWallFromResources)
        {
            string fullPath = string.IsNullOrEmpty(choixWall)
                ? resourcesWallPath
                : resourcesWallPath + "/" + choixWall;

            mat = Resources.Load<Material>(fullPath);
            if (mat == null) Debug.LogError("[RandomMurPorte] Matériau mur introuvable: " + fullPath);
        }
        if (mat == null)
            Debug.LogWarning("[RandomMurPorte] Aucun matériau mur défini (le prefab utilisera son mat).");
        return mat;
    }

    Material ResolveDoorMaterial()
    {
        Material mat = materialPorte;
        if (loadDoorFromResources)
        {
            mat = Resources.Load<Material>(resourcesDoorPath);
            if (mat == null) Debug.LogError("[RandomMurPorte] Matériau porte introuvable: " + resourcesDoorPath);
        }
        return mat;
    }

    Material ResolveWindowMaterial()
    {
        Material mat = materialFenetre;
        if (loadWindowFromResources)
        {
            mat = Resources.Load<Material>(resourcesWindowPath);
            if (mat == null) Debug.LogError("[RandomMurPorte] Matériau fenêtre introuvable: " + resourcesWindowPath);
        }
        return mat;
    }

    void ApplyMaterialRecursivement(GameObject go, Material mat)
    {
        if (mat == null) return;
        var renderers = go.GetComponentsInChildren<Renderer>(true);
        foreach (var r in renderers) r.material = mat;
    }

    void Tilingdumaterial(GameObject go, float tilesParMetre_U, float tilesParMetre_V)
    {
        var rend = go.GetComponent<Renderer>();
        if (!rend) return;
        var mat = rend.material;
        Vector3 p = go.transform.localScale;
        float u = Mathf.Max(0.001f, p.x * tilesParMetre_U);
        float v = Mathf.Max(0.001f, p.y * tilesParMetre_V);
        mat.SetTextureScale("_BaseMap", new Vector2(u, v));
    }

    GameObject CreateWallSeg(string name, Vector3 center, Vector3 size, Quaternion rot)
    {
        var seg = Instantiate(Mur, transform);
        seg.name = name;
        seg.transform.localRotation = rot;
        seg.transform.localPosition = center;
        seg.transform.localScale    = size;
        ApplyMaterialRecursivement(seg, _resWallMat);
        Tilingdumaterial(seg, 0.5f, 0.5f);
        return seg;
    }

    // Mur aligné X (bas/haut) avec éventuelle ouverture rectangulaire
    void BuildXWallWithOpening(
        string baseName, float zFix, float lenX, float h, float thick,
        bool hasOpening, float xMin=0, float xMax=0, float yMin=0, float yMax=0)
    {
        Quaternion rot = Quaternion.identity;

        if (!hasOpening)
        {
            CreateWallSeg(baseName, new Vector3(lenX/2f, h/2f, zFix), new Vector3(lenX, h, thick), rot);
            return;
        }

        // Gauche
        if (xMin > 0f)
        {
            float L = xMin;
            CreateWallSeg(baseName+"_Left",
                new Vector3(L/2f, h/2f, zFix),
                new Vector3(L, h, thick), rot);
        }
        // Droite
        if (xMax < lenX)
        {
            float L = lenX - xMax;
            CreateWallSeg(baseName+"_Right",
                new Vector3(xMax + L/2f, h/2f, zFix),
                new Vector3(L, h, thick), rot);
        }
        // Dessus
        if (yMax < h)
        {
            float H = h - yMax;
            CreateWallSeg(baseName+"_Top",
                new Vector3((xMin+xMax)/2f, yMax + H/2f, zFix),
                new Vector3(xMax - xMin, H, thick), rot);
        }
        // Dessous (utile pour fenêtre, pas porte)
        if (yMin > 0f)
        {
            float H = yMin;
            CreateWallSeg(baseName+"_Bottom",
                new Vector3((xMin+xMax)/2f, H/2f, zFix),
                new Vector3(xMax - xMin, H, thick), rot);
        }
    }

    // Mur aligné Z (gauche/droit) avec éventuelle ouverture
    void BuildZWallWithOpening(
        string baseName, float xFix, float lenZ, float h, float thick,
        bool hasOpening, float zMin=0, float zMax=0, float yMin=0, float yMax=0)
    {
        Quaternion rot = Quaternion.Euler(0f, 90f, 0f);

        if (!hasOpening)
        {
            CreateWallSeg(baseName,
                new Vector3(xFix, h/2f, lenZ/2f),
                new Vector3(lenZ, h, thick), rot);
            return;
        }

        // Gauche (en Z-)
        if (zMin > 0f)
        {
            float L = zMin;
            CreateWallSeg(baseName+"_Left",
                new Vector3(xFix, h/2f, L/2f),
                new Vector3(L, h, thick), rot);
        }
        // Droite (en Z+)
        if (zMax < lenZ)
        {
            float L = lenZ - zMax;
            CreateWallSeg(baseName+"_Right",
                new Vector3(xFix, h/2f, zMax + L/2f),
                new Vector3(L, h, thick), rot);
        }
        // Dessus
        if (yMax < h)
        {
            float H = h - yMax;
            CreateWallSeg(baseName+"_Top",
                new Vector3(xFix, yMax + H/2f, (zMin+zMax)/2f),
                new Vector3(zMax - zMin, H, thick), rot);
        }
        // Dessous (fenêtre)
        if (yMin > 0f)
        {
            float H = yMin;
            CreateWallSeg(baseName+"_Bottom",
                new Vector3(xFix, H/2f, (zMin+zMax)/2f),
                new Vector3(zMax - zMin, H, thick), rot);
        }
    }

    void Build()
    {
        float lX = RandomRange(minLongueurX, maxLongueurX);
        float lZ = RandomRange(minLargeurZ, maxLargeurZ);

        int murPorte = rnd.Next(0, 4); // 0=bas(z=0),1=haut(z=lZ),2=gauche(x=0),3=droit(x=lX)
        float p_xMin=0, p_xMax=0, p_yMin=0, p_yMax=0; // pour murs X
        float p_zMin=0, p_zMax=0;                     // pour murs Z

        if (murPorte == 0 || murPorte == 1) // murs alignés X
        {
            float px = RandomRange(marge, lX - (lporte + marge));
            p_xMin = px;
            p_xMax = px + lporte;
            p_yMin = 0f;                   // porte touche le sol
            p_yMax = Mathf.Min(hmur, hporte);
        }
        else // 2 ou 3, murs alignés Z
        {
            float pz = RandomRange(marge, lZ - (lporte + marge));
            p_zMin = pz;
            p_zMax = pz + lporte;
            p_yMin = 0f;
            p_yMax = Mathf.Min(hmur, hporte);
        }

        // ===== FENÊTRE (facultative) =======================================
        bool placeFenetre = (Fenetre != null) && UnityEngine.Random.value <= probaFenetre;
        int murFenetre = -1;
        float f_xMin=0, f_xMax=0, f_yMin=0, f_yMax=0;
        float f_zMin=0, f_zMax=0;

        if (placeFenetre)
        {
            murFenetre = rnd.Next(0, 4);
            if (murFenetre == murPorte) murFenetre = (murFenetre + 1) % 4;

            if (murFenetre == 0 || murFenetre == 1) // murs X
            {
                float fx = RandomRange(margeFenetre, lX - (lfenetre + margeFenetre));
                float fy = RandomRange(1.0f, hmur - (hfenetre + 0.5f));
                f_xMin = fx;
                f_xMax = fx + lfenetre;
                f_yMin = fy;
                f_yMax = fy + hfenetre;
            }
            else // murs Z
            {
                float fz = RandomRange(margeFenetre, lZ - (lfenetre + margeFenetre));
                float fy = RandomRange(1.0f, hmur - (hfenetre + 0.5f));
                f_zMin = fz;
                f_zMax = fz + lfenetre;
                f_yMin = fy;
                f_yMax = fy + hfenetre;
            }
        }

        // Mur BAS (z=0) – aligné X (face extérieure en +Z)
        bool holeOnBas = (murPorte == 0) || (placeFenetre && murFenetre == 0);
        BuildXWallWithOpening(
            "MurBas",
            +Lmur * 0.5f,
            lX, hmur, Lmur,
            holeOnBas,
            (murPorte==0)?p_xMin:f_xMin,
            (murPorte==0)?p_xMax:f_xMax,
            (murPorte==0)?p_yMin:f_yMin,
            (murPorte==0)?p_yMax:f_yMax
        );

        // Mur HAUT (z=lZ) – aligné X (face extérieure en -Z, mais on garde repère cohérent)
        bool holeOnHaut = (murPorte == 1) || (placeFenetre && murFenetre == 1);
        BuildXWallWithOpening(
            "MurHaut",
            lZ - Lmur * 0.5f,
            lX, hmur, Lmur,
            holeOnHaut,
            (murPorte==1)?p_xMin:f_xMin,
            (murPorte==1)?p_xMax:f_xMax,
            (murPorte==1)?p_yMin:f_yMin,
            (murPorte==1)?p_yMax:f_yMax
        );

        // Mur GAUCHE (x=0) – aligné Z (face extérieure en +X)
        bool holeOnGauche = (murPorte == 2) || (placeFenetre && murFenetre == 2);
        BuildZWallWithOpening(
            "MurGauche",
            +Lmur * 0.5f,
            lZ, hmur, Lmur,
            holeOnGauche,
            (murPorte==2)?p_zMin:f_zMin,
            (murPorte==2)?p_zMax:f_zMax,
            (murPorte==2)?p_yMin:f_yMin,
            (murPorte==2)?p_yMax:f_yMax
        );

        // Mur DROIT (x=lX) – aligné Z (face extérieure en -X)
        bool holeOnDroit = (murPorte == 3) || (placeFenetre && murFenetre == 3);
        BuildZWallWithOpening(
            "MurDroit",
            lX - Lmur * 0.5f,
            lZ, hmur, Lmur,
            holeOnDroit,
            (murPorte==3)?p_zMin:f_zMin,
            (murPorte==3)?p_zMax:f_zMax,
            (murPorte==3)?p_yMin:f_yMin,
            (murPorte==3)?p_yMax:f_yMax
        );

        // Porte affleurante à la face extérieure
        {
            float offset = (Lmur - Lporte) * 0.5f; // “à fleur”
            var p = Instantiate(Porte, transform);
            ApplyMaterialRecursivement(p, _resDoorMat);

            if (murPorte == 0) // bas (face +Z)
            {
                p.transform.localPosition = new Vector3((p_xMin+p_xMax)/2f, (p_yMin+p_yMax)/2f, +Lmur*0.5f - offset);
                p.transform.localRotation = Quaternion.identity;
                p.transform.localScale    = new Vector3(lporte, hporte, Lporte);
            }
            else if (murPorte == 1) // haut (face -Z côté intérieur -> on place vers l'extérieur logique)
            {
                p.transform.localPosition = new Vector3((p_xMin+p_xMax)/2f, (p_yMin+p_yMax)/2f, lZ - (Lmur*0.5f - offset));
                p.transform.localRotation = Quaternion.identity;
                p.transform.localScale    = new Vector3(lporte, hporte, Lporte);
            }
            else if (murPorte == 2) // gauche (face +X)
            {
                p.transform.localPosition = new Vector3(+Lmur*0.5f - offset, (p_yMin+p_yMax)/2f, (p_zMin+p_zMax)/2f);
                p.transform.localRotation = Quaternion.Euler(0, 90, 0);
                p.transform.localScale    = new Vector3(Lporte, hporte, lporte);
            }
            else // droit (face -X)
            {
                p.transform.localPosition = new Vector3(lX - (Lmur*0.5f - offset), (p_yMin+p_yMax)/2f, (p_zMin+p_zMax)/2f);
                p.transform.localRotation = Quaternion.Euler(0, 90, 0);
                p.transform.localScale    = new Vector3(Lporte, hporte, lporte);
            }
        }

        // Fenêtre visuelle (si prefab fourni)
        if (placeFenetre)
        {
            var f = Instantiate(Fenetre, transform);
            ApplyMaterialRecursivement(f, _resWindowMat);

            if (murFenetre == 0)
            {
                f.transform.localPosition = new Vector3((f_xMin+f_xMax)/2f, (f_yMin+f_yMax)/2f, +Lmur*0.5f);
                f.transform.localRotation = Quaternion.identity;
                f.transform.localScale    = new Vector3(lfenetre, hfenetre, Lfenetre);
            }
            else if (murFenetre == 1)
            {
                f.transform.localPosition = new Vector3((f_xMin+f_xMax)/2f, (f_yMin+f_yMax)/2f, lZ - Lmur*0.5f);
                f.transform.localRotation = Quaternion.identity;
                f.transform.localScale    = new Vector3(lfenetre, hfenetre, Lfenetre);
            }
            else if (murFenetre == 2)
            {
                f.transform.localPosition = new Vector3(+Lmur*0.5f, (f_yMin+f_yMax)/2f, (f_zMin+f_zMax)/2f);
                f.transform.localRotation = Quaternion.Euler(0, 90, 0);
                f.transform.localScale    = new Vector3(Lfenetre, hfenetre, lfenetre);
            }
            else // 3
            {
                f.transform.localPosition = new Vector3(lX - Lmur*0.5f, (f_yMin+f_yMax)/2f, (f_zMin+f_zMax)/2f);
                f.transform.localRotation = Quaternion.Euler(0, 90, 0);
                f.transform.localScale    = new Vector3(Lfenetre, hfenetre, lfenetre);
            }
        }
    }

    // Petit helper Random float
    float RandomRange(float min, float max) => min + (float)rnd.NextDouble() * (max - min);
}
