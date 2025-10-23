using Robust.Shared.Prototypes;

namespace Content.Shared.CM14.Xenos.Construction.Events;

[ByRefEvent]
public readonly record struct XenoConstructionChosenEvent(EntProtoId Choice);
