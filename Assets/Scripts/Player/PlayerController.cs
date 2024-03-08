using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInput;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : NetworkBehaviour, IPlayerActions
{
    private PlayerInput _playerInput;
    private Vector2 _moveInput = new();
    private Vector2 _cursorLocation;

    private Transform _shipTransform;
    private Rigidbody2D _rb;

    private Transform turretPivotTransform;


    public UnityAction<bool> onFireEvent;

    [Header("Settings")]
    [SerializeField] private float movementSpeed = 5f;
    [SerializeField] private float shipRotationSpeed = 100f;
    [SerializeField] private float turretRotationSpeed = 4f;

    // -- CHEATING --
    [Header("Cheating")]
    [SerializeField][Range(1.0f, 5.0f)] private float _positionCheatingAcceptance = 1.3f; // Accepts movements at 1.1 times the movement speed to avoid false positives for cheating protection.
    [SerializeField][Range(1.0f, 5.0f)] private float _cheatTeleportDistance = 3.0f; //Determines teleport distance for cheating input (Space bar) 
    [SerializeField][Range(0.5f, 2.0f)] private float _acceptableLagDuration = 0.5f;
    private float _lastKnownPositionTime;
    private Vector3 _lastKnownPosition;
    private bool _bShouldPerformCheatTeleport = false;
    // --------------

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            //Initialize cheating detection variables
            _lastKnownPosition = transform.position;
            _lastKnownPositionTime = Time.time;
        }

        if (!IsOwner) return;

        if (_playerInput == null)
        {
            _playerInput = new();
            _playerInput.Player.SetCallbacks(this);
        }
        _playerInput.Player.Enable();

        _rb = GetComponent<Rigidbody2D>();
        _shipTransform = transform;
        turretPivotTransform = transform.Find("PivotTurret");
        if (turretPivotTransform == null) Debug.LogError("PivotTurret is not found", gameObject);
    }

    public void OnFire(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            onFireEvent.Invoke(true);
        }
        else if (context.canceled)
        {
            onFireEvent.Invoke(false);
        }
    }

    public void OnMove(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<Vector2>();
    }

    private void FixedUpdate()
    {
        if (IsServer)
            TeleportingCheatDetection();

        if (!IsOwner) return;
        _rb.velocity = transform.up * _moveInput.y * movementSpeed;
        _rb.MoveRotation(_rb.rotation + _moveInput.x * -shipRotationSpeed * Time.fixedDeltaTime);

        if (_bShouldPerformCheatTeleport)
        {
            transform.position += transform.up * _cheatTeleportDistance;
            _bShouldPerformCheatTeleport = false;
        }
    }
    private void LateUpdate()
    {
        if (!IsOwner) return;
        Vector2 screenToWorldPosition = Camera.main.ScreenToWorldPoint(_cursorLocation);
        Vector2 targetDirection = new Vector2(screenToWorldPosition.x - turretPivotTransform.position.x, screenToWorldPosition.y - turretPivotTransform.position.y).normalized;
        Vector2 currentDirection = Vector2.Lerp(turretPivotTransform.up, targetDirection, Time.deltaTime * turretRotationSpeed);
        turretPivotTransform.up = currentDirection;
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        _cursorLocation = context.ReadValue<Vector2>();
    }

    // When pressing Space bar, the user cheats by teleporting forward a short distance.
    public void OnCheatTeleport(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (context.started)
            _bShouldPerformCheatTeleport = true;
    }


    private void TeleportingCheatDetection()
    {
        float travelTime = Time.time - _lastKnownPositionTime;
        float travelLength = (_lastKnownPosition - transform.position).magnitude;

        if (_lastKnownPosition == transform.position) // We have not received a new position from the client.
        {
            //We might still be waiting for the transform to replicate from client. If we've waited long enough, we move through and update last known position and time.
            if (travelTime < _acceptableLagDuration) return;
        }
        else
        {
            // If the distance travelled since the last update is greater than what the movementSpeed would allow, the user is cheating. 
            // _positionCheatingAcceptance is slightly higher than 1.0 to avoid false positives.
            if (travelLength > travelTime * movementSpeed * _positionCheatingAcceptance)
            {
                if (IsOwnedByServer)
                    Debug.Log("The server can cheat if it wants to!");
                else
                {
                    string cheatingReason = "User used teleporting cheat";
                    HandleCheater(cheatingReason);
                }
                
            } 
        }

        _lastKnownPosition = transform.position;
        _lastKnownPositionTime = Time.time;
    }

    private void HandleCheater(string cheatingReason)
    {
        
        ServerSingelton server = ServerSingelton.GetInstance();
        if (server && server.serverManager)
        {
            server.serverManager.KickPlayer(OwnerClientId, cheatingReason);
            return;
        }
        HostSingelton host = HostSingelton.GetInstance();
        if (host)
        {
            host.hostManager.KickPlayer(OwnerClientId, cheatingReason);
        }
        
    }

}
