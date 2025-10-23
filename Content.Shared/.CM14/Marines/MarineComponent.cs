using Robust.Shared.GameStates;

namespace Content.Shared.CM14.Marines;

/// <summary>
/// Minimal marker component for CM14 marines used by Xeno interactions (devour targeting).
/// This is a stub sufficient for Shared usage; server/client can extend if needed.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MarineComponent : Component
{
}
