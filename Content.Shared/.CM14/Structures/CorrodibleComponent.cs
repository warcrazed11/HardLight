using Robust.Shared.GameStates;

namespace Content.Shared.CM14.Structures;

[RegisterComponent, NetworkedComponent, ComponentProtoName("Corrodible")]
public sealed partial class CorrodibleComponent : Component
{
    // Placeholder for future functionality.
    [DataField("isCorrodible")]
    public bool IsCorrodible = true;
}
