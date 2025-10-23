using Robust.Shared.GameStates;

namespace Content.Shared.CM14.NightVision;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NightVisionComponent : Component
{
    [DataField, AutoNetworkedField]
    public NightVisionState State = NightVisionState.Off;
}

public enum NightVisionState
{
    Off = 0,
    On = 1
}
