namespace Content.Shared._Mono.SpaceArtillery;

/// <summary>
/// Marker component for classifying ship-mounted weapon turrets by type.
/// Used by legacy _Mono prototypes (e.g., Ballistic/Plasma/Missile turrets).
/// </summary>
[RegisterComponent]
public sealed partial class ShipGunTypeComponent : Component
{
    /// <summary>
    /// The classification of the ship weapon.
    /// </summary>
    [DataField("shipType")] public ShipWeaponType Type = ShipWeaponType.Ballistic;
}

/// <summary>
/// Weapon type categories referenced by prototype field 'shipType'.
/// </summary>
public enum ShipWeaponType
{
    Ballistic,
    Energy,
    Missile,
}
