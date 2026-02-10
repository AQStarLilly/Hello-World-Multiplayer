using Unity.Netcode;
using UnityEngine;

public class TagPlayer : NetworkBehaviour
{
    private ColorController colorController;
    private float tagCooldown = 0.5f;
    private float lastTagTime = -10f;

    public NetworkVariable<bool> IsTagger = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public NetworkVariable<bool> IsFrozen = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );


    private float freezeTimer;

    private void Awake()
    {
        colorController = GetComponentInChildren<ColorController>();
    }

    public override void OnNetworkSpawn()
    {
        IsTagger.OnValueChanged += OnTaggerChanged;
        UpdateColor(IsTagger.Value);
    }

    public override void OnNetworkDespawn()
    {
        IsTagger.OnValueChanged -= OnTaggerChanged;
    }

    private void OnTaggerChanged(bool previous, bool current)
    {
        UpdateColor(current);
    }

    private void UpdateColor(bool isTagger)
    {
        if (colorController == null)
        {
            Debug.LogWarning("TagPlayer: ColorController not found.");
            return;
        }

        colorController.SetColor(isTagger);
    }

    public void SetTagger(bool value)
    {
        if (IsServer)
            IsTagger.Value = value;
    }

    private void Update()
    {
        if (!IsServer) return;

        if (IsFrozen.Value)
        {
            freezeTimer -= Time.deltaTime;
            if (freezeTimer <= 0f)
                IsFrozen.Value = false;
        }
    }

    public void Freeze(float duration)
    {
        if (!IsServer) return;

        IsFrozen.Value = true;
        freezeTimer = duration;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (!IsTagger.Value) return;
        if (IsFrozen.Value) return;

        if (Time.time - lastTagTime < tagCooldown) return;

        if (collision.gameObject.TryGetComponent<TagPlayer>(out var other))
        {
            if (other.IsFrozen.Value) return;

            lastTagTime = Time.time;

            // Transfer tagger role
            SetTagger(false);
            other.SetTagger(true);

            // Freeze the newly tagged player
            other.Freeze(3f);
        }
    }
}
