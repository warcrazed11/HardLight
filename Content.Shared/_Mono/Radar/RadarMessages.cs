// SPDX-FileCopyrightText: 2025 Ark
// SPDX-FileCopyrightText: 2025 Ilya246
// SPDX-FileCopyrightText: 2025 ark1368
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Mono.Radar;

[Serializable, NetSerializable]
public enum RadarBlipShape
{
    Circle,
    Square,
    GridAlignedBox,
    Triangle,
    Star,
    Diamond,
    Hexagon,
    Arrow,
    Ring
}

[Serializable, NetSerializable]
public sealed class GiveBlipsEvent : EntityEventArgs
{
    /// <summary>
    /// Palette of blip configs, basically an int->config map.
    /// </summary>
    public readonly List<BlipConfig> ConfigPalette;

    /// <summary>
    /// Blips are now (position, velocity, scale, color, shape).
    /// </summary>
    public readonly List<BlipNetData> Blips;

    /// <summary>
    /// Hitscan lines to display on the radar as (grid entity, start position, end position, thickness, color).
    /// If grid entity is null, positions are world-space; otherwise they are grid-local.
    /// </summary>
    public readonly List<HitscanNetData> HitscanLines;

    public GiveBlipsEvent(List<BlipNetData> blips)
    {
        ConfigPalette = new List<BlipConfig>();
        Blips = blips;
        HitscanLines = new List<HitscanNetData>();
    }

    public GiveBlipsEvent(
        List<BlipNetData> blips,
        List<(NetEntity? Grid, Vector2 Start, Vector2 End, float Thickness, Color Color)> hitscans)
    {
        ConfigPalette = new List<BlipConfig>();
        Blips = blips;
        HitscanLines = new List<HitscanNetData>(hitscans.Count);
        foreach (var (grid, start, end, thickness, color) in hitscans)
        {
            HitscanLines.Add(new HitscanNetData(grid, start, end, thickness, color));
        }
    }

    public GiveBlipsEvent(
        List<BlipConfig> configPalette,
        List<BlipNetData> blips,
        List<HitscanNetData> hitscans)
    {
        ConfigPalette = configPalette;
        Blips = blips;
        HitscanLines = hitscans;
    }
}

[Serializable, NetSerializable]
public sealed class RequestBlipsEvent : EntityEventArgs
{
    public NetEntity Radar;
    public RequestBlipsEvent(NetEntity radar)
    {
        Radar = radar;
    }
}

[Serializable, NetSerializable]
public sealed class BlipRemovalEvent : EntityEventArgs
{
    public NetEntity NetBlipUid;

    public BlipRemovalEvent(NetEntity netBlipUid)
    {
        NetBlipUid = netBlipUid;
    }
}

[Serializable, NetSerializable]
public record struct BlipNetData
(
    NetEntity Uid,
    NetCoordinates Position,
    Vector2 Vel,
    Angle Rotation,
    ushort ConfigIndex,
    ushort? OnGridConfigIndex
);

[Serializable, NetSerializable]
public record struct HitscanNetData(NetEntity? Grid, Vector2 Start, Vector2 End, float Thickness, Color Color);

[Serializable, NetSerializable, DataDefinition]
public partial record struct BlipConfig
{
    [DataField]
    public Box2 Bounds = new Box2(-0.5f, -0.5f, 0.5f, 0.5f);

    [DataField]
    public Color Color = Color.OrangeRed;

    [DataField]
    public RadarBlipShape Shape = RadarBlipShape.Circle;

    [DataField]
    public bool RespectZoom = false;

    [DataField]
    public bool Rotate = false;

    public BlipConfig() { }
}
