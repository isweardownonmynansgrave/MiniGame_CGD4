using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    #region Instanzvariablen
    // Basic-Komponenten - Referenzen
    private Camera mainCamera;
    private CharacterController charCon;
    private Animator animator;

    // Input
    private Vector2 moveInput;
    private Vector2 lookInput;

    // Movement
    private Vector3 moveVector;
    private float currentRotationY;
    private float verticalVelocity = 0f;
    private float moveSpeed;

    // Jump
    private bool isPressingJump = false;
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;
    private float jumpInitialForce;
    private float jumpHoldForce;
    private float jumpHoldTime;

    // Environment
    private float gravity;

    // Server Coms
    private Queue<PlayerInputData> inputQueue = new Queue<PlayerInputData>(); // Queue

    // Game Mechanics
    ETeam teamChoice;
    [SerializeField] private GameObject punkteQuellePrefab1;
    [SerializeField] private GameObject punkteQuellePrefab2;
    private const int SOURCE1_BOUND = 3;
    private const int SOURCE2_BOUND = 5;

    [Header("DevArea")]
    [SerializeField] private bool isAnimated = false;

    // Debug
    [SerializeField] private float debugIntervall =  1f;
    [SerializeField] private bool printMcInfo = false;
    [SerializeField] private bool printGroundedInfo = false;
    [SerializeField] private bool printAnimatorValues = false;
    #endregion
    #region Netzwerkvariablen
    // score - WIP
    private NetworkVariable<float> score = new NetworkVariable<float>(
        -20f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    #endregion

    #region Accessoren
    // Später für Console nutzbar
    public float DebugIntervall
    {
        get { return debugIntervall; }
        set { if (value > 0) debugIntervall = value; }
    }
    public bool PrintMcInfo
    {
        get { return printMcInfo; }
        set { printMcInfo = value; }
    }
    public bool PrintGroundedInfo
    {
        get { return printGroundedInfo; }
        set { printGroundedInfo = value; }    
    }
    public bool PrintAnimatorValues
    {
        get { return printAnimatorValues; }
        set { printAnimatorValues = value; }
    }
    #endregion

    #region MonoBehaviour-Methoden
    private void Awake()
    {
        charCon = GetComponent<CharacterController>();
    }
    private void Start()
    {
        // Cam init
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("MainCamera nicht gefunden – Tag prüfen!");
        else
        {
            //currentRotationY = mainCamera.transform.forward;
            currentRotationY = 0;
        }

        // CharacterController init
        charCon = GetComponent<CharacterController>();

        if (isAnimated)
        {
            // Animator Komponente
            animator = GetComponentInChildren<Animator>();
            if (animator == null)
                Debug.LogError("Animator-Komponente nicht gefunden!");
        }

        // Cleanup
        SetCursorLock(true);

        // Network Spawn
        GetComponent<NetworkObject>().Spawn();

        // Debug
        StartCoroutine(DebugVariablesRoutine());
    }
    private void Update()
    {
        // Owner Check
        if (!IsOwner) return;

        // Bodencheck
        if (charCon.isGrounded)
        {
            // Jump Mechanic
            if (isPressingJump)
            {
                isJumping = true;
                jumpTimeCounter = jumpHoldTime;
                verticalVelocity = jumpInitialForce;
            }
            else
            {
                verticalVelocity = -1f; // Leicht am Boden kleben
                isJumping = false;
            }

            // Animationen
            // ?
        }
        else
        {
            if (isJumping && isPressingJump && jumpTimeCounter > 0f)
            {
                verticalVelocity += jumpHoldForce * Time.deltaTime;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                verticalVelocity += gravity * Time.deltaTime;
            }
        }

        // Bewegungsrichtung (horizontal)
        Vector3 moveDir = mainCamera.transform.forward * moveInput.y + mainCamera.transform.right * moveInput.x;
        moveDir.y = 0;
        moveDir.Normalize();

        // Vertikal hinzufügen
        moveDir.y = verticalVelocity;

        // Input fetchen
        /* Vector3 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); // WIP - fetch from InputSystem */
        var inputData = new PlayerInputData
        {
            Move = moveDir, // vorher: input
            Timestamp = Time.time
        };

        // Movement an Server Submiten
        SubmitInputServerRpc(inputData); // Bewegung wird Server-seitig angewendet
    }
    private void FixedUpdate()
    {
        if (!IsServer) return;

        while (inputQueue.Count > 0)
        {
            var input = inputQueue.Dequeue();
            Vector3 moveDir = new Vector3(input.Move.x, input.Move.y, input.Move.z);

            if (moveDir.sqrMagnitude > 0.01f)
            {
                transform.forward = moveDir;
                animator.SetBool("walking", true);
            }
            else
            {
                animator.SetBool("walking", false);
            }

            charCon.Move(moveDir * moveSpeed * Time.fixedDeltaTime);
        }
    }
    private void LateUpdate()
    {
        if (true) // Bedingung um Kamera zu rotieren, z.B. Movement vorhanden oder Tastendruck
        {
            RotateToView();
        }
    }
    #endregion

    #region Network-Handling
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Initialwert setzen (z. B. aus Spielmanager)
        if (IsServer)
        {
            SetInitialSpeedForPlayer();
        }
        if (IsOwner)
        {
            // Der Owner (Client) fordert Registrierung an
            //string chosenName = "SomeName"; // evtl. über ein UI gesetzt
            string chosenName = UIManager.Instance.Username; // Erster Ansatz - fetch from UI
            teamChoice = UIManager.Instance.TeamChoice;
            RequestRegistrationServerRpc(chosenName, teamChoice);
        }
    }
    
    private void SetInitialSpeedForPlayer() // Beispiel Init
    {
        // Hier könntest du z. B. auch Werte aus einem zentralen Profil ziehen
        float serverDefinedSpeed = 5f; // Beispiel
        moveSpeed = serverDefinedSpeed;
    }

    [ServerRpc] // Exec by Client
    private void RequestRegistrationServerRpc(string playerName, ETeam _teamChoice)
    {
        PlayerManager.Instance.RegisterPlayer(OwnerClientId, playerName, _teamChoice);
    }

    [ServerRpc]
    private void SubmitInputServerRpc(PlayerInputData input)
    {
        inputQueue.Enqueue(input);
    }
    #endregion

    #region On-Methoden
    public void OnMove(InputAction.CallbackContext context)
    {
        if (context.started || context.performed) // WIP - Bedingung prüfen
        {
            // Aktuellen Wert direkt auslesen
            moveInput = context.ReadValue<Vector2>();
        }
        else if (context.canceled)
        {
            // Input zurücksetzen, wenn die Eingabe endet
            moveInput = Vector2.zero;
        }

        // Vector2 in Vector3 umwandeln
        moveVector = new Vector3(moveInput.x, 0, moveInput.y);
        //BlendMovement(moveInput); // Animationen nicht existent
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // Aktuelle Look-Eingabe (z. B. Mausbewegung oder Controller-Stick) - Legcyy
            lookInput = context.ReadValue<Vector2>();

            if (Input.GetMouseButtonDown(1)) // 1=Rechtsklick
            {
                SetCursorLock(true);
                currentRotationY += lookInput.x; // Rotation mithilfe des Inputs ändern
            }
            if (Input.GetMouseButtonUp(1)) // Cursor für Button Clicks unlocken
                SetCursorLock(false);
        }
    }
    public void OnJump(InputAction.CallbackContext context)
    {
        // Jump Logik WIP
        if (context.started)
        {
            if(charCon.isGrounded)
            {
                isPressingJump = true;
                //UpwardsThrust();
            }
        }
        else if (context.canceled)
        {
            isPressingJump = false;
        }       
    }
    #endregion

    #region Playermethoden
    // Rotation
    private void RotateToView()
    {
        if (moveInput.sqrMagnitude > 0.01f) // Nur drehen, wenn Input vorhanden
        {
            Vector3 direction = mainCamera.transform.forward * moveInput.y + mainCamera.transform.right * moveInput.x;
            direction.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, 10 * Time.deltaTime);
        }
    }

    [ClientRpc] // Exec by server
    private void SetPlayerSettingsClientRpc(float _speed, // Wird immer zum Aufruf verwendet, Rest default
                                            float _jumpInitialForce = 8f,
                                            float _jumpHoldForce = 4f,
                                            float _jumpHoldTime = 0.2f,
                                            float _gravity = -20f)
    {
        moveSpeed = _speed;
        jumpInitialForce = _jumpInitialForce;
        jumpHoldForce = _jumpHoldForce;
        jumpHoldTime = _jumpHoldTime;
    }
    #endregion

    #region Hilfsmethoden
    private void SetCursorLock(bool _isLocked)
    {
        if (_isLocked)
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;

        Cursor.visible = !_isLocked;
    }
    #endregion

    #region GameMechanics
    // Wird vom Client aufgerufen
    [ServerRpc]
    public void RequestSpawnPointSourceServerRpc(int sourceChoice, ServerRpcParams rpcParams = default)
    {
        // Sicherheit: Nur gültige Werte erlauben
        if (sourceChoice < 1 || sourceChoice > 2)
        {
            Debug.Log("[PlayerMovement] Ungültige Auswahl der Quelle (nur 1 oder 2 erlaubt).");
            return;
        }

        // Hole den ClientId des Anrufers
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Sicherheitsprüfung: existiert der Spieler und ist er autorisiert?
        if (!IsAuthorized(clientId))
            return;

        // Logik wie in deiner Originalmethode – aber nur serverseitig!
        GameObject tempSource = null;
        if (score.Value >= SOURCE1_BOUND && sourceChoice == 1)
        {
            tempSource = Instantiate(punkteQuellePrefab1, transform.position + Vector3.forward, Quaternion.identity);
        }
        else if (score.Value >= SOURCE2_BOUND && sourceChoice == 2)
        {
            tempSource = Instantiate(punkteQuellePrefab2, transform.position + Vector3.forward, Quaternion.identity);
        }

        if (tempSource != null)
        {
            // Netzwerkobjekt aktivieren
            tempSource.GetComponent<NetworkObject>().Spawn();

            // Setup-Logik
            SetupPointSource(tempSource.GetComponent<Punktequelle>(), teamChoice, clientId);

            // Register?
        }
    }
    private void SetupPointSource(Punktequelle _source, ETeam _teamChoice, ulong _id)
    {
        _source.Team = _teamChoice;
        _source.OwnerId = _id;
    }
    private bool IsAuthorized(ulong clientId)
    {
        // Beispiel: Nur Owner darf seine Quelle spawnen
        return clientId == OwnerClientId;
    }
    public void SpawnSource1() => RequestSpawnPointSourceServerRpc(1); // UIManager kann für Buttons nun += dieser Methoden nutzen
    public void SpawnSource2() => RequestSpawnPointSourceServerRpc(2);
    #endregion

    #region Debugging
    private IEnumerator DebugVariablesRoutine()
    {
        while (true) // Unendliche Schleife
        {
            // Methoden hier
            if (printMcInfo) PrintMCInfo();
            if (printGroundedInfo) DebugGrounded();
            if (printAnimatorValues) DebugAnimatorValues();


            // Intervall abwarten
            yield return new WaitForSeconds(debugIntervall);
        }
    }
    private void DebugGrounded() => Debug.Log("isGrounded: " + charCon.isGrounded);
    private void PrintMCInfo()
    {
        Debug.Log("isHoldingJump: " + isJumping);
        Debug.Log("Velocity: " + charCon.velocity);
    }
    private void DebugAnimatorValues()
    {
        Debug.Log("LStickX: " + animator.GetFloat("LStickX"));
        Debug.Log("LStickY: " + animator.GetFloat("LStickY"));
    }
    #endregion
}
