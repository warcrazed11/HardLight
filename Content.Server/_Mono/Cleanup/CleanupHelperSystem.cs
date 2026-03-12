using System.Numerics;
using Robust.Server.Player;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Server._Mono.Cleanup;

/// <summary>
/// Utility helpers for cleanup systems to check whether players or grids are near a coordinate.
/// </summary>
public sealed class CleanupHelperSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    public bool HasNearbyPlayers(EntityCoordinates coordinates, float maxDistance)
    {
        if (maxDistance <= 0f)
            return false;

        var mapCoords = _xform.ToMapCoordinates(coordinates);
        var mapId = mapCoords.MapId;
        if (mapId == MapId.Nullspace)
            return false;

        var position = mapCoords.Position;
        var maxDistanceSq = maxDistance * maxDistance;

        foreach (var session in _players.Sessions)
        {
            var player = session.AttachedEntity;
            if (player is not { Valid: true })
                continue;

            if (!TryComp<TransformComponent>(player.Value, out var xform))
                continue;

            if (xform.MapID != mapId)
                continue;

            var playerPos = _xform.GetWorldPosition(xform);
            if (Vector2.DistanceSquared(position, playerPos) <= maxDistanceSq)
                return true;
        }

        return false;
    }

    public bool HasNearbyGrids(EntityCoordinates coordinates, float maxDistance)
    {
        if (maxDistance <= 0f)
            return false;

        var mapCoords = _xform.ToMapCoordinates(coordinates);
        var mapId = mapCoords.MapId;
        if (mapId == MapId.Nullspace)
            return false;

        var position = mapCoords.Position;
        var maxDistanceSq = maxDistance * maxDistance;

        var grids = EntityQueryEnumerator<MapGridComponent, TransformComponent>();
        while (grids.MoveNext(out _, out _, out var gridXform))
        {
            if (gridXform.MapID != mapId)
                continue;

            var gridPos = _xform.GetWorldPosition(gridXform);
            if (Vector2.DistanceSquared(position, gridPos) <= maxDistanceSq)
                return true;
        }

        return false;
    }
}
