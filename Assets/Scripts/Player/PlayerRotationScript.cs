using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerRotationScript : NetworkBehaviour
{
    public GameObject playerCamera;
    [SerializeField] private float _rotationSpeed = 10;

    public override void OnNetworkSpawn()
    {
        if (IsLocalPlayer)
        {
            enabled = true;
        }
    }
    
    void Update()
    {
        float inputMouseY = -Input.GetAxisRaw("Mouse Y");
        float inputMouseX = Input.GetAxisRaw("Mouse X");

        // Update body rotation
        Vector3 newRotation = transform.localRotation.eulerAngles;
        newRotation.y += inputMouseX * _rotationSpeed;
        transform.localRotation = Quaternion.Euler(newRotation);

        // Update camera movement
       /* Vector3 oldRot = playerCamera.transform.rotation.eulerAngles;
        Vector3 newCameraRotation = playerCamera.transform.rotation.eulerAngles;
        newCameraRotation.x += inputMouseY * _rotationSpeed;


        Debug.Log("NewCameraRotation X: " + newCameraRotation.x);
        //newCameraRotation.x = Mathf.Clamp(newCameraRotation.x, -50, 60);

        playerCamera.transform.rotation = Quaternion.Euler(newCameraRotation);*/
    }
}
