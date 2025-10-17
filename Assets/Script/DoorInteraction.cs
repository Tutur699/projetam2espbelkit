using UnityEngine;
using System.Collections;

public class DoorInteraction : MonoBehaviour
{
    [Header("Références")]
    public Transform pivot;              // laisse vide si ce script est sur Pivot

    [Header("Ouverture")]
    public float openAngle = 90f;       // +90/-90 selon le sens
    public float animTime  = 0.35f;      // durée de l'anim

    bool isOpen;
    Quaternion closedLocal;
    Quaternion openLocal;
    Coroutine co;

    void Awake()
    {
        if (!pivot) pivot = transform;               // par défaut, on anime cet objet
        closedLocal = pivot.localRotation;           // on travaille en LOCAL
        openLocal   = closedLocal * Quaternion.Euler(0f, openAngle, 0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (co != null) StopCoroutine(co);
            co = StartCoroutine(AnimateTo(!isOpen)); // on choisit la cible APRÈS inversion
        }
    }

    IEnumerator AnimateTo(bool targetOpen)
    {
        Quaternion from = pivot.localRotation;
        Quaternion to   = targetOpen ? openLocal : closedLocal;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, animTime);
            pivot.localRotation = Quaternion.Slerp(from, to, Mathf.SmoothStep(0f,1f,t));
            yield return null;
        }

        pivot.localRotation = to;
        isOpen = targetOpen;
        co = null;
    }
}
