using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateInSyncWith : MonoBehaviour
{
    [SerializeField] public Transform with;

    [SerializeField] public bool x;
    [SerializeField] public bool y;
    [SerializeField] public bool z;

    private Vector3 _newRotation = Vector3.zero;

    void Update()
    {
        bool shouldRotate = x | y | z;
        if (!shouldRotate) return;

        if (x)
        {
            _newRotation.x = with.rotation.eulerAngles.x;
        }

        if (y)
        {
            _newRotation.y = with.rotation.eulerAngles.y;
        }

        if (z)
        {
            _newRotation.z = with.rotation.eulerAngles.z;
        }

        transform.rotation = Quaternion.Euler(_newRotation);    
    }

    public void Rotate()
    {
        Update();
    }
}
