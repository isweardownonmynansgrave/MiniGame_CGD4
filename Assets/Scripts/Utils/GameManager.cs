using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Backbone Management
    public bool IsClient = false;
    public bool IsDedicServer = false;
    private List<Punktequelle> punktequellen;

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
        punktequellen = new List<Punktequelle>();
    }
    #endregion
    void Update()
    {

    }
    
    // Server-RPC, wird vom Client aufgerufen
    [ServerRpc(RequireOwnership = false)]
    public void RegisterObjectServerRpc(Punktequelle objectRef, ServerRpcParams rpcParams = default)
    {
        if (!punktequellen.Contains(objectRef))
        {
            punktequellen.Add(objectRef);
            Debug.Log($"[GameManager] Objekt {objectRef} wurde registriert.");
        }
        else
        {
            Debug.Log($"[GameManager] Objekt {objectRef} ist bereits registriert.");
        }
    }
}