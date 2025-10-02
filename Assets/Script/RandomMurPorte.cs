using UnityEngine;
using System;

public class RandomMurPorte : MonoBehaviour
{
    //X ->, Y ^, Z <-
    [Header("Prefabs")]
    public GameObject Mur;
    public GameObject Porte; // porte

    [Header("Mur")]
    public float hmur = 5f;     // hauteur
    public float Lmur = 0.5f;   // épaisseur mur

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Build();
    }

    float MyRandom(System.Random r, float min, float max)
    {
        return min + (float)r.NextDouble() * (max - min);
    }

    void Build()
    {
        float lX = MyRandom(rnd, minLongueurX, maxLongueurX);
        float lZ = MyRandom(rnd, minLargeurZ, maxLargeurZ);
        var murbas = Instantiate(Mur, transform);
        murbas.name = "MurBas";
        murbas.transform.localScale = new Vector3(lX, hmur, Lmur);
        murbas.transform.localPosition = new Vector3(lX / 2f, hmur / 2f, 0f);
        var murgauche = Instantiate(Mur, transform);
        murgauche.name = "MurGauche";
        murgauche.transform.localScale = new Vector3(lZ, hmur, Lmur);
        murgauche.transform.localPosition = new Vector3(0f, hmur / 2f, lZ / 2f);
        murgauche.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        var murhaut = Instantiate(Mur, transform);
        murhaut.name = "MurHaut";
        murhaut.transform.localScale = new Vector3(lX, hmur, Lmur);
        murhaut.transform.localPosition = new Vector3(lX / 2f, hmur / 2f, lZ);
        var murdroit = Instantiate(Mur, transform);
        murdroit.name = "MurDroit";
        murdroit.transform.localScale = new Vector3(lZ, hmur, Lmur);
        murdroit.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
        murdroit.transform.localPosition = new Vector3(lX, hmur / 2f, lZ / 2f);


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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
