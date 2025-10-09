using Robust.Shared.Serialization;
using Content.Shared.UserInterface;

// Suppress naming style rule for the _NF namespace prefix (project convention)
#pragma warning disable IDE1006
namespace Content.Shared._NF.ShuttleRecords.Events;

/// <summary>
/// Client -> server: request to create/assign a deed for a selected docked grid to the inserted ID.
/// </summary>
[Serializable, NetSerializable]
public sealed class CreateDeedFromDockedGridMessage : BoundUserInterfaceMessage
{
    public NetEntity TargetGrid;

    public CreateDeedFromDockedGridMessage(NetEntity targetGrid)
    {
        TargetGrid = targetGrid;
    }
}
