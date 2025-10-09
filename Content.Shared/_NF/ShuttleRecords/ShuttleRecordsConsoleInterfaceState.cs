using Robust.Shared.Serialization;

// Suppress naming style rule for the _NF namespace prefix (project convention)
#pragma warning disable IDE1006
namespace Content.Shared._NF.ShuttleRecords;

[Serializable, NetSerializable]
public sealed class DockedGridEntry
{
    public NetEntity Grid { get; set; }
    public string Name { get; set; } = string.Empty;

    public DockedGridEntry()
    {
    }

    public DockedGridEntry(NetEntity grid, string name)
    {
        Grid = grid;
        Name = name;
    }
}

[Serializable, NetSerializable]
public sealed class ShuttleRecordsConsoleInterfaceState(
    List<ShuttleRecord>? records,
    bool isTargetIdPresent,
    string? targetIdFullName,
    string? targetIdVesselName,
    double transactionPercentage,
    uint minTransactionPrice,
    uint maxTransactionPrice,
    uint? fixedTransactionPrice,
    List<DockedGridEntry>? dockedGrids
) : BoundUserInterfaceState
{
    public bool IsTargetIdPresent { get; set; } = isTargetIdPresent;
    public List<ShuttleRecord>? Records { get; set; } = records; // To cut down on bandwidth, states without changes to records imply no change to the last state seen.
    public string? TargetIdFullName { get; set; } = targetIdFullName;
    public string? TargetIdVesselName { get; set; } = targetIdVesselName;
    public double TransactionPercentage { get; set; } = transactionPercentage;
    public uint MinTransactionPrice { get; set; } = minTransactionPrice;
    public uint MaxTransactionPrice { get; set; } = maxTransactionPrice;
    public uint? FixedTransactionPrice { get; set; } = fixedTransactionPrice;

    // List of grids currently docked to the console's grid
    public List<DockedGridEntry> DockedGrids { get; set; } = dockedGrids ?? new List<DockedGridEntry>();
}
