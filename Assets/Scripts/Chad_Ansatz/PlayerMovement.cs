public class PlayerMovement : NetworkBehaviour
{
    private CharacterController controller;

    // Nur Server darf schreiben
    private NetworkVariable<float> moveSpeed = new NetworkVariable<float>(
        5f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private Queue<PlayerInputData> inputQueue = new Queue<PlayerInputData>();

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialwert setzen (z. B. aus Spielmanager)
        if (IsServer)
        {
            SetInitialSpeedForPlayer();
        }
    }

    private void SetInitialSpeedForPlayer()
    {
        // Hier könntest du z. B. auch Werte aus einem zentralen Profil ziehen
        float serverDefinedSpeed = 7.5f; // Beispiel
        moveSpeed.Value = serverDefinedSpeed;
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

            if (moveDir.sqrMagnitude > 0.01f)
            {
                transform.forward = moveDir;
            }

            controller.Move(moveDir * moveSpeed.Value * Time.fixedDeltaTime);
        }
    }
}
