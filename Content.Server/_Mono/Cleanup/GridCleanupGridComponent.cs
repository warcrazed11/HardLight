using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Server._Mono.Cleanup;

[RegisterComponent]
public sealed partial class GridCleanupGridComponent : Component
{
    [DataField]
    public bool IgnoreIFF = false;

    [DataField]
    public bool IgnorePowered = false;

    [DataField]
    public bool IgnorePrice = false;

    [DataField]
    public float? DistanceOverride;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer))]
    public TimeSpan CleanupAccumulator = TimeSpan.Zero;

    [DataField]
    public float CleanupAcceleration = 1f;
}
