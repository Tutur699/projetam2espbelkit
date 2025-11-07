using UnityEngine;
using System;
using Unity.Netcode;

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

    [Header("Mur")]
    public float hmur = 5f;       // hauteur des murs
    public float Lmur = 0.5f;     // épaisseur des murs
    public string choixWall = "";

    [Header("Tirage aléatoire des dimensions au sol")]
    public float minLongueurX = 10f;
    public float maxLongueurX = 30f;
    public float minLargeurZ  = 8f;
    public float maxLargeurZ  = 20f;
    System.Random _rng;

    [Header("Porte – calibration auto")]
    public Vector3 doorTargetScale = new Vector3(1.5f, 1.5f, 1.5f); // remplace ton 1.5f hardcodé
    public bool autoFitOpeningToDoor = true; // si true, on taille l’ouverture sur la vraie porte
    [Tooltip("Épaisseur visuelle de la porte en proportion de l'épaisseur du mur (0.1 = 10%)")]
    [Range(0.05f, 1f)] public float doorThicknessRatio = 0.25f;

    

    [Header("Dimensions porte")]
    public float lporte = 7f;     // largeur ouverture
    public float hporte = 3f;     // hauteur ouverture
    public float Lporte = 0.3f;   // épaisseur du visuel porte
    public float marge  = 0.5f;   // marge latérale / verticale

    [Header("Toit - pignon (triangle)")]
    [Tooltip("Active le toit en triangle (pignon) au lieu de la dalle plate.")]
    public bool useGableRoof = true;

    [Tooltip("Hauteur supplémentaire entre le haut des murs et le faîtage.")]
    public float roofHeight = 1.2f;

    [Tooltip("Débord du toit en X (pignons) et en Z (égouts).")]
    public float roofOverhang = 0.4f;

    [Tooltip("Épaisseur visuelle de chaque pan de toit.")]
    public float roofThickness = 0.2f;

    [Tooltip("Matériau du toit (si nul, matérial mur par défaut).")]
    public Material materialToit;

    public bool loadRoofFromResources = false;
    public string resourcesRoofPath = "WALL/MyRoof";
    [Header("Pièces intérieures (BSP)")]
    public bool generateRooms = true;

    [Tooltip("Nombre de pièces à créer à l'intérieur du volume")]
    [Range(1, 24)] public int nombrePieces = 6;

    [Tooltip("Taille minimale (X) d'une pièce")]
    public float minPieceX = 4f;

    [Tooltip("Taille minimale (Z) d'une pièce")]
    public float minPieceZ = 4f;

    [Header("Ouvertures entre pièces")]
    [Tooltip("Largeur d'ouverture entre pièces (sera clampée si mur trop court)")]
    public float largeurOuvertureInterne = 1.6f;

    [Tooltip("Hauteur d'ouverture entre pièces")]
    public float hauteurOuvertureInterne = 2.2f;

    [Tooltip("Marge de chaque côté de l'ouverture à l'intérieur de la cloison")]
    public float margeOuvertureInterne = 0.4f;
    // Enregistre les partitions pour les contrôles d'intersection
    System.Collections.Generic.List<Partition> _partsForValidation = new System.Collections.Generic.List<Partition>();

    [Tooltip("Distance de sécurité autour d'une jonction mur/cloison où on interdit les ouvertures en façade")]
    public float margeIntersection = 0.6f;
    [Header("Sol & Soubassement")]
    public bool creerSol = true;
    public float epaisseurSol = 0.25f;
    public Material materialSol;

    public bool creerSoubassement = true;
    [Tooltip("Hauteur supplémentaire sous le bâtiment")]
    public float hauteurSoubassement = 1.0f;



    
    private Material _resWallMat;
    private Material _resDoorMat;
    private Material _resWindowMat;
    private Material _resRoofMat;

    void Start()
    {
        _resWallMat   = ResolveWallMaterial();
        _resDoorMat   = ResolveDoorMaterial();
        _resWindowMat = ResolveWindowMaterial();
        _resRoofMat   = ResolveRoofMaterial();
    }

    // Appelé par GameWorldSeed quand le seed est connu
    public void Generate(int seed)
    {
        _rng = new System.Random(seed ^ StableHash(GetHierarchyPath(transform)));// sel local pour varier par objet si tu veux
        // Option : force la même qualité partout
        QualitySettings.SetQualityLevel(2, true);

        Build(); // ta méthode existante
    }
    string GetHierarchyPath(Transform t)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        while (t != null)
        {
            sb.Insert(0, "/" + t.name + "#" + t.GetSiblingIndex());
            t = t.parent;
        }
        return sb.ToString();
    }

    int StableHash(string s)
    {
        unchecked
        {
            int h = 23;
            for (int i = 0; i < s.Length; i++)
                h = h * 31 + s[i];
            return h;
        }
    }


    // Utilitaires déterministes
    float RandomRange(float min, float max) => min + (float)_rng.NextDouble() * (max - min);
    int   RandomInt(int min, int max)       => _rng.Next(min, max); // max exclu
    bool  RandomBool(float proba=0.5f)      => _rng.NextDouble() < proba;

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

    Material ResolveRoofMaterial()
    {
        Material mat = materialToit;
        if (loadRoofFromResources)
        {
            mat = Resources.Load<Material>(resourcesRoofPath);
            if (mat == null) Debug.LogError("[RandomMurPorte] Matériau toit introuvable: " + resourcesRoofPath);
        }
        return mat != null ? mat : _resWallMat;
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

    // Aligne le bas de la porte sur un Y local donné du parent (par défaut 0)
    // sans changer l'altitude globale du parent.
    void AlignDoorBottomToParentGround(GameObject door, Transform parent, float localGroundY = 0f)
    {
        var rends = door.GetComponentsInChildren<Renderer>(true);
        if (rends.Length == 0) return;

        Bounds b = rends[0].bounds; // bounds en ESPACE MONDE
        for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);

        // Y monde de la "référence sol" du parent (Y local = localGroundY)
        float targetWorldY = parent.TransformPoint(new Vector3(0f, localGroundY, 0f)).y;

        // Décalage nécessaire pour que le bas de la porte touche ce plan
        float deltaY = targetWorldY - b.min.y;
        if (Mathf.Abs(deltaY) > 1e-5f)
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

    // Spécifique toit : applique le matériau toit et laisse le scale tel quel
    GameObject CreateRoofSeg(string name, Vector3 center, Vector3 size, Quaternion rot)
    {
        var seg = Instantiate(Mur, transform);
        seg.name = name;
        seg.transform.localRotation = rot;
        seg.transform.localPosition = center;
        seg.transform.localScale = size;
        ApplyMaterialRecursivement(seg, _resRoofMat);
        return seg;
    }
    // Sol: une dalle pleine sous le bâtiment
    GameObject CreateFloor(string name, float lX, float lZ, float ep)
    {
        var floor = Instantiate(Mur, transform);
        floor.name = name;
        floor.transform.localRotation = Quaternion.identity;
        floor.transform.localPosition = new Vector3(lX * 0.5f, ep * 0.5f, lZ * 0.5f);
        floor.transform.localScale = new Vector3(lX, ep, lZ);

        ApplyMaterialRecursivement(floor, materialSol ? materialSol : _resWallMat);
        if (!floor.TryGetComponent<Collider>(out _)) floor.AddComponent<BoxCollider>();
        return floor;
    }

    // Raycast (ou Terrain) pour connaître le Y du sol monde sous un point
    float GetGroundYUnder(Vector3 worldPos)
    {
        // 1) Raycast
        var origin = worldPos + Vector3.up * 2000f;
        if (Physics.Raycast(origin, Vector3.down, out var hit, 4000f))
            return hit.point.y;

        // 2) Terrain Unity (fallback)
        if (Terrain.activeTerrain)
            return Terrain.activeTerrain.SampleHeight(worldPos) + Terrain.activeTerrain.transform.position.y;

        // 3) défaut
        return 0f;
    }

    // 4 murs minces qui descendent sous le bâtiment
    void CreateSkirtWalls(float lX, float lZ, float heightDown)
    {
        // on descend vers Y négatif en local
        float yMid = -heightDown * 0.5f;

        // bas (mur // X)
        CreateWallSeg("Soub_Bas",
            new Vector3(lX * 0.5f, yMid, +Lmur * 0.5f),
            new Vector3(lX, heightDown, Lmur),
            Quaternion.identity);

        // haut (mur // X)
        CreateWallSeg("Soub_Haut",
            new Vector3(lX * 0.5f, yMid, lZ - Lmur * 0.5f),
            new Vector3(lX, heightDown, Lmur),
            Quaternion.identity);

        // gauche (mur // Z)
        CreateWallSeg("Soub_Gauche",
            new Vector3(+Lmur * 0.5f, yMid, lZ * 0.5f),
            new Vector3(lZ, heightDown, Lmur),
            Quaternion.Euler(0f, 90f, 0f));

        // droit (mur // Z)
        CreateWallSeg("Soub_Droit",
            new Vector3(lX - Lmur * 0.5f, yMid, lZ * 0.5f),
            new Vector3(lZ, heightDown, Lmur),
            Quaternion.Euler(0f, 90f, 0f));
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
    void BuildXWallWithOpeningOffset(
        string baseName, float xOrigin, float zFix, float lenX, float h, float thick,
        bool hasOpening, float xMin=0, float xMax=0, float yMin=0, float yMax=0)
    {
        Quaternion rot = Quaternion.identity;

        if (!hasOpening)
        {
            CreateWallSeg(baseName, new Vector3(xOrigin + lenX/2f, h/2f, zFix), new Vector3(lenX, h, thick), rot);
            return;
        }

        // Gauche
        if (xMin > 0f)
        {
            float L = xMin;
            CreateWallSeg(baseName+"_Left",
                new Vector3(xOrigin + L/2f, h/2f, zFix),
                new Vector3(L, h, thick), rot);
        }
        // Droite
        if (xMax < lenX)
        {
            float L = lenX - xMax;
            CreateWallSeg(baseName+"_Right",
                new Vector3(xOrigin + xMax + L/2f, h/2f, zFix),
                new Vector3(L, h, thick), rot);
        }
        // Dessus
        if (yMax < h)
        {
            float H = h - yMax;
            CreateWallSeg(baseName+"_Top",
                new Vector3(xOrigin + (xMin+xMax)/2f, yMax + H/2f, zFix),
                new Vector3(xMax - xMin, H, thick), rot);
        }
        // Dessous
        if (yMin > 0f)
        {
            float H = yMin;
            CreateWallSeg(baseName+"_Bottom",
                new Vector3(xOrigin + (xMin+xMax)/2f, H/2f, zFix),
                new Vector3(xMax - xMin, H, thick), rot);
        }
    }

    // Variante de mur aligné Z avec "origine" en Z
    void BuildZWallWithOpeningOffset(
        string baseName, float xFix, float zOrigin, float lenZ, float h, float thick,
        bool hasOpening, float zMin=0, float zMax=0, float yMin=0, float yMax=0)
    {
        Quaternion rot = Quaternion.Euler(0f, 90f, 0f);

        if (!hasOpening)
        {
            CreateWallSeg(baseName,
                new Vector3(xFix, h/2f, zOrigin + lenZ/2f),
                new Vector3(lenZ, h, thick), rot);
            return;
        }

        // Gauche (Z-)
        if (zMin > 0f)
        {
            float L = zMin;
            CreateWallSeg(baseName+"_Left",
                new Vector3(xFix, h/2f, zOrigin + L/2f),
                new Vector3(L, h, thick), rot);
        }
        // Droite (Z+)
        if (zMax < lenZ)
        {
            float L = lenZ - zMax;
            CreateWallSeg(baseName+"_Right",
                new Vector3(xFix, h/2f, zOrigin + zMax + L/2f),
                new Vector3(L, h, thick), rot);
        }
        // Dessus
        if (yMax < h)
        {
            float H = h - yMax;
            CreateWallSeg(baseName+"_Top",
                new Vector3(xFix, yMax + H/2f, zOrigin + (zMin+zMax)/2f),
                new Vector3(zMax - zMin, H, thick), rot);
        }
        // Dessous
        if (yMin > 0f)
        {
            float H = yMin;
            CreateWallSeg(baseName+"_Bottom",
                new Vector3(xFix, H/2f, zOrigin + (zMin+zMax)/2f),
                new Vector3(zMax - zMin, H, thick), rot);
        }
    }

    // ===================== BSP : DECOUPE EN PIECES + OUVERTURES =====================
    struct Cell
    {
        public float x0, x1, z0, z1;
        public float W => x1 - x0;
        public float D => z1 - z0;
        public float Area => W * D;
        public Cell(float X0, float X1, float Z0, float Z1){ x0=X0; x1=X1; z0=Z0; z1=Z1; }
    }

    struct Partition // une cloison à poser + ouverture
    {
        public bool vertical;     // true: mur // Z (x=const), false: mur // X (z=const)
        public float pos;         // x (si vertical) ou z (si horizontal)
        public float start,end;   // intervalle [start..end] le long de la cloison (Z si vertical, X si horizontal)
    }
    struct Interval { public float a, b; public Interval(float A, float B){ a=A; b=B; } }

    // Construit la liste des intervalles "interdits" sur la façade donnée, à partir des cloisons qui y arrivent.
    System.Collections.Generic.List<Interval> GetForbiddenIntervalsForFace(int face, float lX, float lZ, float pad)
    {
        var res = new System.Collections.Generic.List<Interval>();
        const float EPS = 1e-3f;

        // 0=bas(z=0) et 1=haut(z=lZ) -> coordonnée le long de X (0..lX)
        if (face == 0 || face == 1)
        {
            foreach (var p in _partsForValidation)
            {
                if (!p.vertical) continue; // il faut une cloison verticale (x=const) pour toucher ces façades
                bool toucheBas  = Mathf.Abs(p.start - 0f)   < EPS;
                bool toucheHaut = Mathf.Abs(p.end   - lZ)   < EPS;
                if ((face==0 && toucheBas) || (face==1 && toucheHaut))
                {
                    float a = Mathf.Max(0f, p.pos - pad);
                    float b = Mathf.Min(lX, p.pos + pad);
                    if (b > a) res.Add(new Interval(a,b));
                }
            }
        }
        // 2=gauche(x=0) et 3=droit(x=lX) -> coordonnée le long de Z (0..lZ)
        else
        {
            foreach (var p in _partsForValidation)
            {
                if (p.vertical) continue; // il faut une cloison horizontale (z=const) pour toucher ces façades
                bool toucheGauche = Mathf.Abs(p.start - 0f)  < EPS;
                bool toucheDroit  = Mathf.Abs(p.end   - lX)  < EPS;
                if ((face==2 && toucheGauche) || (face==3 && toucheDroit))
                {
                    float a = Mathf.Max(0f, p.pos - pad);
                    float b = Mathf.Min(lZ, p.pos + pad);
                    if (b > a) res.Add(new Interval(a,b));
                }
            }
        }

        // (Optionnel) fusionne les intervalles qui se chevauchent
        res.Sort((u,v) => u.a.CompareTo(v.a));
        var merged = new System.Collections.Generic.List<Interval>();
        foreach (var iv in res)
        {
            if (merged.Count==0 || iv.a > merged[merged.Count-1].b)
                merged.Add(iv);
            else
                merged[merged.Count-1] = new Interval(merged[merged.Count-1].a, Mathf.Max(merged[merged.Count-1].b, iv.b));
        }
        return merged;
    }

    // Vérifie si [a,b] est entièrement à l’extérieur de tous les intervalles interdits
    bool IsClear(System.Collections.Generic.List<Interval> forb, float a, float b)
    {
        for (int i=0;i<forb.Count;i++)
        {
            var iv = forb[i];
            if (!(b <= iv.a || a >= iv.b)) return false; // recouvrement
        }
        return true;
    }

    // Tente de tirer aléatoirement une position centrée qui évite les interdits ; sinon renvoie une position "greffée" au plus grand trou.
    float PickOpeningStart(float len, float openW, float margin, System.Collections.Generic.List<Interval> forb)
    {
        float min = margin;
        float max = len - (openW + margin);
        if (max < min) return margin; // pas de place, on fera avec…

        const int MAX_TRY = 24;
        for (int t=0;t<MAX_TRY;t++)
        {
            float x = RandomRange(min, max);
            float a = x;
            float b = x + openW;
            if (IsClear(forb, a, b)) return x;
        }

        // Fallback : scanne linéairement pour trouver le premier créneau libre
        float step = Mathf.Max(0.05f, openW * 0.1f);
        for (float x=min; x<=max; x+=step)
        {
            float a = x;
            float b = x + openW;
            if (IsClear(forb, a, b)) return x;
        }

        // Dernier recours : colle l’ouverture juste après le plus gros interdit (ou au margin)
        if (forb.Count==0) return min;
        float bestEnd = forb[0].b;
        for (int i=1;i<forb.Count;i++) if (forb[i].b > bestEnd) bestEnd = forb[i].b;
        float candidate = Mathf.Clamp(bestEnd + margin, min, max);
        return candidate;
    }

    void BuildRoomsBSP(float lX, float lZ)
    {
        _partsForValidation.Clear();
        // === Mesures du prefab de PORTE pour dimensionner les OUVERTURES internes ===
        Vector3 parentScale = transform.lossyScale;
        bool hasDoor = Porte != null && autoFitOpeningToDoor;

        Vector3 doorSzZ_world = hasDoor
            ? MeasurePrefabAABBSize(Porte, transform, Quaternion.identity, doorTargetScale)
            : new Vector3(Lporte, hporte, lporte);

        Vector3 doorSzX_world = hasDoor
            ? MeasurePrefabAABBSize(Porte, transform, Quaternion.Euler(0f, 90f, 0f), doorTargetScale)
            : new Vector3(lporte, hporte, Lporte);

        // Conversion monde -> local
        float doorHeightLocal = Mathf.Max(0.001f, (hasDoor ? doorSzZ_world.y : hporte) / Mathf.Max(0.0001f, parentScale.y));
        float doorWidthForZ   = Mathf.Max(0.001f, (hasDoor ? doorSzZ_world.z : lporte) / Mathf.Max(0.0001f, parentScale.z)); // murs // Z
        float doorWidthForX   = Mathf.Max(0.001f, (hasDoor ? doorSzX_world.x : lporte) / Mathf.Max(0.0001f, parentScale.x)); // murs // X
        if (!generateRooms || nombrePieces <= 1) return;

        // 1) Génère N cellules par BSP
        var cells = new System.Collections.Generic.List<Cell>();
        var parts = new System.Collections.Generic.List<Partition>();
        cells.Add(new Cell(0f, lX, 0f, lZ));

        int safety = 0;
        while (cells.Count < nombrePieces && safety < 200)
        {
            safety++;
            // prend la plus grande cellule
            int idx = 0;
            float bestA = cells[0].Area;
            for (int i = 1; i < cells.Count; i++)
                if (cells[i].Area > bestA) { bestA = cells[i].Area; idx = i; }

            var c = cells[idx];

            bool canV = c.W >= 2f * Mathf.Max(0.1f, minPieceX);
            bool canH = c.D >= 2f * Mathf.Max(0.1f, minPieceZ);
            if (!canV && !canH) break; // plus rien à couper

            bool verticalSplit = (canV && canH) ? RandomBool(0.5f) : canV;

            if (verticalSplit)
            {
                float minS = c.x0 + minPieceX;
                float maxS = c.x1 - minPieceX;
                if (maxS <= minS) { continue; }
                float s = RandomRange(minS, maxS);

                var a = new Cell(c.x0, s,   c.z0, c.z1);
                var b = new Cell(s,   c.x1, c.z0, c.z1);
                cells[idx] = a;
                cells.Add(b);

                parts.Add(new Partition { vertical = true, pos = s, start = c.z0, end = c.z1 });
            }
            else
            {
                float minS = c.z0 + minPieceZ;
                float maxS = c.z1 - minPieceZ;
                if (maxS <= minS) { continue; }
                float s = RandomRange(minS, maxS);

                var a = new Cell(c.x0, c.x1, c.z0, s);
                var b = new Cell(c.x0, c.x1, s,   c.z1);
                cells[idx] = a;
                cells.Add(b);

                parts.Add(new Partition { vertical = false, pos = s, start = c.x0, end = c.x1 });
            }
        }

        // 2) Pose les cloisons + ouvre une porte dans CHAQUE cloison (+ instancie la porte visuelle)
        for (int i = 0; i < parts.Count; i++)
        {
            var p = parts[i];
            float len = p.end - p.start;
            if (len <= 0.05f) continue;

            
            // Dimensions d’ouverture CIBLE issues du prefab (selon orientation)
            float targetW = p.vertical ? doorWidthForZ : doorWidthForX;
            float targetH = doorHeightLocal;

            // Clamp par la longueur dispo et la hauteur de mur / réglages
            float openW = Mathf.Clamp(targetW, 0.5f, Mathf.Max(0.5f, len - 2f * margeOuvertureInterne));
            float openH = Mathf.Clamp(targetH, 0.5f, Mathf.Min(hmur, hauteurOuvertureInterne));

            // Positionner l’ouverture au hasard en respectant la marge
            float center = RandomRange(p.start + margeOuvertureInterne + openW * 0.5f,
                                    p.end   - margeOuvertureInterne - openW * 0.5f);
            float minRel = Mathf.Clamp(center - openW * 0.5f - p.start, 0f, len);
            float maxRel = Mathf.Clamp(center + openW * 0.5f - p.start, 0f, len);

            if (p.vertical)
            {
                // Ouverture dans le mur
                BuildZWallWithOpeningOffset("CloisonV_" + i, p.pos, p.start, len, hmur, Lmur, true, minRel, maxRel, 0f, openH);

                // Instancier la porte au centre de l’ouverture
                float zCenter = p.start + (minRel + maxRel) * 0.5f;
                var d = Instantiate(Porte, transform);
                ApplyMaterialRecursivement(d, _resDoorMat);
                d.transform.localRotation = Quaternion.identity;          // porte // Z
                d.transform.localPosition = new Vector3(p.pos, 0f, zCenter);
                FitDoorToOpening(d, d.transform.localRotation, openW, openH, Lmur);
                AlignDoorBottomToParentGround(d, transform);

            }
            else
            {
                BuildXWallWithOpeningOffset("CloisonH_" + i, p.start, p.pos, len, hmur, Lmur, true, minRel, maxRel, 0f, openH);
                float xCenter = p.start + (minRel + maxRel) * 0.5f;
                var d = Instantiate(Porte, transform);
                ApplyMaterialRecursivement(d, _resDoorMat);
                d.transform.localRotation = Quaternion.Euler(0f, 90f, 0f); // porte // X
                d.transform.localPosition = new Vector3(xCenter, 0f, p.pos);
                FitDoorToOpening(d, d.transform.localRotation, openW, openH, Lmur);
                AlignDoorBottomToParentGround(d, transform);

            }
        }

        _partsForValidation = parts;
    }

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
        BuildRoomsBSP(lX, lZ);
        // sol
        if (creerSol)
            CreateFloor("Sol", lX, lZ, epaisseurSol);

        // soubassement
        if (creerSoubassement)
        {
            // Y monde du bas du bâtiment (local y=0)
            float baseWorldY = transform.TransformPoint(Vector3.zero).y;

            // Sol monde sous le centre
            Vector3 centerWorld = transform.TransformPoint(new Vector3(lX * 0.5f, 0f, lZ * 0.5f));
            float groundY = GetGroundYUnder(centerWorld);

            // Espace à combler + marge voulue
            float gap = Mathf.Max(0f, baseWorldY - groundY);
            float skirtHeight = gap + Mathf.Max(0.05f, hauteurSoubassement);

            if (skirtHeight > 0.01f)
                CreateSkirtWalls(lX, lZ, skirtHeight);
        }


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
            murPorte = RandomInt(0, 4); // aléatoire
        }
        float p_xMin=0, p_xMax=0, p_yMin=0, p_yMax=0; // pour murs X
        float p_zMin=0, p_zMax=0;                      // pour murs Z

        
        if (murPorte == 0 || murPorte == 1)
        {
            float widthDesired = autoFitOpeningToDoor ? doorWidthForX : lporte;
            float width = ClampOpeningWidth(lX, widthDesired, marge);

            var forb = GetForbiddenIntervalsForFace(murPorte, lX, lZ, margeIntersection);
            float px = PickOpeningStart(lX, width, marge, forb);

            p_xMin = px; p_xMax = px + width;
            p_yMin = 0f; p_yMax = Mathf.Min(hmur, doorHeight);
        }
        else
        {
            float widthDesired = autoFitOpeningToDoor ? doorWidthForZ : lporte;
            float width = ClampOpeningWidth(lZ, widthDesired, marge);

            // Utilise les intervalles interdits le long de Z
            var forb = GetForbiddenIntervalsForFace(murPorte, lX, lZ, margeIntersection);
            float pz = PickOpeningStart(lZ, width, marge, forb);

            p_zMin = pz; p_zMax = pz + width;
            p_yMin = 0f; p_yMax = Mathf.Min(hmur, doorHeight);
        }


        // ======== FENÊTRES MULTIPLES (hauteur uniforme) ========
        bool placeFenetre = (Fenetre != null) && RandomBool(probaFenetre);
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
            int idx = RandomInt(0, faces.Count);
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

            AlignDoorBottomToParentGround(p, transform);
        }

        // Fenêtres visuelles (0..4 selon hasWindow)
        for (int face = 0; face < 4; face++)
        {
            if (!hasWindow[face]) continue;
            var f = Instantiate(Fenetre, transform);
            ApplyMaterialRecursivement(f, _resWindowMat);

            if (face == 0)
            {
                f.transform.localPosition = new Vector3((f_xMin[0] + f_xMax[0]) / 2f, (f_yMin[0] + f_yMax[0]) / 2f, +Lmur * 0.5f);
                f.transform.localRotation = Quaternion.Euler(0, 0, 0); // fenêtre parallèle au mur X
                f.transform.localScale = new Vector3(lfenetre, hfenetre, Lfenetre);
            }
            else if (face == 1)
            {
                f.transform.localPosition = new Vector3((f_xMin[1] + f_xMax[1]) / 2f, (f_yMin[1] + f_yMax[1]) / 2f, lZ - Lmur * 0.5f);
                f.transform.localRotation = Quaternion.Euler(0, 0, 0); // fenêtre parallèle au mur X
                f.transform.localScale = new Vector3(lfenetre, hfenetre, Lfenetre);
            }
            else if (face == 2)
            {
                f.transform.localPosition = new Vector3(+Lmur * 0.5f, (f_yMin[2] + f_yMax[2]) / 2f, (f_zMin[2] + f_zMax[2]) / 2f);
                f.transform.localRotation = Quaternion.Euler(0, 0, 0);  // fenêtre parallèle au mur Z
                f.transform.localScale = new Vector3(Lfenetre, hfenetre, lfenetre);
            }
            else // face == 3
            {
                f.transform.localPosition = new Vector3(lX - Lmur * 0.5f, (f_yMin[3] + f_yMax[3]) / 2f, (f_zMin[3] + f_zMax[3]) / 2f);
                f.transform.localRotation = Quaternion.Euler(0, 0, 0);  // fenêtre parallèle au mur Z
                f.transform.localScale = new Vector3(Lfenetre, hfenetre, lfenetre);
            }
        }
        
        // soit toit simple soit en pignon
        if (useGableRoof)
        {
            BuildGableRoof(lX, lZ);
            BuildGableEnds(lX, lZ);   // <= AJOUT : ferme les deux côtés
            
        }
        else
        {
            // Dalle plate (ancienne version)
            CreateWallSeg(
                "Toit",
                new Vector3(lX / 2f, hmur + Lmur * 0.5f, lZ / 2f),
                new Vector3(lX, Lmur, lZ),
                Quaternion.identity
            );
        }
    }

    
    GameObject CreateTriPrism(string name, float widthX, float baseY, float apexY, float thicknessZ, float z0, Material mat)
    {
        // Triangle en plan X-Y (0,baseY)-(widthX,baseY)-(widthX/2,apexY), extrudé en Z sur [0..thicknessZ]
        Vector3[] v = new Vector3[6];
        v[0] = new Vector3(0f,        baseY, 0f);
        v[1] = new Vector3(widthX,    baseY, 0f);
        v[2] = new Vector3(widthX*0.5f, apexY, 0f);
        v[3] = new Vector3(0f,        baseY, thicknessZ);
        v[4] = new Vector3(widthX,    baseY, thicknessZ);
        v[5] = new Vector3(widthX*0.5f, apexY, thicknessZ);

        int[] t = new int[]
        {
            // face avant (z=0)
            0,2,1,
            // face arrière (z=thickness)
            3,4,5,

            // côté bas (rectangle coupé en 2)
            0,1,4,
            0,4,3,

            // côté gauche (rectangle coupé en 2)
            0,3,5,
            0,5,2,

            // côté droit (rectangle coupé en 2)
            1,2,5,
            1,5,4
        };

        var go = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        go.transform.SetParent(transform, false);

        var mesh = new Mesh();
        mesh.name = name + "_Mesh";
        mesh.vertices = v;
        mesh.triangles = t;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        go.GetComponent<MeshFilter>().mesh = mesh;
        var mr = go.GetComponent<MeshRenderer>();
        mr.sharedMaterial = mat != null ? mat : _resWallMat;

        // Positionne le prisme sur la façade voulue (en local)
        go.transform.localPosition = new Vector3(0f, 0f, z0);

        return go;
    }

    // Construit les 2 pignons (façades bas et haut)
    void BuildGableEnds(float lX, float lZ)
    {
        float baseY = hmur;                 // haut du mur
        float apexY = hmur + roofHeight;    // faîtage
        float thick = Lmur;                 // on remplit l’épaisseur du mur

        // Façade BAS (z = 0)
        CreateTriPrism("Pignon_Bas", lX, baseY, apexY, thick, 0f, _resWallMat);

        // Façade HAUT (z = lZ - Lmur), on colle au mur du fond
        CreateTriPrism("Pignon_Haut", lX, baseY, apexY, thick, lZ - Lmur, _resWallMat);
    }
    void FitDoorToOpening(GameObject doorPrefabInstance, Quaternion rot, float openWidth, float openHeight, float wallThickness)
    {
        // Mesure la taille AABB du prefab à scale=(1,1,1) avec la rotation voulue
        Vector3 baseSize = MeasurePrefabAABBSize(Porte, transform, rot, Vector3.one);
        if (baseSize.x < 1e-4f || baseSize.y < 1e-4f || baseSize.z < 1e-4f)
            return;

        // Détermine si la porte est alignée "mur // Z" (rotation ~ identity)
        bool isZAligned = Mathf.Abs(Quaternion.Angle(rot, Quaternion.identity)) < 0.1f;

        // Axes interprétés:
        // - isZAligned == true  -> largeur d'ouverture sur Z, profondeur du mesh sur X
        // - isZAligned == false -> largeur d'ouverture sur X, profondeur du mesh sur Z
        float meshWidth  = isZAligned ? baseSize.z : baseSize.x;
        float meshHeight = baseSize.y;
        float meshDepth  = isZAligned ? baseSize.x : baseSize.z;

        // Épaisseur visuelle cible de la porte (fraction de l'épaisseur du mur)
        // Ajoute dans ta classe : [Range(0.05f,1f)] public float doorThicknessRatio = 0.25f;
        float targetDepth = Mathf.Max(0.01f, Mathf.Min(wallThickness * doorThicknessRatio, wallThickness));

        // Facteurs d'échelle pour remplir l'ouverture (largeur/hauteur) sans "gonfler" toute l'épaisseur du mur
        float sx, sy, sz;
        if (isZAligned)
        {
            // Largeur sur Z, profondeur sur X
            sx = targetDepth / Mathf.Max(1e-4f, meshDepth);  // épaisseur de la porte (petite)
            sy = openHeight / Mathf.Max(1e-4f, meshHeight);  // hauteur
            sz = openWidth  / Mathf.Max(1e-4f, meshWidth);   // largeur
        }
        else
        {
            // Largeur sur X, profondeur sur Z
            sx = openWidth  / Mathf.Max(1e-4f, meshWidth);   // largeur
            sy = openHeight / Mathf.Max(1e-4f, meshHeight);  // hauteur
            sz = targetDepth/ Mathf.Max(1e-4f, meshDepth);   // épaisseur de la porte (petite)
        }

        doorPrefabInstance.transform.localScale = new Vector3(sx, sy, sz);
    }



    void BuildGableRoof(float lX, float lZ)
    {
        // demi-portée + débord
        float halfSpanXWithOverhang = lX * 0.5f + roofOverhang;
        float widthZWithOverhang    = lZ + 2f * roofOverhang;

        // longueur de pente (hypoténuse) et angle
        float hyp = Mathf.Sqrt(halfSpanXWithOverhang * halfSpanXWithOverhang + roofHeight * roofHeight);
        float thetaDeg = Mathf.Atan2(roofHeight, halfSpanXWithOverhang) * Mathf.Rad2Deg;

        float yTopWall = hmur + Mathf.Max(0.01f, roofThickness * 0.5f);


        // Point du faîtage (au centre)
        Vector3 ridge = new Vector3(lX * 0.5f, yTopWall + roofHeight, lZ * 0.5f);

    
        // Pan gauche : eave côté x = -overhang -> ridge
        Vector3 eaveLeft = new Vector3(-roofOverhang, yTopWall, lZ * 0.5f);
        Vector3 centerLeft = (eaveLeft + ridge) * 0.5f;

        // Pan droit : eave côté x = lX + overhang -> ridge
        Vector3 eaveRight = new Vector3(lX + roofOverhang, yTopWall, lZ * 0.5f);
        Vector3 centerRight = (eaveRight + ridge) * 0.5f;

        // Taille des pans : (longueur le long de la pente, épaisseur, largeur en Z)
        Vector3 size = new Vector3(hyp, roofThickness, widthZWithOverhang);

        // Création des deux pans
        var left = CreateRoofSeg("Toit_PanGauche", centerLeft, size, Quaternion.Euler(0f, 0f,  thetaDeg));
        var right= CreateRoofSeg("Toit_PanDroit",  centerRight, size, Quaternion.Euler(0f, 0f, -thetaDeg));

        // Tilingdumaterial(left, 0.5f, 0.5f);
        // Tilingdumaterial(right, 0.5f, 0.5f);
    }

}
