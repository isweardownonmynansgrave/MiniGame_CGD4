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

    // Jump
    private bool isPressingJump = false;
    private bool isJumping = false;
    private float jumpTimeCounter = 0f;

    [Header("DevArea")]
    [SerializeField] private bool isAnimated = false;

    // Debug
    [SerializeField] private float debugIntervall =  1f;
    [SerializeField] private bool printMcInfo = false;
    [SerializeField] private bool printGroundedInfo = false;
    [SerializeField] private bool printAnimatorValues = false;
    #endregion
    #region Netzwerkvariablen
    // Nur Server darf schreiben
    // Player Settings
    private NetworkVariable<float> moveSpeed = new NetworkVariable<float>(
        5f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<float> jumpInitialForce = new NetworkVariable<float>(
        8f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<float> jumpHoldForce = new NetworkVariable<float>(
        4f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    private NetworkVariable<float> jumpHoldTime = new NetworkVariable<float>(
        0.2f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    // Environment Settings
    private NetworkVariable<float> gravity = new NetworkVariable<float>(
        -20f,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );
    // Queue
    private Queue<PlayerInputData> inputQueue = new Queue<PlayerInputData>();
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
                jumpTimeCounter = jumpHoldTime.Value;
                verticalVelocity = jumpInitialForce.Value;
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
                verticalVelocity += jumpHoldForce.Value * Time.deltaTime;
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                verticalVelocity += gravity.Value * Time.deltaTime;
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
            }

            charCon.Move(moveDir * moveSpeed.Value * Time.fixedDeltaTime);
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



    [ServerRpc]
    private void SubmitInputServerRpc(PlayerInputData input)
    {
        inputQueue.Enqueue(input);
    }

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

            // Rotation mithilfe des Inputs ändern
            currentRotationY += lookInput.x;
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
