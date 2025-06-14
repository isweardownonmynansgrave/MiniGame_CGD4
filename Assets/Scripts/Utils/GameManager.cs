using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Backbone Management
    public bool IsClient = false;
    public bool IsDedicServer = false;

    // Singleton
    public static GameManager Instance;


    #region MonoBehaviour-Methoden
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
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
    #endregion
    void Update()
    {
        
    }
}