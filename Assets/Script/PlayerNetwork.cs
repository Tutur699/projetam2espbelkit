using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using Cinemachine;
using StarterAssets;

public class PlayerNetwork : NetworkBehaviour
{
    private FPC_PLAYER _controller;

    private void Awake()
    {
        _controller = GetComponent<FPC_PLAYER>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        SetupCameraForLocalPlayer();
    }


    private void SetupCameraForLocalPlayer()
    {
        if (!IsOwner) return;
        // Caméra principale (de la scène)
        Camera mainCam = Camera.main;
        if (mainCam == null)
        {
            Debug.LogError("Aucune MainCamera trouvée dans la scène (tag MainCamera).");
            return;
        }

        // On récupère la CinemachineVirtualCamera présente dans la scène
        CinemachineVirtualCamera vcam = FindFirstObjectByType<CinemachineVirtualCamera>();
        if (vcam == null)
        {
            Debug.LogError("Aucune CinemachineVirtualCamera trouvée dans la scène.");
            return;
        }

        if (_controller == null || _controller.CinemachineCameraTarget == null)
        {
            Debug.LogError("ThirdPersonController ou CinemachineCameraTarget non assigné sur le joueur.");
            return;
        }

        // On dit à Cinemachine de suivre CE joueur local
        Transform target = _controller.CinemachineCameraTarget.transform;
        vcam.Follow = target;
        vcam.LookAt = target;

        // On s'assure que la caméra et l'audio sont actifs pour ce client
        mainCam.enabled = true;
        var audioListener = mainCam.GetComponent<AudioListener>();
        if (audioListener != null)
            audioListener.enabled = true;
    }
}
