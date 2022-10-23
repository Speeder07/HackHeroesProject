using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System;

public class MenuUI : MonoBehaviour
{
    [SerializeField] TMPro.TMP_InputField ipInput;
    [SerializeField] TMPro.TMP_InputField portAddres;
    public Button hostButton;
    public Button clientButton;
    void Start()
    {
        hostButton.onClick.AddListener(OnHost);
        clientButton.onClick.AddListener(OnClient);
    }

    void OnHost()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipInput.text,(ushort)(Int32.Parse(portAddres.text)));
        NetworkManager.Singleton.StartHost();
        this.gameObject.SetActive(false);
    }

    void OnClient()
    {
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ipInput.text,(ushort)(Int32.Parse(portAddres.text)));
        NetworkManager.Singleton.StartClient();
        this.gameObject.SetActive(false);
    }
}
