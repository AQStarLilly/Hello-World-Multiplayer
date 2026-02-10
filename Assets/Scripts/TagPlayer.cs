using Unity.Netcode;
using UnityEngine;

public class TagPlayer : NetworkBehaviour
{
    public NetworkVariable<bool> IsTagger = new NetworkVariable<bool>();
    public NetworkVariable<bool> IsFrozen = new NetworkVariable<bool>();

    private float freezeTimer;

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

        if (collision.gameObject.TryGetComponent<TagPlayer>(out var other))
        {
            if (!other.IsFrozen.Value)
            {
                other.Freeze(3f); // freeze for 3 seconds
            }
        }
    }
}
