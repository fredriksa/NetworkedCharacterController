using Cinemachine;
using Core;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private NetworkMovementComponent _playerMovement;

    private int _tick = 0;
    private float _accumulatedTickTime = 0;

    private Vector3 _moveInput = Vector3.zero;
    private Vector3 _rotationInput = Vector3.zero;

    public override void OnNetworkSpawn()
    {
        _playerMovement = GetComponent<NetworkMovementComponent>();
    }

    private void Update()
    {
        _accumulatedTickTime += Time.deltaTime; 
        if (_accumulatedTickTime > NetworkConstants.TickRate)
        {
            Tick();
            _accumulatedTickTime -= NetworkConstants.TickRate;
            _tick++;
        }
    }

    private void Tick()
    {
        if (IsClient && IsOwner)
        {
            UpdateInput();
            _playerMovement.HandleInput(_tick, _moveInput, _rotationInput);
            ResetInput();
        }
    }

    private void UpdateInput()
    {
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.z = Input.GetAxisRaw("Vertical");

        _rotationInput.x += Input.GetAxisRaw("Mouse X");
        _rotationInput.y += Input.GetAxisRaw("Mouse Y");
    }

    private void ResetInput()
    {
        _rotationInput.x = 0;
        _rotationInput.y = 0;
    }
}
