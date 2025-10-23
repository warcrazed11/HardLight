using Content.Shared.CM14.Xenos.Construction;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.CM14.Xenos.Construction.Nest;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedXenoConstructionSystem))]
public sealed partial class XenoWeedableComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public EntProtoId Spawn;

    [DataField, AutoNetworkedField]
    public EntityUid? Entity;
}
