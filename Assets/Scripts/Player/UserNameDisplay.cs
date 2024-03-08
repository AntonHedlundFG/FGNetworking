using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class UserNameDisplay : NetworkBehaviour
{
    [SerializeField] private TextMesh _textMesh;
    private NetworkVariable<FixedString64Bytes> _userNameNetVar = new NetworkVariable<FixedString64Bytes>();
    private Vector3 _positionOffset;

    public override void OnNetworkSpawn()
    {
        if (!TryGetComponent<TextMesh>(out _textMesh)) 
        {
            enabled = false;
            return;
        }

        // Store text position to avoid it rotating when the player does.
        _positionOffset = transform.localPosition;

        // Bind NetworkVariable to update text mesh when changed
        _userNameNetVar.OnValueChanged += OnTextValueChanged;

        if (IsServer)
        {
            //For servers, update network variable based on owner's username.
            UserData userData = SavedClientInformationManager.GetUserData(OwnerClientId);
            _userNameNetVar.Value = userData.userName;
        } 
        else
        {
            //For clients, check if the network variable already has an initial value, which will not trigger OnValueChanged.
            FixedString64Bytes startingValue = _userNameNetVar.Value;
            _textMesh.text = startingValue.ConvertToString();
        }
        
    }

    private void OnTextValueChanged(FixedString64Bytes previous, FixedString64Bytes current)
    {
        _textMesh.text = current.ConvertToString();
    }

    private void Update()
    {
        //Make sure text mesh does not rotate and move around as the parent does.
        transform.position = transform.parent.position + _positionOffset;
        transform.rotation = Quaternion.identity;
    }
}
