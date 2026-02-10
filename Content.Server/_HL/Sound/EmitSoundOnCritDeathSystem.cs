using Content.Shared._HL.Sound;
using Content.Shared.Mobs;
using Robust.Shared.Audio.Systems;

namespace Content.Server._HL.Sound;

/// <summary>
/// Handles playing sounds when an entity enters critical or dead state.
/// </summary>
public sealed class EmitSoundOnCritDeathSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmitSoundOnCritDeathComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, EmitSoundOnCritDeathComponent component, MobStateChangedEvent args)
    {
        // Don't replay sounds if we're already in the same state
        if (args.OldMobState == args.NewMobState)
            return;

        switch (args.NewMobState)
        {
            case MobState.Critical when component.CritSound != null:
                _audio.PlayPvs(component.CritSound, uid);
                break;
            case MobState.Dead when component.DeathSound != null:
                _audio.PlayPvs(component.DeathSound, uid);
                break;
        }
    }
}
