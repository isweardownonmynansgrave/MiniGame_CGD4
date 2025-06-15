using Unity.Collections;
using UnityEngine;

public class PlayerData
{
    public ulong ClientId;
    public FixedString32Bytes Name;
    public Color PlayerColor;
    public ETeam Team;
    // Weitere per-Spieler Settings

    public PlayerData(ulong _clientId, FixedString32Bytes _name, Color _playerColor, ETeam _team)
    {
        ClientId = _clientId;
        Name = _name;
        PlayerColor = _playerColor;
        Team = _team;
    }
}
