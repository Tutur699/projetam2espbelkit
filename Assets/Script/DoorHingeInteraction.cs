using UnityEngine;
using System.Collections;

public class DoorHingeInteraction : MonoBehaviour
{
    [Header("Références")]
    public Transform doorLeaf;   // -> 01.Low (le panneau de porte)
    public Transform hinge;      // -> Empty "Hinge" placé sur les gonds

    [Header("Animation")]
    public float openAngle = 90f;    // mets -90 si tu veux l’autre sens
    public float animTime  = 0.35f;  // durée d’ouverture/fermeture
    public KeyCode key     = KeyCode.E;

    // état
    bool isOpen = false;
    float currentAngle = 0f;

    // base fermée (en WORLD), pour éviter toute dérive
    Vector3 r0;           // vecteur (porte - charnière) à l’état fermé
    Quaternion rot0;      // rotation monde de la porte à l’état fermé

    void Start()
    {
        if (!doorLeaf || !hinge)
        {
            Debug.LogError("[DoorHingeInteraction] Assigne doorLeaf et hinge.");
            enabled = false; return;
        }

        // On mémorise la géométrie fermée
        r0   = doorLeaf.position - hinge.position;
        rot0 = doorLeaf.rotation;

        // On force l’état fermé au démarrage
        SetAngle(0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(key))
            StartCoroutine(AnimateTo(isOpen ? 0f : openAngle));
    }

    IEnumerator AnimateTo(float targetAngle)
    {
        float start = currentAngle;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, animTime);
            float a = Mathf.Lerp(start, targetAngle, Mathf.SmoothStep(0f, 1f, t));
            SetAngle(a);
            yield return null;
        }
        SetAngle(targetAngle);
        isOpen = Mathf.Abs(targetAngle) > 0.01f;
    }

    // Applique l’angle autour de la charnière, sans cumuler
    void SetAngle(float angleDeg)
    {
        currentAngle = angleDeg;

        // Axe en WORLD (charnière locale transformée)
        Vector3 axis = hinge.TransformDirection(Vector3.up);

        Quaternion R = Quaternion.AngleAxis(angleDeg, axis);

        doorLeaf.position = hinge.position + R * r0;
        doorLeaf.rotation = R * rot0;
    }
}
