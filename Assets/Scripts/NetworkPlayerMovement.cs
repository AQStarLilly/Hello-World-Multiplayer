using Unity.Netcode;
using UnityEngine;

public class NetworkPlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 8f;

    [Header("Sprint")]
    [SerializeField] private float sprintMultiplier = 1.6f;
    [SerializeField] private float sprintDuration = 2f;
    [SerializeField] private float sprintCooldown = 3f;

    private bool sprintRequested;
    private float sprintTimeRemaining;
    private float sprintCooldownRemaining;

    [SerializeField] private float sendRate = 1f / 30f;
    private float _sendTimer;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        var tagPlayer = GetComponent<TagPlayer>();
        if (tagPlayer.IsFrozen.Value)
            return;

        float x = 0f;
        float z = 0f;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RequestSprintServerRpc();
        }

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

    private void FixedUpdate()
    {
        if (!IsServer) return;

        if (sprintTimeRemaining > 0f)
        {
            sprintTimeRemaining -= Time.fixedDeltaTime;
            if (sprintTimeRemaining <= 0f)
            {
                sprintCooldownRemaining = sprintCooldown;
            }
        }
        else if (sprintCooldownRemaining > 0f)
        {
            sprintCooldownRemaining -= Time.fixedDeltaTime;
        }
    }

    [ServerRpc]
    private void RequestSprintServerRpc()
    {
        if (sprintCooldownRemaining > 0f) return;
        if (sprintTimeRemaining > 0f) return;

        sprintTimeRemaining = sprintDuration;
    }

    [ServerRpc]
    private void SubmitMoveInputServerRpc(Vector2 input)
    {
        var tagPlayer = GetComponent<TagPlayer>();
        if (tagPlayer.IsFrozen.Value) return;

        float speed = moveSpeed;

        if (sprintTimeRemaining > 0f)
            speed *= sprintMultiplier;

        Vector3 dir = new Vector3(input.x, 0f, input.y);
        Vector3 targetPos = rb.position + dir * speed * sendRate;

        rb.MovePosition(targetPos);
    }
}
