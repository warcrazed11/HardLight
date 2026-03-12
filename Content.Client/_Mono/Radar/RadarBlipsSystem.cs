using System.Numerics;
using Content.Shared._Mono.Radar;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Client._Mono.Radar;

public sealed partial class RadarBlipsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _map = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private const double BlipStaleSeconds = 3.0;
    private const float MaxBlipRenderDistance = 256f;
    private static readonly List<(NetEntity? Grid, Vector2 Start, Vector2 End, float Thickness, Color Color)> EmptyHitscanList = new();
    private TimeSpan _lastRequestTime = TimeSpan.Zero;
    private static readonly TimeSpan RequestThrottle = TimeSpan.FromMilliseconds(500);

    private TimeSpan _lastUpdatedTime;
    private List<BlipNetData> _blips = new();
    private List<HitscanNetData> _hitscans = new();
    private List<BlipConfig> _configPalette = new();
    private Vector2 _radarWorldPosition;

    // cached results to avoid allocating on every draw/frame
    private readonly List<BlipData> _cachedBlipData = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<GiveBlipsEvent>(HandleReceiveBlips);
        SubscribeNetworkEvent<BlipRemovalEvent>(RemoveBlip);
    }

    private void HandleReceiveBlips(GiveBlipsEvent ev, EntitySessionEventArgs args)
    {
        _configPalette = ev.ConfigPalette;
        _blips = ev.Blips;
        _hitscans = ev.HitscanLines;
        _lastUpdatedTime = _timing.CurTime;
    }

    private void RemoveBlip(BlipRemovalEvent args)
    {
        var blipid = _blips.FirstOrDefault(x => x.Uid == args.NetBlipUid);
        _blips.Remove(blipid);
    }

    public void RequestBlips(EntityUid console)
    {
        // Only request if we have a valid console
        if (!Exists(console))
            return;

        // Add request throttling to avoid network spam
        if (_timing.CurTime - _lastRequestTime < RequestThrottle)
            return;

        _lastRequestTime = _timing.CurTime;

        // Cache the radar position for distance culling
        _radarWorldPosition = _xform.GetWorldPosition(console);
        var netConsole = GetNetEntity(console);
        var ev = new RequestBlipsEvent(netConsole);
        RaiseNetworkEvent(ev);
    }

    /// <summary>
    /// Gets the current blips as world positions with their scale, color and shape.
    /// </summary>
    public List<BlipData> GetCurrentBlips()
    {
        // clear the cache and bail early if the data is stale
        _cachedBlipData.Clear();
        if (_timing.CurTime.TotalSeconds - _lastUpdatedTime.TotalSeconds > BlipStaleSeconds)
            return _cachedBlipData;

        // populate the cached list instead of allocating a new one each frame
        foreach (var blip in _blips)
        {
            var coord = GetCoordinates(blip.Position);

            if (!coord.IsValid(EntityManager))
                continue;

            var predictedPos = new EntityCoordinates(coord.EntityId, coord.Position + blip.Vel * (float)(_timing.CurTime - _lastUpdatedTime).TotalSeconds);

            var predictedMap = _xform.ToMapCoordinates(predictedPos);

            var config = _configPalette[blip.ConfigIndex];
            var rotation = blip.Rotation;
            // hijack our shape if we're on a grid and we want to do that
            if (_map.TryFindGridAt(predictedMap, out var grid, out _) && grid != EntityUid.Invalid)
            {
                if (blip.OnGridConfigIndex is { } gridIdx)
                    config = _configPalette[gridIdx];
                rotation += Transform(grid).LocalRotation;
            }
            var maybeGrid = grid != EntityUid.Invalid ? grid : (EntityUid?)null;

            _cachedBlipData.Add(new(blip.Uid, predictedPos, rotation, maybeGrid, config));
        }

        return _cachedBlipData;
    }

    /// <summary>
    /// Gets the hitscan lines to be rendered on the radar
    /// </summary>
    public List<HitscanNetData> GetHitscanLines()
    {
        if (_timing.CurTime.TotalSeconds - _lastUpdatedTime.TotalSeconds > BlipStaleSeconds)
            return new();

        return _hitscans;
    }

    /// <summary>
    /// Gets the hitscan lines to be rendered on the radar
    /// </summary>
    public List<(Vector2 Start, Vector2 End, float Thickness, Color Color)> GetWorldHitscanLines()
    {
        if (_timing.CurTime.TotalSeconds - _lastUpdatedTime.TotalSeconds > BlipStaleSeconds)
            return new List<(Vector2, Vector2, float, Color)>();

        var result = new List<(Vector2, Vector2, float, Color)>(_hitscans.Count);

        foreach (var hitscan in _hitscans)
        {
            Vector2 worldStart, worldEnd;

            // If no grid, positions are already in world coordinates
            if (hitscan.Grid == null)
            {
                worldStart = hitscan.Start;
                worldEnd = hitscan.End;

                // Distance culling - check if either end of the line is in range
                var startDist = Vector2.DistanceSquared(worldStart, _radarWorldPosition);
                var endDist = Vector2.DistanceSquared(worldEnd, _radarWorldPosition);

                if (startDist > MaxBlipRenderDistance * MaxBlipRenderDistance &&
                    endDist > MaxBlipRenderDistance * MaxBlipRenderDistance)
                    continue;

                result.Add((worldStart, worldEnd, hitscan.Thickness, hitscan.Color));
                continue;
            }

            // If grid exists, transform from grid-local to world coordinates
            if (TryGetEntity(hitscan.Grid, out var gridEntity))
            {
                // Transform the grid-local positions to world positions
                var worldPos = _xform.GetWorldPosition(gridEntity.Value);
                var gridRot = _xform.GetWorldRotation(gridEntity.Value);

                // Rotate the local positions by grid rotation and add grid position
                var rotatedLocalStart = gridRot.RotateVec(hitscan.Start);
                var rotatedLocalEnd = gridRot.RotateVec(hitscan.End);

                worldStart = worldPos + rotatedLocalStart;
                worldEnd = worldPos + rotatedLocalEnd;

                // Distance culling - check if either end of the line is in range
                var startDist = Vector2.DistanceSquared(worldStart, _radarWorldPosition);
                var endDist = Vector2.DistanceSquared(worldEnd, _radarWorldPosition);

                if (startDist > MaxBlipRenderDistance * MaxBlipRenderDistance &&
                    endDist > MaxBlipRenderDistance * MaxBlipRenderDistance)
                    continue;

                result.Add((worldStart, worldEnd, hitscan.Thickness, hitscan.Color));
            }
        }

        return result;
    }

    /// <summary>
    /// Gets the raw hitscan data which includes grid information for more accurate rendering.
    /// </summary>
    public List<(NetEntity? Grid, Vector2 Start, Vector2 End, float Thickness, Color Color)> GetRawHitscanLines()
    {
        if (_timing.CurTime.TotalSeconds - _lastUpdatedTime.TotalSeconds > BlipStaleSeconds)
            return EmptyHitscanList;

        if (_hitscans.Count == 0)
            return EmptyHitscanList;

        var filteredHitscans = new List<(NetEntity? Grid, Vector2 Start, Vector2 End, float Thickness, Color Color)>(_hitscans.Count);

        foreach (var hitscan in _hitscans)
        {
            // For non-grid hitscans, do direct distance check
            if (hitscan.Grid == null)
            {
                // Check if either endpoint is in range
                var startDist = Vector2.DistanceSquared(hitscan.Start, _radarWorldPosition);
                var endDist = Vector2.DistanceSquared(hitscan.End, _radarWorldPosition);

                if (startDist <= MaxBlipRenderDistance * MaxBlipRenderDistance ||
                    endDist <= MaxBlipRenderDistance * MaxBlipRenderDistance)
                {
                    filteredHitscans.Add((hitscan.Grid, hitscan.Start, hitscan.End, hitscan.Thickness, hitscan.Color));
                }
                continue;
            }

            // For grid hitscans, transform to world space for distance check
            if (TryGetEntity(hitscan.Grid, out var gridEntity))
            {
                var worldPos = _xform.GetWorldPosition(gridEntity.Value);
                var gridRot = _xform.GetWorldRotation(gridEntity.Value);

                var rotatedLocalStart = gridRot.RotateVec(hitscan.Start);
                var rotatedLocalEnd = gridRot.RotateVec(hitscan.End);

                var worldStart = worldPos + rotatedLocalStart;
                var worldEnd = worldPos + rotatedLocalEnd;

                // Check if either endpoint is in range
                var startDist = Vector2.DistanceSquared(worldStart, _radarWorldPosition);
                var endDist = Vector2.DistanceSquared(worldEnd, _radarWorldPosition);

                if (startDist <= MaxBlipRenderDistance * MaxBlipRenderDistance ||
                    endDist <= MaxBlipRenderDistance * MaxBlipRenderDistance)
                {
                    filteredHitscans.Add((hitscan.Grid, hitscan.Start, hitscan.End, hitscan.Thickness, hitscan.Color));
                }
            }
        }

        return filteredHitscans;
    }
}

public record struct BlipData
(
    NetEntity NetUid,
    EntityCoordinates Position,
    Angle Rotation,
    EntityUid? GridUid,
    BlipConfig Config
);
