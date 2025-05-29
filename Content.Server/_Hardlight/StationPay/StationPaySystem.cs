using System.Diagnostics.CodeAnalysis;
using Content.Server._NF.Bank;
using Content.Server.GameTicking;
using Content.Server.Roles.Jobs;
using Content.Shared._NF.Bank.Components;
using Content.Shared._NF.Roles.Components;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;

namespace Content.Server._Hardlight.StationPay;

[UsedImplicitly]
public sealed class StationPaySystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly BankSystem _bank = default!;
    [Dependency] private readonly JobSystem _jobs = default!;

    private const int PayoutDelay = 10; // 3600;

    // map of {Mind.OwnedEntity: lastPayoutTime} where lastPayoutTime was the round duration at time of payout
    // sorted in ascending order
    private readonly Dictionary<ProtoId<JobPrototype>, int> _jobPayoutRates = new();
    private OrderedDictionary<EntityUid, int> _scheduledPayouts = new();

    public override void Initialize()
    {
        base.Initialize();

        foreach (var proto in _prototypeManager.EnumeratePrototypes<StationPayPrototype>())
        {
            _jobPayoutRates[proto.JobProto] = proto.PayPerHour;
            Console.WriteLine("loaded prototype: " + proto.JobProto.Id + " at " + proto.PayPerHour);
        }

        SubscribeLocalEvent<GameRunLevelChangedEvent>(OnRunLevelChanged);
        SubscribeLocalEvent<RoleAddedEvent>(OnRoleAddedEvent);
        SubscribeLocalEvent<RoleRemovedEvent>(OnRoleRemovedEvent);

        /*
         * TODO: account for disconnecting players
         *
         * when someone disconnects add them to a removal list with a timestamp 10 minutes in the future
         *
         * after that time they are removed from the scheduledpayout dict
         *
         * if they reconnect before that time they are removed from the disconnect tracker
         *
         * this allows for a grace period where if you happen to disconnect right before the hour you still get paid
         *
         * and if you disconnect and reconnect you still get paid
         *
         * we also don't have to do any complex bookkeeping
         */
        // SubscribeLocalEvent<MindRemovedMessage>(OnMindRemoved);
    }

    private void OnRunLevelChanged(GameRunLevelChangedEvent ev)
    {
        // restartroundnow command
        if (ev.Old == GameRunLevel.InRound && ev.New == GameRunLevel.PreRoundLobby)
            OnRoundEnd();
        else if (ev.New == GameRunLevel.PostRound)
            OnRoundEnd();
    }

    private void OnRoundEnd()
    {
        // TODO: payout people who have worked less than payoutdelay
    }

    private bool GetJobPay(
        [NotNullWhen(true)] EntityUid? uid,
        [NotNullWhen(true)] out ProtoId<JobPrototype>? jobPrototype)
    {
        jobPrototype = null;
        if (TryComp<JobTrackingComponent>(uid, out var jtc)
            && jtc.Job is {} job
            && _jobPayoutRates.ContainsKey(job))
        {
            jobPrototype = job;
        }

        return jobPrototype != null;
    }

    private void OnRoleAddedEvent(RoleAddedEvent args)
    {
        var uid = args.Mind.OwnedEntity;

        if (uid == null
            || !TryComp<BankAccountComponent>(uid, out _)
            || !GetJobPay(uid, out _)
        )
            return;

        _scheduledPayouts[(EntityUid)uid] = (int)_gameTicker.RoundDuration().TotalSeconds + PayoutDelay;
    }

    private void OnRoleRemovedEvent(RoleRemovedEvent args)
    {
        if(args.Mind.OwnedEntity != null)
            _scheduledPayouts.Remove((EntityUid)args.Mind.OwnedEntity);
    }

    private void PayoutFor(EntityUid uid, int secondsWorked)
    {
        if (!_scheduledPayouts.ContainsKey(uid))
            return;

        if (!GetJobPay(uid, out var job))
            return;

        var employedTime = (int)(secondsWorked / (double)PayoutDelay);
        var pay = _jobPayoutRates[(ProtoId<JobPrototype>)job];
        var amount = employedTime * pay;
        Log.Info($"Paying entity {uid} ${amount} for {secondsWorked} seconds of work as {job.Value.Id}.");

        if (!_bank.TryBankDeposit(uid, amount))
            Log.Error("Failed to deposit station pay for uid: " + uid);
    }

    public override void Update(float frameTime)
    {
        var now = (int)_gameTicker.RoundDuration().TotalSeconds;
        var updated = new Lazy<OrderedDictionary<EntityUid, int>>(() => new OrderedDictionary<EntityUid, int>(_scheduledPayouts));

        foreach (var (uid,lastPayout) in _scheduledPayouts)
        {
            if (lastPayout + PayoutDelay >= now)
                break;

            var dict = updated.Value;
            dict.Remove(uid);
            // schedule their next payout relative to when their last payout should've been in case we're
            // paying out late due to shenanigans with the round clock like e.g. 10x timescale where the
            // round clock is advancing faster than realtime
            dict.Insert(dict.Count, uid, lastPayout + PayoutDelay*2);

            PayoutFor(uid, PayoutDelay);
        }

        if (updated.IsValueCreated)
            _scheduledPayouts = updated.Value;

        base.Update(frameTime);
    }
}
