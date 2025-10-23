using Robust.Shared.GameStates;

namespace Content.Shared.CM14.Status;

[RegisterComponent, NetworkedComponent, ComponentProtoName("Flashable")]
public sealed partial class FlashableComponent : Component
{
    // Placeholder for future functionality.
    [DataField]
    public bool Enabled = true;
}
