using UnityEngine;

public class RandomMurPorte : MonoBehaviour
{
    public GameObject Mur;
    public GameObject Porte;

    //tailles car objects simples
    public float lmur = 20f;
    public float Lmur = 0.5f;
    public float hmur = 5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Build();
    }

    void Build()
    {
        var murcreer = Instantiate(Mur, transform); //transform => devient enfant de notre script mis sur un GameObject
        murcreer.name = "MurTest";
        murcreer.transform.localScale = new Vector3(lmur, hmur, Lmur);
        murcreer.transform.localPosition = new Vector3(lmur / 2f, hmur / 2f, 0f);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
