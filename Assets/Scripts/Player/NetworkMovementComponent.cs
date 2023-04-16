using Cinemachine;
using Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class NetworkMovementComponent : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CinemachineVirtualCamera _virtualCamera;
    [SerializeField] private RotateInSyncWith _rotateInSyncWith;

    [Header("Attributes")]
    [SerializeField] private float _movementSpeed = 5f;
    [SerializeField] private float _rotationSpeed = 600f;

    private CharacterController _cc;
    private NetworkVariable<TransformState> _serverTransformState = new NetworkVariable<TransformState>();

    private const int BUFFER_SIZE = (int)NetworkConstants.TicksPerSecond * 5; // 5 seconds history
    private TransformState[] _clientTransformStates = new TransformState[BUFFER_SIZE];
    private InputState[] _clientInputStates = new InputState[BUFFER_SIZE];

    [Header("Gizmos")]
    [SerializeField] private Color _trackingMeshCollor = Color.red;
    [SerializeField] private MeshFilter _meshFilter;

    public override void OnNetworkSpawn()
    {
        _cc = GetComponent<CharacterController>();
        _rotateInSyncWith = GetComponent<RotateInSyncWith>();
        _serverTransformState.OnValueChanged += OnObserveServerTransformStateChanged;
    }

    internal void HandleInput(int tick, Vector3 moveInput, Vector3 rotationInput)
    {
        if (IsServer)
        {
            HybridRotate(rotationInput);
            HybridMove(moveInput);

            UpdateServerTransformState(tick);
        }
        else
        {
            SendMoveToServerRpc(tick, moveInput);
            HybridMove(moveInput);

            ClientCacheInput(tick, moveInput);
            ClientCacheTransformState(tick);
        }
    }

    private void UpdateServerTransformState(int tick)
    {
        TransformState transformState = new TransformState()
        {
            Tick = tick,
            Position = transform.position,
            Rotation = transform.rotation
        };

        _serverTransformState.Value = transformState;
    }

    [ServerRpc]
    private void SendMoveToServerRpc(int tick, Vector3 moveInput)
    {
        HybridMove(moveInput);
        UpdateServerTransformState(tick);
    }

    private void HybridRotate(Vector3 rotationInput)
    {
        Vector3 rotation = rotationInput * _rotationSpeed * NetworkConstants.TickRate;

        // Swap around as mouse X should rotate around Y and mouse Y around X axis.
        float auxY = rotation.y;
        rotation.y = rotation.x;
        rotation.x = auxY;
        rotation.z = 0;

        // Invert horizontal movement
        rotation.x = -rotation.x;
        _virtualCamera.transform.Rotate(rotation);

        // Prevent Z axis rotation
        Vector3 angles = _virtualCamera.transform.eulerAngles;
        angles.z = 0;
        _virtualCamera.transform.rotation = Quaternion.Euler(angles);
        _rotateInSyncWith.Rotate();
    }

    private void HybridMove(Vector3 moveInput)
    {
        Vector3 movement = moveInput.normalized;
        movement *= _movementSpeed;
        movement = movement.x * transform.right + movement.z * transform.forward;

        if (!_cc.isGrounded)
        {
            movement.y = Physics.gravity.y;
        }

        _cc.Move(movement * NetworkConstants.TickRate);
    }

    private void ClientCacheInput(int tick, Vector3 moveInput)
    {
        int cacheIndex = tick % BUFFER_SIZE;
        _clientInputStates[cacheIndex] = new InputState()
        {
            Tick = tick,
            MovementInput = moveInput
        };
    }

    private void ClientCacheTransformState(int tick)
    {
        int cacheIndex = tick % BUFFER_SIZE;
        _clientTransformStates[cacheIndex] = new TransformState()
        {
            Tick = tick,
            Position = transform.position
        };
    }

    private void PerformReconciliation(TransformState newServerState)
    {
        TransformState clientStateAtTick = null;

        int clientTransformIndex = -1;
        for (int i = 0; i < _clientTransformStates.Length; i++) 
        { 
            if (_clientTransformStates[i].Tick == newServerState.Tick)
            {
                clientStateAtTick = _clientTransformStates[i];
                clientTransformIndex = i;
                break;
            }
        }

        if (clientStateAtTick == null) 
        {
            Debug.Log("Could not find client state at tick!");
            return;
        }

        if (clientStateAtTick.Position == newServerState.Position)
        {
            // No need to perform reconciliation - positions match.
            return;
        }

        Debug.Log("Performing reconciliation");
        _clientTransformStates[clientTransformIndex] = new TransformState()
        {
            Tick = newServerState.Tick,
            Position = newServerState.Position,
        };

        _cc.enabled = false;
        transform.position = newServerState.Position;
        _cc.enabled = true;

        IEnumerable<InputState> inputStates = _clientInputStates.Where(inputState => inputState.Tick > newServerState.Tick);
        inputStates = inputStates.OrderBy(inputState => inputState.Tick);

        foreach (InputState inputState in inputStates) 
        {
            HybridMove(inputState.MovementInput);
            ClientCacheTransformState(inputState.Tick);
        }
    }

    private void OnObserveServerTransformStateChanged(TransformState oldServerState, TransformState newServerState)
    {
        // If we are the server, we already have the new state "active" in our game.
        if (IsServer)
        {
            return;
        }

        // If we are the player for which the server updated it's state.
        // We need to perform reconciliation if needed.
        if (IsLocalPlayer)
        {
            PerformReconciliation(newServerState);
            return;
        }

        // If we are observing another player entity, we just update it's position to the latest position.
        // This would be a good point to perform interpolation between transforms or such.
        transform.position = newServerState.Position;
        transform.rotation = newServerState.Rotation;
        Debug.Log("Got rotation from other player: " + transform.rotation.x + " - " + transform.rotation.y + " - " + transform.rotation.z);
    }

    public void OnDrawGizmos()
    {
        if (_serverTransformState.Value == null)
        {
            return;
        }

        if (IsLocalPlayer)
        {
            Gizmos.color = _trackingMeshCollor;
            Gizmos.DrawMesh(_meshFilter.mesh, _serverTransformState.Value.Position);
        }
    }
}
