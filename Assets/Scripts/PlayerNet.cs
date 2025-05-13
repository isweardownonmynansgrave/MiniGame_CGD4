using Unity.Netcode;
using UnityEngine;

public class PlayerNet : NetworkBehaviour
{
    public GameObject pillar;
    bool isOwner;

    public NetworkVariable<int> score = new NetworkVariable<int>();
    void Start()
    {

    }
    void Update()
    {
        if (!isOwner) { return; }

        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(x,0,z);
        if (movement.magnitude > 0)
        {
            MovingServerRPC(movement);
        }

        //score.Value += 1;
        if (Input.GetKeyDown(KeyCode.Space))
            CreateObject();
    }

    [ServerRpc]
    void CreateObjectServerRPC()
    {
        GameObject obj = Instantiate(pillar);
        obj.GetComponent<NetworkObject>.Spawn(true);
    }


    [ServerRpc]
    void MovingServerRPC(Vector3 _movement)
    {
        [ServerRpc]
        void MovingServerRPC(Vector3 position)
        {
            transform.position = position;
            MovingClientRPC(transform.position);
        }
    }
}