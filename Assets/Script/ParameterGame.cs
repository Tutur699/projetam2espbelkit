using UnityEngine;

public class ParameterGame : MonoBehaviour
{
    public GameObject panelParametre;
    public GameObject[] objetsAJeterQuandParam;   // les boutons du menu principal

    void Start()
    {
        // menu paramètres caché au début
        if (panelParametre != null)
            panelParametre.SetActive(false);
    }

    public void OpenParameters()
    {
        if (panelParametre != null)
            panelParametre.SetActive(true);

        foreach (GameObject go in objetsAJeterQuandParam)
        {
            if (go != null) go.SetActive(false);
        }
    }

    public void CloseParameters()
    {
        if (panelParametre != null)
            panelParametre.SetActive(false);

        foreach (GameObject go in objetsAJeterQuandParam)
        {
            if (go != null) go.SetActive(true);
        }
    }
}
