namespace Content.Server._NF.GameRule.Components;

[RegisterComponent, Access(typeof(NFAdventureRuleSystem))]
public sealed partial class NFAdventureRuleComponent : Component
{
    public List<EntityUid> NFPlayerMinds = new();
    public List<EntityUid> CargoDepots = new();
}
