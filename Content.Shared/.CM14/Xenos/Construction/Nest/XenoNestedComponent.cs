using Robust.Shared.GameStates;

namespace Content.Shared.CM14.Xenos.Construction.Nest;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(XenoNestSystem))]
public sealed partial class XenoNestedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Nest;

    [DataField, AutoNetworkedField]
    public bool Detached;
}
