using System;
using Content.Shared.Actions;
using Content.Shared.Popups;
using Content.Shared.Access.Components;
using Content.Shared.CM14.Xenos.Evolution;
using Content.Shared.CM14.Xenos.Construction;
using Content.Shared.Mind;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared.CM14.Xenos;

public sealed class XenoSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _action = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<XenoComponent, MapInitEvent>(OnXenoMapInit);
        SubscribeLocalEvent<XenoComponent, EntityUnpausedEvent>(OnXenoUnpaused);
        SubscribeLocalEvent<XenoComponent, XenoOpenEvolutionsEvent>(OnXenoEvolve);
        SubscribeLocalEvent<XenoComponent, EvolveBuiMessage>(OnXenoEvolveBui);
        SubscribeLocalEvent<XenoComponent, GetAccessTagsEvent>(OnXenoGetAdditionalAccess);
    }

    private void OnXenoMapInit(Entity<XenoComponent> ent, ref MapInitEvent args)
    {
        Log.Info($"[Xeno] ({(_net.IsServer ? "server" : "client")}) MapInit {ToPrettyString(ent)} actionIds={ent.Comp.ActionIds.Count}");
        if (_net.IsServer)
        {
            // Server-authoritative action registration (replicated to clients)
            foreach (var actionId in ent.Comp.ActionIds)
            {
                if (!ent.Comp.Actions.ContainsKey(actionId) &&
                    _action.AddAction(ent, actionId) is { } newAction)
                {
                    ent.Comp.Actions[actionId] = newAction;
                }
            }

            // Ensure key actions are properly configured server-side.
            // - Plant Weeds: always has an event instance and is raised on user.
            // - Choose Structure: ensure it's raised on user so the shared handler gets the event.
            // - Secrete Structure: ensure it is a world target action raised on user as well.
            if (ent.Comp.Actions.TryGetValue("ActionXenoPlantWeeds", out var weedsAction))
            {
                if (TryComp<InstantActionComponent>(weedsAction, out var instant))
                {
                    instant.Event ??= new XenoPlantWeedsEvent();
                    instant.RaiseOnUser = true;
                    instant.RaiseOnAction = false;
                    Dirty(weedsAction, instant);
                    _action.SetEnabled(weedsAction, true);
                }
            }

            if (ent.Comp.Actions.TryGetValue("ActionXenoChooseStructure", out var chooseAction))
            {
                if (TryComp<InstantActionComponent>(chooseAction, out var instant))
                {
                    instant.Event ??= new Content.Shared.CM14.Xenos.Construction.Events.XenoChooseStructureActionEvent();
                    instant.RaiseOnUser = true;
                    instant.RaiseOnAction = false;
                    instant.CheckCanInteract = false;
                    instant.CheckConsciousness = true;
                    Dirty(chooseAction, instant);
                }
            }

            if (ent.Comp.Actions.TryGetValue("ActionXenoSecreteStructure", out var secreteAction))
            {
                if (TryComp<WorldTargetActionComponent>(secreteAction, out var wta))
                {
                    wta.Event ??= new Content.Shared.CM14.Xenos.Construction.Events.XenoSecreteStructureEvent();
                    wta.RaiseOnUser = true;
                    wta.RaiseOnAction = false;
                    wta.CheckCanInteract = false;
                    wta.CheckConsciousness = true;
                    // Critical: disable access/LOS gating and align target range with the xeno's build range.
                    // Otherwise ValidateWorldTarget can fail before our own tile validation runs, leading to handled=false.
                    wta.CheckCanAccess = false;
                    wta.Range = ent.Comp.BuildRange;
                    Dirty(secreteAction, wta);
                }
            }

            // Ensure a sane default build choice so secrete never has a null selection.
            if (ent.Comp.BuildChoice == null)
            {
                EntProtoId defaultChoice = "WallXenoResin";
                if (ent.Comp.CanBuild.Count > 0)
                {
                    if (!ent.Comp.CanBuild.Contains(defaultChoice))
                        defaultChoice = ent.Comp.CanBuild[0];

                    ent.Comp.BuildChoice = defaultChoice;
                    Dirty(ent);

                    // Notify actions that a selection exists (helps icons/tooltips sync up).
                    foreach (var (_, actionUid) in ent.Comp.Actions)
                    {
                        var chosenEv = new Content.Shared.CM14.Xenos.Construction.Events.XenoConstructionChosenEvent(defaultChoice);
                        RaiseLocalEvent(actionUid, ref chosenEv);
                    }
                }
            }

            Log.Info($"[Xeno] (server) Actions registered={ent.Comp.Actions.Count} for {ToPrettyString(ent)}");
        }

        // Evolution action: prefer an existing evolve action from ActionIds to avoid duplicates.
        if (_net.IsServer && ent.Comp.EvolvesTo.Count > 0)
        {
            // Prefer ActionXenoEvolve60 if present, otherwise use configured EvolveActionId
            EntityUid? evolveAction = null;
            if (ent.Comp.Actions.TryGetValue("ActionXenoEvolve60", out var evo60))
                evolveAction = evo60;
            else if (ent.Comp.Actions.TryGetValue(ent.Comp.EvolveActionId, out var evo))
                evolveAction = evo;
            else
                _action.AddAction(ent, ref evolveAction, ent.Comp.EvolveActionId);

            ent.Comp.EvolveAction = evolveAction;

            // Only set cooldown here if the action doesn't have a specialized cooldown component.
            if (evolveAction != null && !HasComp<CM14.Xenos.Evolution.XenoEvolveActionComponent>(evolveAction.Value))
            {
                _action.SetCooldown(evolveAction, _timing.CurTime, _timing.CurTime + ent.Comp.EvolveIn);
            }
        }
    }

    private void OnXenoUnpaused(Entity<XenoComponent> ent, ref EntityUnpausedEvent args)
    {
        ent.Comp.NextPlasmaRegenTime += args.PausedTime;
    }
    private void OnXenoGetAdditionalAccess(Entity<XenoComponent> ent, ref GetAccessTagsEvent args)
    {
        args.Tags.UnionWith(ent.Comp.AccessLevels);
    }
    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<XenoComponent>();
        var time = _timing.CurTime;

        while (query.MoveNext(out var uid, out var xeno))
        {
            if (time < xeno.NextPlasmaRegenTime)
                continue;

            xeno.Plasma += xeno.PlasmaRegen;
            xeno.NextPlasmaRegenTime = time + xeno.PlasmaRegenCooldown;
            Dirty(uid, xeno);
        }
    }

    public bool HasPlasma(Entity<XenoComponent> xeno, int plasma)
    {
        return xeno.Comp.Plasma >= plasma;
    }

    public bool TryRemovePlasmaPopup(Entity<XenoComponent> xeno, int plasma)
    {
        if (!HasPlasma(xeno, plasma))
        {
            _popup.PopupClient(Loc.GetString("cm-xeno-not-enough-plasma"), xeno, xeno);
            return false;
        }

        RemovePlasma(xeno, plasma);
        return true;
    }

    public void RemovePlasma(Entity<XenoComponent> xeno, int plasma)
    {
        xeno.Comp.Plasma = Math.Max(xeno.Comp.Plasma - plasma, 0);
        Dirty(xeno);
        if (xeno.Comp.EvolvesTo.Count == 0)
            return;

        // Ensure evolve action exists and set cooldown if needed (but avoid overriding specialized component cooldowns)
        if (xeno.Comp.EvolvesTo.Count > 0)
        {
            EntityUid? evolveAction = null;
            if (xeno.Comp.Actions.TryGetValue("ActionXenoEvolve60", out var evo60))
                evolveAction = evo60;
            else if (xeno.Comp.Actions.TryGetValue(xeno.Comp.EvolveActionId, out var evo))
                evolveAction = evo;
            else
                _action.AddAction(xeno, ref evolveAction, xeno.Comp.EvolveActionId);

            xeno.Comp.EvolveAction = evolveAction;

            if (evolveAction != null && !HasComp<CM14.Xenos.Evolution.XenoEvolveActionComponent>(evolveAction.Value))
                _action.SetCooldown(evolveAction, _timing.CurTime, _timing.CurTime + xeno.Comp.EvolveIn);
        }
    }

    private void OnXenoEvolve(Entity<XenoComponent> ent, ref XenoOpenEvolutionsEvent args)
    {
        if (_net.IsClient)
            return;

        // Require an actor to show UI. If no actor or UI fails to open, fall back to auto-evolve to first option.
        if (TryComp(ent, out ActorComponent? actor))
        {
            if (_ui.TryOpenUi(ent.Owner, XenoEvolutionUIKey.Key, actor.Owner))
                return;
        }

        // Fallback: no UI available; auto-evolve to the first available option if any.
        if (ent.Comp.EvolvesTo.Count > 0)
        {
            var evolution = Spawn(ent.Comp.EvolvesTo[0], _transform.GetMoverCoordinates(ent.Owner));

            // Transfer mind if one exists (for player xenos), otherwise just delete (for AI xenos)
            if (_mind.TryGetMind(ent, out var mindId, out _))
            {
                _mind.TransferTo(mindId, evolution);
                _mind.UnVisit(mindId);
            }

            Del(ent.Owner);
        }
    }

    private void OnXenoEvolveBui(Entity<XenoComponent> ent, ref EvolveBuiMessage args)
    {
        if (!_mind.TryGetMind(ent, out var mindId, out _))
            return;

        var choices = ent.Comp.EvolvesTo.Count;
        if (args.Choice >= choices || args.Choice < 0)
        {
            Log.Warning($"User {ToPrettyString(args.Actor)} sent an out of bounds evolution choice: {args.Choice}. Choices: {choices}");
            return;
        }

        var evolution = Spawn(ent.Comp.EvolvesTo[args.Choice], _transform.GetMoverCoordinates(ent.Owner));
        _mind.TransferTo(mindId, evolution);
        _mind.UnVisit(mindId);
        Del(ent.Owner);

        if (TryComp(ent, out ActorComponent? actor))
            _ui.CloseUi(ent.Owner, XenoEvolutionUIKey.Key, actor.PlayerSession);
    }
}
