using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private CinemachineVirtualCamera _cineMachineVirtualCamera;
    [SerializeField] private RotateInSyncWith _rotateInSyncwithCamera;

    private PlayerColorChanger _colorChanger;

    public override void OnNetworkSpawn()
    {
        _colorChanger = GetComponent<PlayerColorChanger>();

        if (IsOwner) 
        {
            OnLocalPlayerSpawn();
        }
        else
        {
            OnOtherPlayerSpawn();
        }
    }

    private void OnLocalPlayerSpawn()
    {
        _colorChanger.TurnBlue();
        _playerCamera.SetActive(true);
        _cineMachineVirtualCamera.gameObject.SetActive(true);
        _rotateInSyncwithCamera.gameObject.SetActive(true);
    }

    private void OnOtherPlayerSpawn()
    {
        _colorChanger.TurnRed();
    }
}
