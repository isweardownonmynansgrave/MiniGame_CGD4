using UnityEngine;

public class Punktequelle : MonoBehaviour
{
    #region Instanzvariablen
    private ETeam team;
    private ulong ownerId;
    #endregion

    // Accessoren
    public ETeam Team
    {
        get => team;
        set
        {
            team = value;
        }
    }
    public ulong OwnerId
    {
        get => ownerId;
        set { ownerId = value; }
    }

    #region MonoBehaviour-Methoden
    void Start()
    {

    }
    void Update()
    {

    }
    #endregion
}
