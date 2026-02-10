using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._HL.Sound;

/// <summary>
/// Plays sounds when the entity enters critical or dead state.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class EmitSoundOnCritDeathComponent : Component
{
    /// <summary>
    /// Sound to play when the entity enters critical state.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? CritSound;

    /// <summary>
    /// Sound to play when the entity dies.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? DeathSound;
}
