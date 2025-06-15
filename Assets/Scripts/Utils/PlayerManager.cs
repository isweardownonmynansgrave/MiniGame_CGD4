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
    private Dictionary<ulong, bool> teamDictIsRed = new();





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

    #region Hilfsmethoden
    public PlayerData GetPlayer(ulong clientId)
    {
        return connectedPlayers.TryGetValue(clientId, out var data) ? data : null;
    }
    #endregion

    #region UserManagement
    // RPC-Variante
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ulong _clientId, string _playerName, bool _isTeamRed)
    {
        if (!connectedPlayers.ContainsKey(_clientId))
        {
            // Add player to dictionary, assign default settings etc.
            Color tempColor = _isTeamRed ? Color.red : Color.blue; // Teamcolor, 0-Red; 1-Blue

            PlayerData tempData = new PlayerData(_clientId, _playerName, tempColor, _isTeamRed ? 0 : 1); // 0-Red; 1-Blue

            // Zu Dicts hinzufügen
            connectedPlayers.Add(_clientId, tempData);
            teamDictIsRed.Add(_clientId, _isTeamRed);
        }
    }
    public void RegisterPlayer(ulong _clientId, string _playerName, bool _isTeamRed)
    {
        if (!connectedPlayers.ContainsKey(_clientId))
        {
            Color tempColor = _isTeamRed ? Color.red : Color.blue; // Teamcolor, 0-Red; 1-Blue
            PlayerData tempData = new PlayerData(_clientId, _playerName, tempColor, _isTeamRed ? 0 : 1); // 0-Red; 1-Blue

            connectedPlayers.Add(_clientId, tempData);
            teamDictIsRed.Add(_clientId, _isTeamRed);
            Debug.Log($"Player {_playerName} (ClientId {_clientId}) registered.");
        }
    }
    public void UnregisterPlayer(ulong _clientId)
    {
        if (connectedPlayers.ContainsKey(_clientId))
            connectedPlayers.Remove(_clientId);
        else
        { Debug.Log("Fehler - UnregisterPlayer Aufruf mit unbekanntem Key."); }
    }
    #endregion
}
