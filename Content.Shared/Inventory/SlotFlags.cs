using Robust.Shared.Serialization;

namespace Content.Shared.Inventory;

/// <summary>
///     Defines what slot types an item can fit into.
/// </summary>
[Serializable, NetSerializable]
[Flags]
public enum SlotFlags
{
    NONE = 0,
    PREVENTEQUIP = 1 << 0,
    HEAD = 1 << 1,
    EYES = 1 << 2,
    EARS = 1 << 3,
    MASK = 1 << 4,
    OUTERCLOTHING = 1 << 5,
    INNERCLOTHING = 1 << 6,
    NECK = 1 << 7,
    BACK = 1 << 8,
    BELT = 1 << 9,
    GLOVES = 1 << 10,
    IDCARD = 1 << 11,
    POCKET = 1 << 12,
     LEGS = 1 << 13, // Frontier: unused Hardlight: for some reason something wants this now?
    ACCESSORY = 1 << 16,
    ACCESSORYALT = 1 << 17,
    FEET = 1 << 14,
    SUITSTORAGE = 1 << 15,
    WALLET = 1 << 18, // Frontier: using an unused slot, redefine to a new bit if/when it's used (goodbye ushort)
    All = ~NONE,

    WITHOUT_POCKET = All & ~POCKET
}
