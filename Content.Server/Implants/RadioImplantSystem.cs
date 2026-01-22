using Content.Server.Radio.Components;
using Content.Shared.Implants;
using Content.Shared.Implants.Components;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Robust.Shared.Containers;

namespace Content.Server.Implants;

public sealed class RadioImplantSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RadioImplantComponent, ImplantImplantedEvent>(OnImplantImplanted);
        SubscribeLocalEvent<RadioImplantComponent, EntGotRemovedFromContainerMessage>(OnRemove);
    }

    /// <summary>
    /// If implanted with a radio implant, installs the necessary intrinsic radio components
    /// </summary>
    private void OnImplantImplanted(Entity<RadioImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        if (args.Implanted == null)
            return;

        var activeRadio = EnsureComp<ActiveRadioComponent>(args.Implanted.Value);
        foreach (var channel in ent.Comp.RadioChannels)
        {
            if (activeRadio.IntrinsicChannels.Add(channel))
                ent.Comp.ActiveAddedChannels.Add(channel);
        }

        EnsureComp<IntrinsicRadioReceiverComponent>(args.Implanted.Value);

        var intrinsicRadioTransmitter = EnsureComp<IntrinsicRadioTransmitterComponent>(args.Implanted.Value);
        foreach (var channel in ent.Comp.RadioChannels)
        {
            if (intrinsicRadioTransmitter.IntrinsicChannels.Add(channel))
                ent.Comp.TransmitterAddedChannels.Add(channel);
        }

        if (TryComp<EncryptionKeyHolderComponent>(args.Implanted.Value, out var keyHolder))
        {
            foreach (var channel in ent.Comp.RadioChannels)
            {
                if (keyHolder.IntrinsicChannels.Add(channel))
                    ent.Comp.HolderAddedChannels.Add(channel);
            }

            RaiseLocalEvent(args.Implanted.Value, new EncryptionChannelsChangedEvent(keyHolder));
        }

        SyncActiveRadioChannels(args.Implanted.Value, activeRadio);
        SyncTransmitterChannels(args.Implanted.Value, intrinsicRadioTransmitter);
    }

    /// <summary>
    /// Removes intrinsic radio components once the Radio Implant is removed
    /// </summary>
    private void OnRemove(Entity<RadioImplantComponent> ent, ref EntGotRemovedFromContainerMessage args)
    {
        if (TryComp<ActiveRadioComponent>(args.Container.Owner, out var activeRadioComponent))
        {
            foreach (var channel in ent.Comp.ActiveAddedChannels)
            {
                activeRadioComponent.IntrinsicChannels.Remove(channel);
            }
            ent.Comp.ActiveAddedChannels.Clear();

            SyncActiveRadioChannels(args.Container.Owner, activeRadioComponent);

            if (activeRadioComponent.Channels.Count == 0 && activeRadioComponent.IntrinsicChannels.Count == 0)
            {
                RemCompDeferred<ActiveRadioComponent>(args.Container.Owner);
            }
        }

        if (!TryComp<IntrinsicRadioTransmitterComponent>(args.Container.Owner, out var radioTransmitterComponent))
            return;

        foreach (var channel in ent.Comp.TransmitterAddedChannels)
        {
            radioTransmitterComponent.IntrinsicChannels.Remove(channel);
        }
        ent.Comp.TransmitterAddedChannels.Clear();

        SyncTransmitterChannels(args.Container.Owner, radioTransmitterComponent);

        if (radioTransmitterComponent.Channels.Count == 0 && radioTransmitterComponent.IntrinsicChannels.Count == 0)
        {
            RemCompDeferred<IntrinsicRadioTransmitterComponent>(args.Container.Owner);
        }

        if (TryComp<IntrinsicRadioReceiverComponent>(args.Container.Owner, out _)
            && !HasComp<ActiveRadioComponent>(args.Container.Owner))
        {
            RemCompDeferred<IntrinsicRadioReceiverComponent>(args.Container.Owner);
        }

        if (TryComp<EncryptionKeyHolderComponent>(args.Container.Owner, out var keyHolder))
        {
            foreach (var channel in ent.Comp.HolderAddedChannels)
            {
                keyHolder.IntrinsicChannels.Remove(channel);
            }
            ent.Comp.HolderAddedChannels.Clear();

            RaiseLocalEvent(args.Container.Owner, new EncryptionChannelsChangedEvent(keyHolder));
        }
    }

    private void SyncActiveRadioChannels(EntityUid uid, ActiveRadioComponent activeRadioComponent)
    {
        if (TryComp<EncryptionKeyHolderComponent>(uid, out var keyHolder))
        {
            RaiseLocalEvent(uid, new EncryptionChannelsChangedEvent(keyHolder));
            return;
        }

        activeRadioComponent.Channels.Clear();
        activeRadioComponent.Channels.UnionWith(activeRadioComponent.IntrinsicChannels);
    }

    private void SyncTransmitterChannels(EntityUid uid, IntrinsicRadioTransmitterComponent transmitterComponent)
    {
        if (TryComp<EncryptionKeyHolderComponent>(uid, out var keyHolder))
        {
            RaiseLocalEvent(uid, new EncryptionChannelsChangedEvent(keyHolder));
            return;
        }

        transmitterComponent.Channels.Clear();
        transmitterComponent.Channels.UnionWith(transmitterComponent.IntrinsicChannels);
    }
}
