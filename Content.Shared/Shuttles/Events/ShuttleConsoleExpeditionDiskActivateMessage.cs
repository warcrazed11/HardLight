using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

[Serializable, NetSerializable]
public sealed class ShuttleConsoleExpeditionDiskActivateMessage : BoundUserInterfaceMessage
{
}

[Serializable, NetSerializable]
public sealed class ShuttleConsoleExpeditionEndMessage : BoundUserInterfaceMessage
{
}
