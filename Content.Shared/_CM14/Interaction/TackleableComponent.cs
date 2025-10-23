using Robust.Shared.GameStates;

namespace Content.Shared.CM14.Interaction;

[RegisterComponent, NetworkedComponent, ComponentProtoName("Tackleable")]
public sealed partial class TackleableComponent : Component
{
    // Placeholder values to match expected YAML usage in CM14.
    [DataField]
    public bool Enabled = true;

    // Stun duration applied on a successful tackle (seconds or ticks depending on system usage).
    [DataField("stun")]
    public float Stun = 0f;
}
