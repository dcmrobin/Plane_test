using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class UIManager: MonoBehaviour
{
    [SerializeField] TextMeshProUGUI debugText = null;

    public void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            if (debugText != null)
            {
                debugText.text = "Host started";
            }
        }
        else
        {
            if (debugText != null)
            {
                debugText.text = "Host failed to Start";
            }
        }
    }

    public void StartServer()
    {
        if (NetworkManager.Singleton.StartServer())
        {
            if (debugText != null)
            {
                debugText.text = "Server started";
            }
        }
        else
        {
            if (debugText != null)
            {
                debugText.text = "Server failed to Start";
            }
        }
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            if (debugText != null)
            {
                debugText.text = "Client started";
            }
        }
        else
        {
            if (debugText != null)
            {
                debugText.text = "Client failed to Start";
            }
        }
    }
}