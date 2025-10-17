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
    [Range(0f, 1f)] public float probaFenetre = 1f; // 1=toujours
    [Range(0,4)] public int nombreFenetres = 1;     // nb de fenêtres à créer (1 max par face)

    [Header("Fenêtre – placement uniforme")]
    [Tooltip("Hauteur (depuis le sol) du bas de TOUTES les fenêtres.")]
    public float hauteurBasFenetre = 1.2f;

    [Header("Placement manuel")]
    [Tooltip("Si false, le mur est choisi aléatoirement.")]
    public bool choisirMurManuellement = false;

    [Tooltip("0 = bas, 1 = haut, 2 = gauche, 3 = droit")]
    [Range(0,3)] public int murPorteManuel = 0;

    [Tooltip("0 = bas, 1 = haut, 2 = gauche, 3 = droit")]
    [Range(0,3)] public int murFenetreManuel = 1;


    
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

    [Header("Porte – calibration auto")]
    public Vector3 doorTargetScale = new Vector3(1.5f, 1.5f, 1.5f); // remplace ton 1.5f hardcodé
    public bool autoFitOpeningToDoor = true; // si true, on taille l’ouverture sur la vraie porte


    [Header("Dimensions porte")]
    public float lporte = 7f;     // largeur ouverture
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
            if (mat == null) Debug.LogError("[RandomMurPorte] Matérial mur introuvable: " + fullPath);
        }
        if (mat == null)
            Debug.LogWarning("[RandomMurPorte] Aucun matérial mur défini (le prefab utilisera son mat).");
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
    // Mesure la taille monde AABB d’un prefab, avec une rotation/scale données.
    // Instancie temporairement puis détruit proprement (Editor ou Play).
    Vector3 MeasurePrefabAABBSize(GameObject prefab, Transform parent, Quaternion rot, Vector3 scale)
    {
        if (prefab == null) return Vector3.zero;

        GameObject tmp = Instantiate(prefab, parent);
        tmp.hideFlags = HideFlags.HideAndDontSave;
        tmp.transform.localPosition = Vector3.zero;
        tmp.transform.localRotation = rot;
        tmp.transform.localScale    = scale;

        var rends = tmp.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0)
        {
            if (Application.isPlaying) Destroy(tmp); else DestroyImmediate(tmp);
            return Vector3.zero;
        }

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

        Vector3 size = b.size;
        if (Application.isPlaying) Destroy(tmp); else DestroyImmediate(tmp);
        return size;
    }

    // Aligne le bas de la porte au sol (y=0 local du parent) après placement.
    // Utile si le pivot du prefab n’est pas au pied de la porte.
    void AlignDoorBottomToGround(GameObject door)
    {
        var rends = door.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;

        Bounds b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

        float deltaY = -b.min.y; // combien il faut remonter pour que min.y == 0 monde
        door.transform.position += new Vector3(0f, deltaY, 0f);
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
        bool hasOpening, float zMin = 0, float zMax = 0, float yMin = 0, float yMax = 0)
    {
        Quaternion rot = Quaternion.Euler(0f, 90f, 0f);

        if (!hasOpening)
        {
            CreateWallSeg(baseName,
                new Vector3(xFix, h / 2f, lenZ / 2f),
                new Vector3(lenZ, h, thick), rot);
            return;
        }

        // Gauche (en Z-)
        if (zMin > 0f)
        {
            float L = zMin;
            CreateWallSeg(baseName + "_Left",
                new Vector3(xFix, h / 2f, L / 2f),
                new Vector3(L, h, thick), rot);
        }
        // Droite (en Z+)
        if (zMax < lenZ)
        {
            float L = lenZ - zMax;
            CreateWallSeg(baseName + "_Right",
                new Vector3(xFix, h / 2f, zMax + L / 2f),
                new Vector3(L, h, thick), rot);
        }
        // Dessus
        if (yMax < h)
        {
            float H = h - yMax;
            CreateWallSeg(baseName + "_Top",
                new Vector3(xFix, yMax + H / 2f, (zMin + zMax) / 2f),
                new Vector3(zMax - zMin, H, thick), rot);
        }
        // Dessous (fenêtre)
        if (yMin > 0f)
        {
            float H = yMin;
            CreateWallSeg(baseName + "_Bottom",
                new Vector3(xFix, H / 2f, (zMin + zMax) / 2f),
                new Vector3(zMax - zMin, H, thick), rot);
        }
    }
    float ClampOpeningWidth(float wallLen, float desiredWidth, float margin)
    {
        float maxWidth = Mathf.Max(0f, wallLen - 2f * margin);
        return Mathf.Min(desiredWidth, maxWidth);
    }


    void Build()
    {
        float lX = RandomRange(minLongueurX, maxLongueurX);
        float lZ = RandomRange(minLargeurZ, maxLargeurZ);

        // Mesure de la porte avec ses rotations réelles d’installation
        // - Murs alignés X : porte tournée à 90° autour de Y dans ton code
        // - Murs alignés Z : porte non tournée (Quaternion.identity)
        Vector3 doorSizeOnX = autoFitOpeningToDoor && Porte
            ? MeasurePrefabAABBSize(Porte, transform, Quaternion.Euler(0, 90, 0), doorTargetScale)
            : new Vector3(lporte, hporte, Lporte);

        Vector3 doorSizeOnZ = autoFitOpeningToDoor && Porte
            ? MeasurePrefabAABBSize(Porte, transform, Quaternion.identity, doorTargetScale)
            : new Vector3(lporte, hporte, Lporte);

        // Épaisseurs perpendiculaires au mur selon l’orientation
        // Convertit la taille mesurée (monde) en unités locales du parent
        Vector3 parentScale = transform.lossyScale;

        // largeurs d’ouverture en unités **locales**:
        float doorWidthForX = Mathf.Max(0.001f, doorSizeOnX.x / Mathf.Max(0.0001f, parentScale.x)); // le long de X
        float doorWidthForZ = Mathf.Max(0.001f, doorSizeOnZ.z / Mathf.Max(0.0001f, parentScale.z)); // le long de Z
        float doorHeight    = Mathf.Max(0.001f, doorSizeOnX.y / Mathf.Max(0.0001f, parentScale.y)); // vertical

        // épaisseurs (perpendiculaires au mur) aussi en **local** pour l’offset:
        float doorDepthOnX_local = doorSizeOnX.z / Mathf.Max(0.0001f, parentScale.z); // mur X -> normal Z
        float doorDepthOnZ_local = doorSizeOnZ.x / Mathf.Max(0.0001f, parentScale.x); // mur Z -> normal X
        float offsetX = (Lmur - doorDepthOnX_local) * 0.5f;
        float offsetZ = (Lmur - doorDepthOnZ_local) * 0.5f;


        int murPorte;
        if (choisirMurManuellement)
        {
            murPorte = murPorteManuel;
        } else {
            murPorte = rnd.Next(0, 4); // aléatoire
        }
        float p_xMin=0, p_xMax=0, p_yMin=0, p_yMax=0; // pour murs X
        float p_zMin=0, p_zMax=0;                      // pour murs Z

        
        if (murPorte == 0 || murPorte == 1)
        {
            float widthDesired = autoFitOpeningToDoor ? doorWidthForX : lporte;
            float width = ClampOpeningWidth(lX, widthDesired, marge);
            float px = (lX - width <= 2f*marge) ? marge : RandomRange(marge, lX - (width + marge));
            p_xMin = px; p_xMax = px + width;
            p_yMin = 0f; p_yMax = Mathf.Min(hmur, doorHeight);
        }
        else
        {
            float widthDesired = autoFitOpeningToDoor ? doorWidthForZ : lporte;
            float width = ClampOpeningWidth(lZ, widthDesired, marge);
            float pz = (lZ - width <= 2f*marge) ? marge : RandomRange(marge, lZ - (width + marge));
            p_zMin = pz; p_zMax = pz + width;
            p_yMin = 0f; p_yMax = Mathf.Min(hmur, doorHeight);
        }


        // ======== FENÊTRES MULTIPLES (hauteur uniforme) ========
        bool placeFenetre = (Fenetre != null) && UnityEngine.Random.value <= probaFenetre;
        bool[] hasWindow = new bool[4];
        float[] f_xMin = new float[4], f_xMax = new float[4], f_yMin = new float[4], f_yMax = new float[4];
        float[] f_zMin = new float[4], f_zMax = new float[4];

        // faces candidates (0=bas,1=haut,2=gauche,3=droit)
        System.Collections.Generic.List<int> faces = new System.Collections.Generic.List<int>(){0,1,2,3};
        int toPick = Mathf.Clamp(nombreFenetres, 0, 4);

        // hauteur fixe (bas de la fenêtre) pour toutes les faces
        float yBottomFixed = Mathf.Clamp(hauteurBasFenetre, 0.05f, Mathf.Max(0.05f, hmur - (hfenetre + 0.5f)));
        float yTopFixed    = yBottomFixed + hfenetre;

        for (int k=0; k<toPick && faces.Count>0; k++)
        {
            int idx = rnd.Next(0, faces.Count);
            int face = faces[idx];
            faces.RemoveAt(idx);

            hasWindow[face] = placeFenetre;
            if (!hasWindow[face]) continue;

            if (face == 0 || face == 1) // murs X
            {
                // éviter chevauchement horizontal avec la porte si même face
                const int MAX_TRY = 12; int tries = 0;
                float fx;
                do {
                    fx = RandomRange(margeFenetre, lX - (lfenetre + margeFenetre));
                    tries++;
                } while (face == murPorte && !(fx + lfenetre <= p_xMin || fx >= p_xMax) && tries < MAX_TRY);

                f_xMin[face] = fx; f_xMax[face] = fx + lfenetre;
                f_yMin[face] = yBottomFixed; f_yMax[face] = yTopFixed;
            }
            else // murs Z
            {
                const int MAX_TRY = 12; int tries = 0;
                float fz;
                do {
                    fz = RandomRange(margeFenetre, lZ - (lfenetre + margeFenetre));
                    tries++;
                } while (face == murPorte && !(fz + lfenetre <= p_zMin || fz >= p_zMax) && tries < MAX_TRY);

                f_zMin[face] = fz; f_zMax[face] = fz + lfenetre;
                f_yMin[face] = yBottomFixed; f_yMax[face] = yTopFixed;
            }
        }

        // Mur BAS (z=0) – aligné X (face extérieure en +Z)
        bool holeOnBas = (murPorte == 0) || hasWindow[0];
        BuildXWallWithOpening(
            "MurBas",
            +Lmur * 0.5f,
            lX, hmur, Lmur,
            holeOnBas,
            (murPorte==0)?p_xMin:f_xMin[0],
            (murPorte==0)?p_xMax:f_xMax[0],
            (murPorte==0)?p_yMin:f_yMin[0],
            (murPorte==0)?p_yMax:f_yMax[0]
        );

        // Mur HAUT (z=lZ) – aligné X (face extérieure en -Z, mais on garde repère cohérent)
        bool holeOnHaut = (murPorte == 1) || hasWindow[1];
        BuildXWallWithOpening(
            "MurHaut",
            lZ - Lmur * 0.5f,
            lX, hmur, Lmur,
            holeOnHaut,
            (murPorte==1)?p_xMin:f_xMin[1],
            (murPorte==1)?p_xMax:f_xMax[1],
            (murPorte==1)?p_yMin:f_yMin[1],
            (murPorte==1)?p_yMax:f_yMax[1]
        );

        // Mur GAUCHE (x=0) – aligné Z (face extérieure en +X)
        bool holeOnGauche = (murPorte == 2) || hasWindow[2];
        BuildZWallWithOpening(
            "MurGauche",
            +Lmur * 0.5f,
            lZ, hmur, Lmur,
            holeOnGauche,
            (murPorte==2)?p_zMin:f_zMin[2],
            (murPorte==2)?p_zMax:f_zMax[2],
            (murPorte==2)?p_yMin:f_yMin[2],
            (murPorte==2)?p_yMax:f_yMax[2]
        );

        // Mur DROIT (x=lX) – aligné Z (face extérieure en -X)
        bool holeOnDroit = (murPorte == 3) || hasWindow[3];
        BuildZWallWithOpening(
            "MurDroit",
            lX - Lmur * 0.5f,
            lZ, hmur, Lmur,
            holeOnDroit,
            (murPorte==3)?p_zMin:f_zMin[3],
            (murPorte==3)?p_zMax:f_zMax[3],
            (murPorte==3)?p_yMin:f_yMin[3],
            (murPorte==3)?p_yMax:f_yMax[3]
        );

        // Porte affleurante à la face extérieure
        {
            var p = Instantiate(Porte, transform);
            ApplyMaterialRecursivement(p, _resDoorMat);
            p.transform.localScale = doorTargetScale;

            if (murPorte == 0) // bas (face +Z)
            {
                p.transform.localPosition = new Vector3((p_xMin+p_xMax)/2f, 0f, +Lmur*0.5f - offsetX);
                p.transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
            else if (murPorte == 1) // haut (-Z)
            {
                p.transform.localPosition = new Vector3((p_xMin+p_xMax)/2f, 0f, lZ - (Lmur*0.5f - offsetX));
                p.transform.localRotation = Quaternion.Euler(0, 90, 0);
            }
            else if (murPorte == 2) // gauche (face +X)
            {
                p.transform.localPosition = new Vector3(+Lmur*0.5f - offsetZ, 0f, (p_zMin+p_zMax)/2f);
                p.transform.localRotation = Quaternion.identity;
            }
            else // droit (-X)
            {
                p.transform.localPosition = new Vector3(lX - (Lmur*0.5f - offsetZ), 0f, (p_zMin+p_zMax)/2f);
                p.transform.localRotation = Quaternion.identity;
            }

            AlignDoorBottomToGround(p);
        }


        // Fenêtres visuelles (0..4 selon hasWindow)
        for (int face = 0; face < 4; face++)
        {
            if (!hasWindow[face]) continue;
            var f = Instantiate(Fenetre, transform);
            ApplyMaterialRecursivement(f, _resWindowMat);

            if (face == 0)
            {
                f.transform.localPosition = new Vector3((f_xMin[0]+f_xMax[0])/2f, (f_yMin[0]+f_yMax[0])/2f, +Lmur*0.5f);
                f.transform.localRotation = Quaternion.Euler(0, 90, 0); // fenêtre parallèle au mur X
                f.transform.localScale    = new Vector3(lfenetre, hfenetre, Lfenetre);
            }
            else if (face == 1)
            {
                f.transform.localPosition = new Vector3((f_xMin[1]+f_xMax[1])/2f, (f_yMin[1]+f_yMax[1])/2f, lZ - Lmur*0.5f);
                f.transform.localRotation = Quaternion.Euler(0, 90, 0); // fenêtre parallèle au mur X
                f.transform.localScale    = new Vector3(lfenetre, hfenetre, Lfenetre);
            }
            else if (face == 2)
            {
                f.transform.localPosition = new Vector3(+Lmur*0.5f, (f_yMin[2]+f_yMax[2])/2f, (f_zMin[2]+f_zMax[2])/2f);
                f.transform.localRotation = Quaternion.Euler(0, 0, 0);  // fenêtre parallèle au mur Z
                f.transform.localScale    = new Vector3(Lfenetre, hfenetre, lfenetre);
            }
            else // face == 3
            {
                f.transform.localPosition = new Vector3(lX - Lmur*0.5f, (f_yMin[3]+f_yMax[3])/2f, (f_zMin[3]+f_zMax[3])/2f);
                f.transform.localRotation = Quaternion.Euler(0, 0, 0);  // fenêtre parallèle au mur Z
                f.transform.localScale    = new Vector3(Lfenetre, hfenetre, lfenetre);
            }
        }

        // ======== TOIT (dalle simple) ========
        // Dalle posée horizontalement, centrée sur le bâtiment
        // Position Y = hmur + Lmur/2 pour qu’elle repose au-dessus des murs.
        CreateWallSeg(
            "Toit",
            new Vector3(lX / 2f, hmur + Lmur * 0.5f, lZ / 2f),
            new Vector3(lX, Lmur, lZ),
            Quaternion.identity
        );

    }

    // Petit helper Random float
    float RandomRange(float min, float max) => min + (float)rnd.NextDouble() * (max - min);
}
