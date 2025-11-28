using Content.Shared.Tag;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// Used to denote compatibility with <see cref="UpgradeableGunComponent"/>. Does not contain explicit behavior.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(GunUpgradeSystem))]
public sealed partial class GunUpgradeComponent : Component
{
    /// <summary>
    /// Tags used to ensure mutually exclusive upgrades and duplicates are not stacked.
    /// </summary>
    [DataField]
    public List<ProtoId<TagPrototype>> Tags = new();

    /// <summary>
    /// Markup added to the gun on examine to display the upgrades.
    /// </summary>
    [DataField]
    public LocId ExamineText;

    [DataField]
    public int CapacityCost = 30; // By default drains 30% of the capacity.

    /// <summary>
    /// If true, only one such upgrade can be inserted into a gun.
    /// </summary>
    [DataField]
    public bool Unique;
}
