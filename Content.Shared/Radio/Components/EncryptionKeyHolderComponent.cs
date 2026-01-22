using Content.Shared.Chat;
using Content.Shared.Tools;
using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set; // HardLight

namespace Content.Shared.Radio.Components;

/// <summary>
///     This component is by entities that can contain encryption keys
/// </summary>
[RegisterComponent]
public sealed partial class EncryptionKeyHolderComponent : Component
{
    /// <summary>
    ///     Whether or not encryption keys can be removed from the headset.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keysUnlocked")]
    public bool KeysUnlocked = true;

    /// <summary>
    ///     The tool required to extract the encryption keys from the headset.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keysExtractionMethod", customTypeSerializer: typeof(PrototypeIdSerializer<ToolQualityPrototype>))]
    public string KeysExtractionMethod = "Screwing";

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keySlots")]
    public int KeySlots = 2;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keyExtractionSound")]
    public SoundSpecifier KeyExtractionSound = new SoundPathSpecifier("/Audio/Items/pistol_magout.ogg");

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("keyInsertionSound")]
    public SoundSpecifier KeyInsertionSound = new SoundPathSpecifier("/Audio/Items/pistol_magin.ogg");

    [ViewVariables]
    public Container KeyContainer = default!;
    public const string KeyContainerName = "key_slots";

    /// <summary>
    ///     Combined set of radio channels provided by all contained keys.
    /// </summary>
    [ViewVariables]
    public HashSet<string> Channels = new();

    /// <summary>
    ///     HardLight: Intrinsic radio channels provided by the device itself (e.g., built-in common on passenger headsets).
    ///     Populated server-side so examine can show all available channels.
    /// </summary>
    [ViewVariables]
    [DataField("intrinsicChannels", customTypeSerializer: typeof(PrototypeIdHashSetSerializer<RadioChannelPrototype>))]
    public HashSet<string> IntrinsicChannels = new();

    /// <summary>
    ///     This is the channel that will be used when using the default/department prefix (<see cref="SharedChatSystem.DefaultChannelKey"/>).
    /// </summary>
    [ViewVariables]
    public string? DefaultChannel;

    /// <summary>
    ///     Goobstation: Whether or not the headset can be examined to see the encryption keys while the keys aren't accessible.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("examineWhileLocked")]
    public bool ExamineWhileLocked = true;
    /// <summary>
    ///     HardLight: Whether or not radio channels are revealed on basic examination.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("showOnExamine")]
    public bool ShowOnExamine = true;

    /// <summary>
    ///     HardLight: Whether to preserve existing channels (e.g., intrinsic radio channels) when managing encryption keys.
    ///     If true, encryption key channels are added to existing channels. If false, existing channels are replaced.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField("preserveExistingChannels")]
    public bool PreserveExistingChannels = false;
}
