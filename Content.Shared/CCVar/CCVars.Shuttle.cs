﻿using Content.Shared.Administration;
using Content.Shared.CCVar.CVarAccess;
using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Delay for auto-orientation. Used for people arriving via arrivals.
    /// </summary>
    public static readonly CVarDef<double> AutoOrientDelay =
        CVarDef.Create("shuttle.auto_orient_delay", 2.0, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     If true then the camera will match the grid / map and is unchangeable.
    ///     - When traversing grids it will snap to 0 degrees rotation.
    ///     False means the player has control over the camera rotation.
    ///     - When traversing grids it will snap to the nearest cardinal which will generally be imperceptible.
    /// </summary>
    public static readonly CVarDef<bool> CameraRotationLocked =
        CVarDef.Create("shuttle.camera_rotation_locked", false, CVar.REPLICATED);

    /// <summary>
    ///     Whether the arrivals terminal should be on a planet map.
    /// </summary>
    public static readonly CVarDef<bool> ArrivalsPlanet =
        CVarDef.Create("shuttle.arrivals_planet", false, CVar.SERVERONLY);

    /// <summary>
    ///     Whether the arrivals shuttle is enabled.
    /// </summary>
    public static readonly CVarDef<bool> ArrivalsShuttles =
        CVarDef.Create("shuttle.arrivals", false, CVar.SERVERONLY); // Frontier: false

    /// <summary>
    ///     The map to use for the arrivals station.
    /// </summary>
    public static readonly CVarDef<string> ArrivalsMap =
        CVarDef.Create("shuttle.arrivals_map", "/Maps/Misc/terminal.yml", CVar.SERVERONLY);

    /// <summary>
    ///     Cooldown between arrivals departures. This should be longer than the FTL time or it will double cycle.
    /// </summary>
    public static readonly CVarDef<float> ArrivalsCooldown =
        CVarDef.Create("shuttle.arrivals_cooldown", 30f, CVar.SERVERONLY);

    /// <summary>
    ///     Are players allowed to return on the arrivals shuttle.
    /// </summary>
    public static readonly CVarDef<bool> ArrivalsReturns =
        CVarDef.Create("shuttle.arrivals_returns", true, CVar.SERVERONLY);

    /// <summary>
    ///     Should all players who spawn at arrivals have godmode until they leave the map?
    /// </summary>
    public static readonly CVarDef<bool> GodmodeArrivals =
        CVarDef.Create("shuttle.godmode_arrivals", false, CVar.SERVERONLY);

    /// <summary>
    ///     If a grid is split then hide any smaller ones under this mass (kg) from the map.
    ///     This is useful to avoid split grids spamming out labels.
    /// </summary>
    public static readonly CVarDef<int> HideSplitGridsUnder =
        CVarDef.Create("shuttle.hide_split_grids_under", 30, CVar.SERVERONLY);

    /// <summary>
    ///     Whether to automatically spawn escape shuttles.
    /// </summary>
    public static readonly CVarDef<bool> GridFill =
        CVarDef.Create("shuttle.grid_fill", false, CVar.SERVERONLY); // Frontier: false

    /// <summary>
    ///     Whether to automatically preloading grids by GridPreloaderSystem
    /// </summary>
    public static readonly CVarDef<bool> PreloadGrids =
        CVarDef.Create("shuttle.preload_grids", false, CVar.SERVERONLY);

    /// <summary>
    ///     How long the warmup time before FTL start should be.
    /// </summary>
    public static readonly CVarDef<float> FTLStartupTime =
        CVarDef.Create("shuttle.startup_time", 5.5f, CVar.SERVERONLY);

    /// <summary>
    ///     How long a shuttle spends in FTL.
    /// </summary>
    public static readonly CVarDef<float> FTLTravelTime =
        CVarDef.Create("shuttle.travel_time", 10f, CVar.SERVERONLY);

    /// <summary>
    ///     How long the final stage of FTL before arrival should be.
    /// </summary>
    public static readonly CVarDef<float> FTLArrivalTime =
        CVarDef.Create("shuttle.arrival_time", 5f, CVar.SERVERONLY);

    /// <summary>
    ///     How much time needs to pass before a shuttle can FTL again.
    /// </summary>
    public static readonly CVarDef<float> FTLCooldown =
        CVarDef.Create("shuttle.cooldown", 2f, CVar.SERVERONLY);

    /// <summary>
    ///     The maximum <see cref="PhysicsComponent.Mass"/> a grid can have before it becomes unable to FTL.
    ///     Any value equal to or less than zero will disable this check.
    /// </summary>
    public static readonly CVarDef<float> FTLMassLimit =
        CVarDef.Create("shuttle.mass_limit", 300f, CVar.SERVERONLY);

    /// <summary>
    ///     How long to knock down entities for if they aren't buckled when FTL starts and stops.
    /// </summary>
    public static readonly CVarDef<float> HyperspaceKnockdownTime =
        CVarDef.Create("shuttle.hyperspace_knockdown_time", 5f, CVar.SERVERONLY);

    /// <summary>
    ///     Is the emergency shuttle allowed to be early launched.
    /// </summary>
    public static readonly CVarDef<bool> EmergencyEarlyLaunchAllowed =
        CVarDef.Create("shuttle.emergency_early_launch_allowed", false, CVar.SERVERONLY);

    /// <summary>
    ///     How long the emergency shuttle remains docked with the station, in seconds.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleDockTime =
        CVarDef.Create("shuttle.emergency_dock_time", 180f, CVar.SERVERONLY); // Frontier: 180f<300f

    /// <summary>
    ///     If the emergency shuttle can't dock at a priority port, the dock time will be multiplied with this value.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleDockTimeMultiplierOtherDock =
        CVarDef.Create("shuttle.emergency_dock_time_multiplier_other_dock", 1.6667f, CVar.SERVERONLY);

    /// <summary>
    ///     If the emergency shuttle can't dock at all, the dock time will be multiplied with this value.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleDockTimeMultiplierNoDock =
        CVarDef.Create("shuttle.emergency_dock_time_multiplier_no_dock", 2f, CVar.SERVERONLY);

    /// <summary>
    ///     How long after the console is authorized for the shuttle to early launch.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleAuthorizeTime =
        CVarDef.Create("shuttle.emergency_authorize_time", 10f, CVar.SERVERONLY);

    /// <summary>
    ///     The minimum time for the emergency shuttle to arrive at Colcomm.
    ///     Actual minimum travel time cannot be less than <see cref="ShuttleSystem.DefaultArrivalTime"/>
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleMinTransitTime =
        CVarDef.Create("shuttle.emergency_transit_time_min", 60f, CVar.SERVERONLY); // Frontier: 60f<300f

    /// <summary>
    ///     The maximum time for the emergency shuttle to arrive at Colcomm.
    /// </summary>
    public static readonly CVarDef<float> EmergencyShuttleMaxTransitTime =
        CVarDef.Create("shuttle.emergency_transit_time_max", 180f, CVar.SERVERONLY); // Frontier: 180f<600f

    /// <summary>
    ///     Whether the emergency shuttle is enabled or should the round just end.
    /// </summary>
    public static readonly CVarDef<bool> EmergencyShuttleEnabled =
        CVarDef.Create("shuttle.emergency", true, CVar.SERVERONLY); // Frontier: false

    /// <summary>
    ///     The percentage of time passed from the initial call to when the shuttle can no longer be recalled.
    ///     ex. a call time of 10min and turning point of 0.5 means the shuttle cannot be recalled after 5 minutes.
    /// </summary>
    public static readonly CVarDef<float> EmergencyRecallTurningPoint =
        CVarDef.Create("shuttle.recall_turning_point", 0.5f, CVar.SERVERONLY);

    /// <summary>
    ///     Time in minutes after round start to auto-call the shuttle. Set to zero to disable.
    /// </summary>
    [CVarControl(AdminFlags.Server | AdminFlags.Mapping, min: 0, max: int.MaxValue)]
    public static readonly CVarDef<int> EmergencyShuttleAutoCallTime =
        CVarDef.Create("shuttle.auto_call_time", 240, CVar.SERVERONLY); // Frontier: 90<360

    /// <summary>
    ///     Time in minutes after the round was extended (by recalling the shuttle) to call
    ///     the shuttle again.
    /// </summary>
    public static readonly CVarDef<int> EmergencyShuttleAutoCallExtensionTime =
        CVarDef.Create("shuttle.auto_call_extension_time", 60, CVar.SERVERONLY);

    /// <summary>
    ///     Impulse multiplier for player interactions that move grids (other than shuttle thrusters, gyroscopes and grid collisons).
    ///     At the moment this only affects the pushback in SpraySystem.
    ///     A higher value means grids have a lower effective mass and therefore will get pushed stronger.
    ///     A value of 0 will disable pushback.
    ///     The default has been chosen such that a one tile grid roughly equals 2/3 Urist masses.
    ///     TODO: Make grid mass a sane number so we can get rid of this.
    ///         At the moment they have a very low mass of roughly 0.48 kg per tile independent of any walls or anchored objects on them.
    /// </summary>
    public static readonly CVarDef<float> GridImpulseMultiplier =
        CVarDef.Create("shuttle.grid_impulse_multiplier", 0.01f, CVar.SERVERONLY);
}
