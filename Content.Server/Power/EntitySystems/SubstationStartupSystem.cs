using Content.Server.Power.Components;
using Content.Shared.Power;

namespace Content.Server.Power.EntitySystems;

/// <summary>
/// Resets substation battery/power network state on map load to avoid stale map data.
/// </summary>
public sealed class SubstationStartupSystem : EntitySystem
{
    [Dependency] private readonly BatterySystem _batterySystem = default!;

    private static readonly HashSet<string> FullChargeSubstations = new()
    {
        "SubstationBasic",
        "SubstationWallBasic"
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PowerMonitoringDeviceComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, PowerMonitoringDeviceComponent component, MapInitEvent args)
    {
        if (component.Group != PowerMonitoringConsoleGroup.Substation)
            return;

        if (!TryComp(uid, out BatteryComponent? battery))
            return;

        var protoId = MetaData(uid).EntityPrototype?.ID;
        if (protoId == null)
            return;

        if (protoId == "SubstationBasicEmpty")
        {
            if (battery.CurrentCharge > 0)
                _batterySystem.SetCharge(uid, 0, battery);
        }
        else if (FullChargeSubstations.Contains(protoId))
        {
            if (battery.CurrentCharge <= 0)
                _batterySystem.SetCharge(uid, battery.MaxCharge, battery);
        }

        if (TryComp(uid, out PowerNetworkBatteryComponent? netBattery))
        {
            netBattery.LoadingNetworkDemand = 0f;
            netBattery.CurrentSupply = 0f;
            netBattery.CurrentReceiving = 0f;
            netBattery.SupplyRampPosition = 0f;
            netBattery.LastSupply = 0f;
        }
    }
}
