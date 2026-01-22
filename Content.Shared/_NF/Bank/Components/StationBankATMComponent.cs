using Content.Shared.Cargo.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Stacks;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using System.Collections.Generic;
using System.Linq;

namespace Content.Shared._NF.Bank.Components;

[RegisterComponent, NetworkedComponent]

public sealed partial class StationBankATMComponent : Component
{
    [DataField]
    public ProtoId<StackPrototype> CashType = "Credit";

    public static string CashSlotId = "station-bank-ATM-cashSlot";

    [DataField]
    public ItemSlot CashSlot = new();

    [DataField]
    public SectorBankAccount Account = SectorBankAccount.Invalid;

    [DataField]
    public SoundSpecifier ErrorSound =
        new SoundPathSpecifier("/Audio/Effects/Cargo/buzz_sigh.ogg");

    [DataField]
    public SoundSpecifier ConfirmSound =
        new SoundPathSpecifier("/Audio/Effects/Cargo/ping.ogg");
}

public enum SectorBankAccount : byte
{
    Invalid, // No assigned account.
    Cargo,
    Engineering,
    Science,
    Security,
    Service,
    Frontier,
    Nfsd,
    ColSec, // HardLight
    Medical,
}

public static class SectorBankAccountMapping
{
    public static readonly Dictionary<SectorBankAccount, string> ToCargoAccountId = new()
    {
        { SectorBankAccount.Cargo, "Cargo" },
        { SectorBankAccount.Engineering, "Engineering" },
        { SectorBankAccount.Science, "Science" },
        { SectorBankAccount.Security, "Security" },
        { SectorBankAccount.Service, "Service" },
        { SectorBankAccount.Frontier, "Frontier" },
        { SectorBankAccount.Nfsd, "Nfsd" },
        { SectorBankAccount.ColSec, "ColSec" }, // HardLight
        { SectorBankAccount.Medical, "Medical" },
    };

    public static bool TryGetCargoAccountId(SectorBankAccount sector, out string? id)
        => ToCargoAccountId.TryGetValue(sector, out id);

    public static bool TryGetSectorBankAccount(string id, out SectorBankAccount sector)
    {
        foreach (var pair in ToCargoAccountId)
        {
            if (pair.Value == id)
            {
                sector = pair.Key;
                return true;
            }
        }
        sector = SectorBankAccount.Invalid;
        return false;
    }

    public static void SetSyncedBalance(
        StationBankAccountComponent stationBank,
        SectorBankAccount sectorAccount,
        int newBalance)
    {
        if (!TryGetCargoAccountId(sectorAccount, out var protoId) || protoId == null)
            return;

        stationBank.Accounts[protoId] = newBalance;
    }

    public static int GetSyncedBalance(
        StationBankAccountComponent stationBank,
        SectorBankAccount sectorAccount)
    {
        if (!TryGetCargoAccountId(sectorAccount, out var protoId) || protoId == null)
            return 0;

        return stationBank.Accounts.TryGetValue(protoId, out var bal) ? bal : 0;
    }
}
