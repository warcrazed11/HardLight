using Robust.Shared.GameObjects;

namespace Content.Server._Mono.Cleanup;

/// <summary>
/// Marker component that opts an entity out of _Mono cleanup systems.
/// </summary>
[RegisterComponent]
public sealed partial class CleanupImmuneComponent : Component
{
}
