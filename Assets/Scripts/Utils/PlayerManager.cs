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
    private Dictionary<ulong, ETeam> teamDict = new();





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
    private Color PickTeamColor(ETeam _teamChoice)
    {
        Color temp = Color.green; // Wenn Objekte grün erscheinen, läuft etwas bei der Farbwahl schief
        switch (_teamChoice)
        {
            case ETeam.Rot:
                temp = Color.red;
                break;
            case ETeam.Blau:
                temp = Color.blue;
                break;
            case ETeam.Gelb:
                temp = Color.yellow;
                break;
        }
        return temp;
    }
    #endregion

    #region UserManagement
    // RPC-Variante
    [ServerRpc(RequireOwnership = false)]
    public void RegisterPlayerServerRpc(ulong _clientId, string _playerName, ETeam _teamChoice)
    {
        if (!connectedPlayers.ContainsKey(_clientId))
        {
            Color tempColor = PickTeamColor(_teamChoice); // Teamcolor, 0-Red; 1-Blue; 2-Yellow
            PlayerData tempData = new PlayerData(_clientId, _playerName, tempColor, _teamChoice);

            // Zu Dicts hinzufügen
            connectedPlayers.Add(_clientId, tempData);
            teamDict.Add(_clientId, _teamChoice);
        }
    }
    public void RegisterPlayer(ulong _clientId, string _playerName, ETeam _teamChoice)
    {
        if (!connectedPlayers.ContainsKey(_clientId))
        {
            Color tempColor = PickTeamColor(_teamChoice); // Teamcolor, 0-Red; 1-Blue; 2-Yellow
            PlayerData tempData = new PlayerData(_clientId, _playerName, tempColor, _teamChoice);

            connectedPlayers.Add(_clientId, tempData);
            teamDict.Add(_clientId, _teamChoice);
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
