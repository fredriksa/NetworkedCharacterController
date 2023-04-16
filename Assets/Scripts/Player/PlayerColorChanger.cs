using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerColorChanger : MonoBehaviour
{
    [SerializeField] private GameObject body;
    [SerializeField] private Material blue;
    [SerializeField] private Material red;

    public void TurnRed()
    {
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshRenderer.material = red;
    }

    public void TurnBlue()
    {
        MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
        meshRenderer.material = blue;
    }
}
