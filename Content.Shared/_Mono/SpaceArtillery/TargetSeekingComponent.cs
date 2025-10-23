using Robust.Shared.Utility;

namespace Content.Shared._Mono.SpaceArtillery;

/// <summary>
/// Marker + data component for homing projectiles referenced by _Mono prototypes.
/// Server-side behavior can be added later; for now, prototypes load without errors.
/// </summary>
[RegisterComponent]
public sealed partial class TargetSeekingComponent : Component
{
    [DataField("acceleration")] public float Acceleration = 0f;
    [DataField("detectionRange")] public float DetectionRange = 0f;
    [DataField("scanArc")] public float ScanArc = 0f;
    [DataField("launchSpeed")] public float LaunchSpeed = 0f;
    [DataField("maxSpeed")] public float MaxSpeed = 0f;
}
