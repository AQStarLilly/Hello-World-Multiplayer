using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4.5f;

    [SerializeField] private float sendRate = 1f / 30f;
    private float _sendTimer;

    private void Update()
    {
        if (!IsOwner) return;
        float x = 0f;
        float z = 0f;

        if (Input.GetKey(KeyCode.LeftArrow)) x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) x += 1f;
        if (Input.GetKey(KeyCode.UpArrow)) z += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) z -= 1f;

        Vector2 input = new Vector2(x, z);
        if(input.sqrMagnitude > 1f) input.Normalize();

        _sendTimer -= Time.deltaTime;
        if(_sendTimer <= 0f)
        {
            _sendTimer = sendRate;
            SubmitMoveInputServerRpc(input);
        }
    }

    [ServerRpc]
    private void SubmitMoveInputServerRpc(Vector2 input, ServerRpcParams rpcParams = default)
    {
        Vector3 dir = new Vector3(input.x, 0f, input.y);
        transform.position += dir * moveSpeed * sendRate;
    }
}
