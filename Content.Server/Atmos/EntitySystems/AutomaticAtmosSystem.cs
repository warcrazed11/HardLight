using Content.Server.Atmos.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Maps;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;

namespace Content.Server.Atmos.EntitySystems;

/// <summary>
/// Handles automatically adding a grid atmosphere to grids that become large enough, allowing players to build shuttles
/// with a sealed atmosphere from scratch.
/// </summary>
public sealed class AutomaticAtmosSystem : EntitySystem
{
    [Dependency] private readonly ITileDefinitionManager _tileDefinitionManager = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TileChangedEvent>(OnTileChanged);
    }

    private void OnTileChanged(ref TileChangedEvent ev)
    {
        // Only if an atmos-holding tile has been added or removed.
        // Note: these calls involve tile lookups; avoid extra work by early-outing when no relevant change.
        foreach (var change in ev.Changes)
        {
            var oldSpace = change.OldTile.IsSpace(_tileDefinitionManager);
            var newSpace = change.NewTile.IsSpace(_tileDefinitionManager);

            if (!((oldSpace && !newSpace) || (!oldSpace && newSpace)))
                continue;

            if (_atmosphereSystem.HasAtmosphere(ev.Entity))
                continue;

            if (!TryComp<PhysicsComponent>(ev.Entity, out var physics))
                continue;

            // Estimate tile count via mass to decide when to add atmosphere.
            if (physics.Mass / ShuttleSystem.TileMassMultiplier >= 7.0f)
            {
                AddComp<GridAtmosphereComponent>(ev.Entity);
                Log.Info($"Giving grid {ev.Entity} GridAtmosphereComponent.");
                break; // Added once per event is enough.
            }
        }
        // It's not super important to remove it should the grid become too small again.
        // If explosions ever gain the ability to outright shatter grids, do rethink this.
    }
}
