using Unity.Netcode;
using UnityEngine;

public struct PlayerInputData : INetworkSerializable
{
    public Vector3 Move;
    public float Timestamp;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref Move);
        serializer.SerializeValue(ref Timestamp);
    }
}
