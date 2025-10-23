using Robust.Shared.GameStates;

namespace Content.Shared.CM14.Interaction;

[RegisterComponent, NetworkedComponent, ComponentProtoName("Huggable")]
public sealed partial class HuggableComponent : Component
{
    // Placeholder for future expansion (cooldowns, counts, etc.)
    [DataField]
    public bool Enabled = true;
}
