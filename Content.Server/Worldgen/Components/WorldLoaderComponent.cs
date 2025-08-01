﻿using Content.Server.Worldgen.Systems;

namespace Content.Server.Worldgen.Components;

/// <summary>
///     This is used for allowing some objects to load the game world.
/// </summary>
[RegisterComponent]
[Access(typeof(WorldControllerSystem))]
public sealed partial class WorldLoaderComponent : Component
{
    /// <summary>
    ///     The radius in which the loader loads the world.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] [DataField("radius")]
    public int Radius = 64;

    /// <summary>
    ///     Frontier: if true, this loader is disabled, and will not be used
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)] [DataField]
    public bool Disabled;
}

