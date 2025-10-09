using Robust.Shared.GameObjects; // MetaDataComponent lives here
using Robust.Shared.Log;

namespace Content.Server.Diagnostics;

/// <summary>
/// Temporary stub to satisfy missing file referenced by build logs.
/// If profiling functionality is reintroduced, replace this with real implementation.
/// </summary>
public sealed class EntitySpawnProfilerSystem : EntitySystem
{
    private ISawmill _log = default!;

    public override void Initialize()
    {
        base.Initialize();
        _log = Logger.GetSawmill("spawnprof");
        // Subscribe to entity added / removed events for MetaDataComponent here if profiling is reinstated.
    }

    public override void Shutdown()
    {
        base.Shutdown();
        // Unsubscribe from any events if subscriptions are added in Initialize.
    }
}
