using Content.Server.Radio.Components;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;

namespace Content.Server._EinsteinEngines.Radio;

public sealed class IntrinsicRadioKeySystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IntrinsicRadioTransmitterComponent, EncryptionChannelsChangedEvent>(OnTransmitterChannelsChanged);
        SubscribeLocalEvent<ActiveRadioComponent, EncryptionChannelsChangedEvent>(OnReceiverChannelsChanged);
    }

    private void OnTransmitterChannelsChanged(EntityUid uid, IntrinsicRadioTransmitterComponent component, EncryptionChannelsChangedEvent args)
    {
        UpdateTransmitterChannels(uid, component, args.Component); // HardLight
    }

    private void OnReceiverChannelsChanged(EntityUid uid, ActiveRadioComponent component, EncryptionChannelsChangedEvent args)
    // HardLight start: Refactored to make intrinsic frequencies work with the EncryptionKeyHolderComponent.
    {
        UpdateChannels(uid, args.Component, ref component.Channels, component.IntrinsicChannels);
    }

    private void UpdateTransmitterChannels(EntityUid uid, IntrinsicRadioTransmitterComponent transmitter, EncryptionKeyHolderComponent keyHolderComp)
    {
        transmitter.Channels.Clear();
        transmitter.Channels.UnionWith(transmitter.IntrinsicChannels);
        transmitter.Channels.UnionWith(keyHolderComp.Channels);
    }

    private void UpdateChannels(EntityUid _, EncryptionKeyHolderComponent keyHolderComp, ref HashSet<string> channels, HashSet<string>? intrinsicChannels = null)
    {
        // Always rebuild from scratch to prevent key channels from lingering after removal
        channels.Clear();

        // Start with intrinsic channels
        if (intrinsicChannels != null)
            channels.UnionWith(intrinsicChannels);

        // Add channels from encryption keys
        channels.UnionWith(keyHolderComp.Channels);
    }
    // HardLight end
}
