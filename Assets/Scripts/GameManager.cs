using UnityEngine;
using Unity.Netcode;

public class GameManager : MonoBehaviour
{
    
    void Start()
    {
        bool isServer = NetworkManager.Singleton.IsServer;
        NetworkManager.Singleton.StartClient();
    }

    
    void Update()
    {

    }
}