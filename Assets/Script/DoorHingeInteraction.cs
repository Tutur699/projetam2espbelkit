using UnityEngine;
using System.Collections;

public class DoorHingeInteraction : MonoBehaviour
{
    [Header("Références")]
    public Transform doorLeaf;   // -> 01_Low (le panneau de porte)
    public Transform hinge;      // -> Empty "Hinge" placé sur les gonds

    [Header("Animation")]
    public float openAngle = 90f;    // mets -90 si tu veux l’autre sens
    public float animTime  = 0.35f;  // durée d’ouverture/fermeture

    // état
    bool isOpen = false;
    bool isAnimating = false;
    float currentAngle = 0f;

    // base fermée (en WORLD), pour éviter toute dérive
    Vector3 r0;           // vecteur (porte - charnière) à l’état fermé
    Quaternion rot0;      // rotation monde de la porte à l’état fermé

    void Start()
    {
        if (!doorLeaf || !hinge)
        {
            Debug.LogError("[DoorHingeInteraction] Assigne doorLeaf et hinge.", this);
            enabled = false; return;
        }

        r0   = doorLeaf.position - hinge.position;
        rot0 = doorLeaf.rotation;
        SetAngle(0f);
    }

    // Appelée par le joueur via Raycast
    public void Toggle()
    {
        if (isAnimating) return;
        StartCoroutine(AnimateTo(isOpen ? 0f : openAngle));
    }

    IEnumerator AnimateTo(float targetAngle)
    {
        isAnimating = true;

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

        isAnimating = false;
    }

    // Applique l’angle autour de la charnière, sans cumuler
    void SetAngle(float angleDeg)
    {
        currentAngle = angleDeg;
        Vector3 axis = hinge.TransformDirection(Vector3.up); // axe monde
        Quaternion R = Quaternion.AngleAxis(angleDeg, axis);
        doorLeaf.position = hinge.position + R * r0;
        doorLeaf.rotation = R * rot0;
    }
}
