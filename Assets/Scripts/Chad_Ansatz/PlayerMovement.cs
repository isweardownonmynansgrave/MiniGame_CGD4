using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    public float moveSpeed = 5f;

    private CharacterController controller;
    private Queue<PlayerInputData> inputQueue = new Queue<PlayerInputData>();

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        var inputData = new PlayerInputData
        {
            Move = input,
            Timestamp = Time.time
        };

        SubmitInputServerRpc(inputData);
    }

    [ServerRpc]
    private void SubmitInputServerRpc(PlayerInputData input)
    {
        inputQueue.Enqueue(input);
    }

    private void FixedUpdate()
    {
        if (!IsServer) return;

        while (inputQueue.Count > 0)
        {
            var input = inputQueue.Dequeue();
            Vector3 moveDir = new Vector3(input.Move.x, 0, input.Move.y);

            // Optional: Rotation in Blickrichtung
            if (moveDir.sqrMagnitude > 0.01f)
            {
                transform.forward = moveDir;
            }

            controller.Move(moveDir * moveSpeed * Time.fixedDeltaTime);
        }
    }
}
