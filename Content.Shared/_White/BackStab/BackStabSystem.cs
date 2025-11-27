// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Shared._White.BackStab;

public sealed class BackStabSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public static readonly SoundSpecifier BackstabSound =
        new SoundPathSpecifier("/Audio/_Goobstation/Weapons/Effects/guillotine.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BackStabComponent, MeleeHitEvent>(HandleHit);
    }

    private void HandleHit(Entity<BackStabComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.DamageMultiplier < 1f || !args.IsHit || args.HitEntities.Count != 1)
            return;

        var target = args.HitEntities[0];

        if (!TryBackstab(target, args.User, ent.Comp.Tolerance))
            return;

        var total = args.BaseDamage.GetTotal();

        var damage = total * ent.Comp.DamageMultiplier;

        args.BonusDamage += new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Slash"), damage - total);
    }

    public bool TryBackstab(EntityUid target,
        EntityUid user,
        Angle tolerance,
        bool showPopup = true,
        bool playSound = true,
        bool alwaysBackstabLaying = true)
    {
        if (target == user || !HasComp<MobStateComponent>(target))
            return false;

        if (alwaysBackstabLaying && _standing.IsDown(target))
        {
            BackstabEffects(target, showPopup, playSound);
            return true;
        }

        var xform = Transform(target);
        var userXform = Transform(user);

        // Get direction vectors in world space (X/Y) and convert to System.Numerics.Vector2
        var v1Raw = -_transform.GetWorldRotation(xform).ToWorldVec();
        var v2Raw = _transform.GetWorldPosition(userXform) - _transform.GetWorldPosition(xform);

        var v1 = new Vector2(v1Raw.X, v1Raw.Y);
        var v2 = new Vector2(v2Raw.X, v2Raw.Y);

        if (v1 == Vector2.Zero || v2 == Vector2.Zero)
            return false;

        v1 = Vector2.Normalize(v1);
        v2 = Vector2.Normalize(v2);

        // Use dot product -> acos to get the angle between the vectors (radians)
        var dot = Vector2.Dot(v1, v2);
        if (dot < -1f)
            dot = -1f;
        else if (dot > 1f)
            dot = 1f;

        var angle = MathF.Acos(dot);

        if (angle > tolerance.Theta)
            return false;

        BackstabEffects(target, showPopup, playSound);
        return true;
    }

    private void BackstabEffects(EntityUid target, bool showPopup = true, bool playSound = true)
    {
        if (_net.IsClient)
            return;

        if (showPopup)
            _popup.PopupEntity(Loc.GetString("backstab-message"), target, PopupType.LargeCaution);

        if (playSound)
            _audio.PlayPvs(BackstabSound, target);
    }
}
