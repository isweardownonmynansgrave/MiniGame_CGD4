using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.Networking.PlayerConnection;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    #region Instanzvariablen
    // Singleton
    public static PlayerManager Instance;

    private Dictionary<ulong, PlayerData> connectedPlayers = new();

    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ulong _clientId, string _playerName, bool isTeamRed)
    {
        // Add player to dictionary, assign default settings etc.
        Color tempColor = isTeamRed ? Color.red : Color.blue; // Teamcolor, 0-Red; 1-Blue

        PlayerData tempData = new PlayerData(_clientId, _playerName, tempColor, isTeamRed ? 0 : 1); // 0-Red; 1-Blue

        // Zum pDict hinzufügen
        connectedPlayers.Add(_clientId, tempData);
    }
    public void RegisterPlayer(ulong _clientId, string _playerName, bool isTeamRed)
    {
        if (!connectedPlayers.ContainsKey(_clientId))
        {
            Color tempColor = isTeamRed ? Color.red : Color.blue; // Teamcolor, 0-Red; 1-Blue
            PlayerData tempData = new PlayerData(_clientId, _playerName, tempColor, isTeamRed ? 0 : 1); // 0-Red; 1-Blue

            connectedPlayers.Add(_clientId, tempData);
            Debug.Log($"Player {_playerName} (ClientId {_clientId}) registered.");
        }
    }
    public void UnregisterPlayer(ulong _clientId)
    {
        if (connectedPlayers.ContainsKey(_clientId))
            connectedPlayers.Remove(_clientId);
        else
        {
            Debug.Log("Fehler - UnregisterPlayer Aufruf mit unbekanntem Key.");
        }
    }
    public PlayerData GetPlayer(ulong clientId)
    {
        return connectedPlayers.TryGetValue(clientId, out var data) ? data : null;
    }

    // Player Settings
    private float moveSpeed = 5f; // Default: 5f
    private float jumpInitialForce = 8f; // Default: 8f
    private float jumpHoldForce = 4f; // Default: 4f
    private float jumpHoldTime = 0.2f; // Default: 0.2f
    private float gravity = -20f; // Default: -20f
    #endregion

    #region MonoBehaviour-Methoden 
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    void Start()
    {

    }
    void Update()
    {

    }
    #endregion
}
