using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool IsClient = false;
    public bool IsDedicServer = false;

    void Start()
    {
        if (!IsClient)
        {
            if (!IsDedicServer)
                NetworkManager.Singleton.StartHost(); // "Localhost" Variante
            else
                NetworkManager.Singleton.StartServer(); // Dedicated Server Variante
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
        
    }

    void Update()
    {
        
    }
}