using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

/// <summary>
///     Component that requires the wearer to have a specific tag to equip this item.
/// </summary>
[NetworkedComponent]
[RegisterComponent]
public sealed partial class RequireTagToWearComponent : Component
{
    /// <summary>
    ///     The tag that the wearer must have to wear this item.
    /// </summary>
    [DataField("tag", required: true)]
    public string Tag { get; set; } = string.Empty;

    /// <summary>
    ///     The localization ID for the message shown when someone without the tag tries to wear this item.
    /// </summary>
    [DataField("denialMessage")]
    public string DenialMessage { get; set; } = "require-tag-to-wear-denial";
}
