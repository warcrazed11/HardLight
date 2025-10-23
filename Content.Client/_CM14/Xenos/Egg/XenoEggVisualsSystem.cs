using Content.Shared.CM14.Xenonids.Egg;
using Robust.Client.GameObjects;

namespace Content.Client.CM14.Xenos.Egg;

/// <summary>
/// Client-side visual system that maps XenoEgg state to sprite layer states.
/// </summary>
public sealed class XenoEggVisualsSystem : EntitySystem
{
    [Dependency] private readonly SpriteSystem _spriteSystem = default!;

    public override void Initialize()
    {
        // Apply visuals when the component starts and whenever the replicated state changes.
        SubscribeLocalEvent<XenoEggComponent, ComponentStartup>(OnEggStartup);
        SubscribeLocalEvent<XenoEggComponent, XenoEggStateChangedEvent>(OnEggStateChanged);
    }

    private void OnEggStartup(Entity<XenoEggComponent> ent, ref ComponentStartup args)
    {
        UpdateVisual(ent);
    }

    private void OnEggStateChanged(Entity<XenoEggComponent> ent, ref XenoEggStateChangedEvent args)
    {
        UpdateVisual(ent);
    }

    private void UpdateVisual(Entity<XenoEggComponent> ent)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        // Ensure the layer exists/mapped; prototype maps Base layer to XenoEggLayers.Base
        _spriteSystem.LayerMapReserve((ent, sprite), XenoEggLayers.Base);

        string state = ent.Comp.State switch
        {
            XenoEggState.Item => ent.Comp.ItemState,
            XenoEggState.Growing => ent.Comp.GrowingState,
            XenoEggState.Grown => ent.Comp.GrownState,
            XenoEggState.Opened => ent.Comp.OpenedState,
            _ => ent.Comp.ItemState
        };

        sprite.LayerSetState(XenoEggLayers.Base, state);
    }
}
