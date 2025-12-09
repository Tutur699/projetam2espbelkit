using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Paramètres")]
    public int pointsPourGagnerMatch = 3;
    public float delaiEntreManches = 3f;

    [Header("Scores")]
    public int scoreJoueur = 0;
    public int scoreEnnemi = 0;

    private bool matchTermine = false;
    private string messageFin = "";
    private GUIStyle styleTexte;

    private void Awake()
    {
        if (instance == null) { 
            instance = this; 
            DontDestroyOnLoad(gameObject); 
        }
        else { 
            Destroy(gameObject); 
        }
    }

    public void JoueurGagneManche()
    {
        if (matchTermine) return;
        scoreJoueur++;
        VerifierFinDeMatch();
    }

    public void EnnemiGagneManche()
    {
        if (matchTermine) return;
        scoreEnnemi++;
        VerifierFinDeMatch();
    }

    void VerifierFinDeMatch()
    {
        if (scoreJoueur >= pointsPourGagnerMatch)
        {
            matchTermine = true;
            messageFin = "🏆 VICTOIRE DU MATCH ! 🏆";
        }
        else if (scoreEnnemi >= pointsPourGagnerMatch)
        {
            matchTermine = true;
            messageFin = "☠️ MATCH PERDU... ☠️";
        }
        else
        {
            StartCoroutine(ResetRoundSansChargement()); 
        }
    }

    public void ResetMatchTotal()
    {
        scoreJoueur = 0;
        scoreEnnemi = 0;
        matchTermine = false;
        
        PlayerManager[] tousLesJoueurs = FindObjectsOfType<PlayerManager>();
        foreach (PlayerManager joueur in tousLesJoueurs)
        {
            joueur.ResetDuJoueur();
        }
    }

    void OnGUI()
    {
        if (matchTermine)
        {
            if (styleTexte == null)
            {
                styleTexte = new GUIStyle(GUI.skin.label);
                styleTexte.fontSize = 30;
                styleTexte.alignment = TextAnchor.MiddleCenter;
                styleTexte.normal.textColor = Color.yellow;
            }

            GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 200), "RÉSULTAT FINAL");
            GUI.Label(new Rect(Screen.width / 2 - 190, Screen.height / 2 - 50, 380, 50), messageFin, styleTexte);

            if (GUI.Button(new Rect(Screen.width / 2 - 100, Screen.height / 2 + 30, 200, 50), "REJOUER LE MATCH"))
            {
                ResetMatchTotal();
            }
        }
    }

    IEnumerator ResetRoundSansChargement()
    {
        yield return new WaitForSeconds(delaiEntreManches);
        PlayerManager[] tousLesJoueurs = FindObjectsByType<PlayerManager>(FindObjectsSortMode.None);
        foreach (PlayerManager joueur in tousLesJoueurs)
        {
            joueur.ResetDuJoueur();
        }
        SC_EnemySpawner spawner = FindFirstObjectByType<SC_EnemySpawner>();
        if (spawner != null)
        {
            spawner.ResetRound();
        }
    }
}