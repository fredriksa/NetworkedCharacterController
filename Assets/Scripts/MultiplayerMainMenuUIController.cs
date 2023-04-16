using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class MultiplayerMainMenuUIController : MonoBehaviour
{
    [SerializeField] private GameObject multiplayerMainMenuUI;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;

    void Start()
    {
        hostButton.onClick.AddListener(() => StartHost());
        clientButton.onClick.AddListener(() => StartClient());

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        HideMenu();
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        HideMenu();
    }

    private void HideMenu()
    {
        multiplayerMainMenuUI.SetActive(false);

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
