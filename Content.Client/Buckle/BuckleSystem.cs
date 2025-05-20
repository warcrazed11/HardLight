using Content.Client.Rotation;
using Content.Shared.Buckle;
using Content.Shared.Buckle.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Rotation;
using Robust.Client.GameObjects;
using Robust.Client.Player;
using Robust.Client.Graphics;
using Robust.Shared.Timing;

namespace Content.Client.Buckle;

internal sealed class BuckleSystem : SharedBuckleSystem
{
    [Dependency] private readonly RotationVisualizerSystem _rotationVisualizerSystem = default!;
    [Dependency] private readonly IEyeManager _eye = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly IPlayerManager _player = default!; // Floof
    [Dependency] private readonly IGameTiming _timing = default!; // Floof

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BuckleComponent, AppearanceChangeEvent>(OnAppearanceChange);
        SubscribeLocalEvent<StrapComponent, MoveEvent>(OnStrapMoveEvent);
        SubscribeLocalEvent<BuckleComponent, BuckledEvent>(OnBuckledEvent);
        SubscribeLocalEvent<BuckleComponent, UnbuckledEvent>(OnUnbuckledEvent);
        SubscribeLocalEvent<BuckleComponent, AttemptMobCollideEvent>(OnMobCollide);
    }

    private void OnMobCollide(Entity<BuckleComponent> ent, ref AttemptMobCollideEvent args)
    {
        if (ent.Comp.Buckled)
        {
            args.Cancelled = true;
        }
    }

    private void OnStrapMoveEvent(EntityUid uid, StrapComponent component, ref MoveEvent args)
    {
        if (args.NewRotation == args.OldRotation)
            return;

        // Frontier: maintain sprite order
        if (component.MaintainSpriteLayers)
            return;
        // End Frontier

        if (!TryComp<SpriteComponent>(uid, out var strapSprite))
            return;

        var angle = _xformSystem.GetWorldRotation(uid) + _eye.CurrentEye.Rotation;
        var isNorth = angle.GetCardinalDir() == Direction.North;

        foreach (var buckledEntity in component.BuckledEntities)
        {
            if (!TryComp<BuckleComponent>(buckledEntity, out var buckle))
                continue;

            if (!TryComp<SpriteComponent>(buckledEntity, out var buckledSprite))
                continue;

            if (isNorth)
            {
                buckle.OriginalDrawDepth ??= buckledSprite.DrawDepth;
                buckledSprite.DrawDepth = strapSprite.DrawDepth - 1;
            }
            else if (buckle.OriginalDrawDepth.HasValue)
            {
                buckledSprite.DrawDepth = buckle.OriginalDrawDepth.Value;
                buckle.OriginalDrawDepth = null;
            }
        }
    }

    /// <summary>
    /// Lower the draw depth of the buckled entity without needing for the strap entity to rotate/move.
    /// Only do so when the entity is facing screen-local north
    /// </summary>
    private void OnBuckledEvent(Entity<BuckleComponent> ent, ref BuckledEvent args)
    {
        if (!TryComp<SpriteComponent>(args.Strap, out var strapSprite))
            return;

        if (!TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        // Frontier: maintain sprite order
        if (args.Strap.Comp.MaintainSpriteLayers)
            return;
        // End Frontier

        var angle = _xformSystem.GetWorldRotation(args.Strap) + _eye.CurrentEye.Rotation;
        if (angle.GetCardinalDir() != Direction.North)
            return;

        ent.Comp.OriginalDrawDepth ??= buckledSprite.DrawDepth;
        buckledSprite.DrawDepth = strapSprite.DrawDepth - 1;
    }

    /// <summary>
    /// Was the draw depth of the buckled entity lowered? Reset it upon unbuckling.
    /// </summary>
    private void OnUnbuckledEvent(Entity<BuckleComponent> ent, ref UnbuckledEvent args)
    {
        if (!TryComp<SpriteComponent>(ent.Owner, out var buckledSprite))
            return;

        if (!ent.Comp.OriginalDrawDepth.HasValue)
            return;

        buckledSprite.DrawDepth = ent.Comp.OriginalDrawDepth.Value;
        ent.Comp.OriginalDrawDepth = null;
    }

    private void OnAppearanceChange(EntityUid uid, BuckleComponent component, ref AppearanceChangeEvent args)
    {
        // EE changes
        if (!TryComp<RotationVisualsComponent>(uid, out var rotVisuals)
            || !Appearance.TryGetData<bool>(uid, BuckleVisuals.Buckled, out var buckled, args.Component)
            || !buckled || args.Sprite == null)
            return;

        // Animate strapping yourself to something at a given angle
        // TODO: Dump this when buckle is better
        _rotationVisualizerSystem.AnimateSpriteRotation(uid, args.Sprite, rotVisuals.HorizontalRotation, 0.125f);
    }

    // Floof section - method for getting the direction of an entity perceived by the local player
    private Direction GetEntityOrientation(EntityUid uid)
    {
        var xform = Transform(uid);
        var ownRotation = xform.LocalRotation;
        var eyeRotation =
            TryComp<EyeComponent>(_player.LocalEntity, out var eye) ? eye.Eye.Rotation : Angle.Zero;

        // This is TOTALLY dumb, but the eye stores camera rotation relative to the WORLD, so we need to convert it to local rotation as well
        // Cameras are also relative to grids (NOT direct parents), so we cannot just GetWorldRotation of the entity or something similar.
        if (xform.GridUid is { Valid: true } grid)
            eyeRotation += _xform.GetWorldRotation(grid);

        // Note: we subtract instead of adding because e.g. rotating an eye +90° visually rotates all entities in vision by -90°
        return (ownRotation + eyeRotation).GetCardinalDir();
    }
    // Floof section end
}
