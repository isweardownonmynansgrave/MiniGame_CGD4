using System.Collections;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class MC_BlobCC2 : MonoBehaviour
{
    #region Instanzvariablen
    // Player Settings
    [Header("Player")]
    [Tooltip("Normale Geschwindigkeit für alle Richtungen, sofern nicht anders angegeben")]
    [SerializeField] private float speed = 3f;
    [Tooltip("Sprint Geschwindigkeit - WIP")]
    [SerializeField] private float speedSprint = 7f;
    [SerializeField] private float jumpInitialForce = 8f;
    [SerializeField] private float jumpHoldForce = 4f;
    [SerializeField] private float jumpHoldTime = 0.2f;

    [Header("Environment")]
    [SerializeField] private float gravity = -20f;

    // Basic-Komponenten - Referenzen
    private Camera mainCamera;
    private CharacterController charCon;
    private Animator animator;

    // Input
    private Vector2 moveInput;
    private Vector2 lookInput;
    private bool isSprinting = false;

    // Movement
    private Vector3 moveVector;
    private float currentRotationY;
    private float verticalVelocity = 0f;

    // Jump
    private Vector3 jumpVector;
    private bool isPressingJump = false;
    private float jumpTimer;
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

    #region MonoBehaviour-Methoden
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
            GroundedTriggered();
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

        // Geschwindigkeit berechnen
        float currentSpeed = isSprinting ? speedSprint : speed;
        Vector3 finalMove = moveDir * currentSpeed;

        // Vertikal hinzufügen
        finalMove.y = verticalVelocity;

        // Bewegung anwenden
        charCon.Move(finalMove * Time.deltaTime);
    }
    private void LateUpdate()
    {
        if (true) // Bedingung um Kamera zu rotieren, z.B. Movement vorhanden oder Tastendruck
        {
            RotateToView();
        }
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
        BlendMovement(moveInput);
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

    // Animationen
    private void BlendMovement(Vector2 moveInput)
    {
        // Nur ausführen, wenn Prefab mit Animator vorhanden
        if (!isAnimated) return;

        animator.SetFloat("LStickX", moveInput.x);
        animator.SetFloat("LStickY", moveInput.y);
    }
    private void GroundedTriggered(bool b = true)
    {
        // Nur ausführen, wenn Prefab mit Animator vorhanden
        if (!isAnimated) return;

        animator.SetBool("Jumping", !b);
        animator.SetBool("Falling", !b);
        animator.SetBool("Landed", b);
    }
    #endregion

    #region Hilfsmethoden
    private void SetCursorLock(bool _isLocked)
    {
        if(_isLocked)
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
/*
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠿⣿⡟⣿⣿⣿⣿⣿⣿⠟⣟⣿⣿⣽⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⢿⣿⢿⣿⣛⣿⣿⣿⣻⣿⣿⣿⣿⣿⢟⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡷⢿⣯⣿⠹⣿⣿⣿⣿⣿⣯⣟⣿⣿⣮⡷⣬⣻⣟⡿⢿⡿⣿⣿⣿⣿⣿⡿⢿⢿⠿⣝⠻⢫⠾⠿⣝⣿⣮⣿⣿⡿⠡⣷⣿⠋⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡈⡯⢷⡄⣝⣿⣿⣿⣤⡶⣧⣭⣽⣟⣻⡿⣿⡿⣿⣿⣿⣷⣶⣶⣾⣿⣿⣿⣿⣿⡿⡫⠿⠿⣿⣿⣿⣿⣿⡿⣅⣷⡋⠪⣡⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡇⡇⡹⣉⢹⡿⣿⢿⢿⡵⠶⣦⣭⣑⡿⠻⢿⣿⣷⣾⣽⣟⣛⣿⣿⣿⡟⣻⡭⣞⣰⣾⣿⢶⢶⢶⣭⣻⡿⣳⣥⡇⠸⢳⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣕⣀⢈⣧⣿⣞⣣⠏⠖⠙⠒⠟⠿⢿⣦⣦⣬⣟⣿⢻⣿⡿⠙⣿⢿⣷⡿⣿⠾⠿⠋⠙⠋⠓⠖⠻⢭⣾⠟⣦⡀⣾⣭⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⣝⣾⢿⠺⢿⠅⠀⠀⠀⠀⠀⠀⠂⠉⠉⠉⠉⠀⠉⢐⡲⢄⣀⠀⠀⠀⠀⠀⠀⠂⠀⠐⢤⢀⡀⢻⡟⢱⡿⢫⣕⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡆⣸⣀⡘⣶⣆⠀⢀⠄⠐⠠⠤⢒⠀⠀⠀⠀⠀⢈⣙⣋⠀⢀⢀⡀⠘⢢⠤⠐⠀⣀⡌⠨⣾⣧⢠⣼⣅⣼⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡍⠁⣿⡇⣿⣿⣶⣌⡙⢃⣲⣂⡴⠰⣌⠃⠀⠀⣿⣿⣿⡇⠀⢈⡀⠀⠦⣔⣺⡾⠏⣐⣿⣿⡿⣸⣿⣀⣾⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠋⣿⣧⠹⣿⣿⣿⣿⣶⣛⣫⣴⣷⡿⠁⠀⣸⣿⣿⣿⣿⡄⠈⢷⣮⣬⣎⣍⣥⣼⣿⡿⠟⢡⣿⣿⣸⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣯⢆⠌⡛⠛⠿⠿⠿⠻⠛⣃⡀⢤⣶⣶⣿⣿⣿⣿⣷⣦⣄⣉⡛⣛⠙⠋⠋⣉⣠⣮⢣⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣟⡎⠀⡀⢶⣴⣷⣿⣿⡿⠟⠀⣤⣿⣿⣿⣿⣿⣿⣿⣿⡈⠙⣿⣿⣿⣿⡿⣿⠳⡃⣃⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣆⠇⠆⢸⣿⣿⣿⣿⠁⠀⠸⣿⣿⣿⣿⣿⣿⡿⣿⣿⣿⠀⠈⠿⣿⣿⣿⡿⢰⠿⢱⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣧⣟⣾⡌⡨⠀⣷⠟⣏⠇⣠⣷⡀⠀⠙⠻⠿⣿⠻⠾⠟⠉⠁⠻⡦⠈⣽⡿⢿⠇⡓⣿⣿⣻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⡿⠛⠙⣹⣿⣲⠀⠑⣣⣾⠋⠃⠁⠀⠀⠀⠀⠀⠀⠈⡀⠀⠀⠀⠀⠈⠀⠐⡌⡭⢀⠼⡿⡟⠀⠙⢿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠉⠀⠀⠀⠁⢯⣹⡀⢸⢏⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⡧⣠⡹⠓⠀⠀⠀⠀⠈⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠋⠁⠀⠀⠀⠀⠀⠀⢸⡸⡀⢘⠋⠀⠀⠀⣀⠀⡀⠀⠀⠀⠐⠀⠀⠀⠀⠀⠠⣤⠀⠀⠀⠇⡚⠃⠀⠀⠀⠀⠀⠀⠀⠀⠉⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠟⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⣸⣷⡀⠨⡄⠂⠀⢰⣿⣿⣿⡿⠿⠛⠛⠛⠛⠿⢻⢿⣹⢻⡏⠁⠐⠒⡑⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠻⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⣿⣿⣿⠿⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⣿⣿⣷⠀⢣⠂⠀⠘⢱⣫⠏⠂⠀⠀⠀⠀⠀⠀⠀⠙⠀⠉⠀⠀⠀⠊⣼⠠⢄⡀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⣿⣿⣿⣿⣿⣿⣿⣿
⣿⣿⣿⣿⣿⣿⣿⠟⠋⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣴⣿⣟⣧⡿⡅⢈⠀⠀⠀⠀⠈⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠈⢕⠔⠀⠟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠙⠻⢿⣿⣿⣿⣿
⣿⣿⣿⡿⠟⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⣯⣿⢮⡵⣳⠄⠘⠂⠀⠀⠀⠀⠀⠀⠀⠀⠐⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢠⠊⠀⢐⠞⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈⠛⢿⣿
⠟⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⠫⣚⠶⣄⡆⢀⠀⠄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠂⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠈
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠙⠚⠥⢆⡠⠀⠘⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠁⠈⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
*/