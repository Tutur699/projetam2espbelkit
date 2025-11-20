using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class OwnerSetup : NetworkBehaviour
{/*
    [Header("Local-only")]
    public Camera playerCamera;             // => PlayerCam (Camera)
    public AudioListener audioListener;     // sur la PlayerCam
    public MonoBehaviour[] localControllers; // ex: PlayerController, PlayerInput, ton script de tir, etc.
    public GameObject[] localOnlyObjects;    // HUD, Arms FPV, etc.

    [Header("Visible pour tous")]
    public GameObject worldModel;           // => "body" (le mesh visible par les autres)

    void Awake()
    {
        // tout OFF par sécurité
        if (playerCamera)   playerCamera.gameObject.SetActive(false);
        if (audioListener)  audioListener.enabled = false;
        if (localControllers != null) foreach (var c in localControllers) if (c) c.enabled = false;
        if (localOnlyObjects != null) foreach (var go in localOnlyObjects) if (go) go.SetActive(false);
        if (worldModel) worldModel.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        // le mesh du joueur est visible pour tout le monde
        if (worldModel) worldModel.SetActive(true);

        // seule la machine qui possède ce joueur a la caméra + inputs
        if (IsOwner)
        {
            if (playerCamera)   playerCamera.gameObject.SetActive(true);
            if (audioListener)  audioListener.enabled = true;
            if (localControllers != null) foreach (var c in localControllers) if (c) c.enabled = true;
            if (localOnlyObjects != null) foreach (var go in localOnlyObjects) if (go) go.SetActive(true);
        }
    }*/
}
