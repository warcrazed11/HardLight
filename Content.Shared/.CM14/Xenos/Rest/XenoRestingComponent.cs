using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.CM14.Xenos.Rest;

[RegisterComponent, NetworkedComponent]
[Access(typeof(XenoRestSystem))]
public sealed partial class XenoRestingComponent : Component
{
	// Used server-side to process regen roughly once per second while resting.
	[DataField("nextTick", customTypeSerializer: typeof(TimeOffsetSerializer))]
	public TimeSpan NextTick;
}
