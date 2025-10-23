using System;
using Robust.Shared.Serialization;
using Content.Shared.UserInterface;

namespace Content.Shared.CM14.Xenos.Evolution;

[Serializable, NetSerializable]
public enum XenoEvolutionUIKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class EvolveBuiMessage : BoundUserInterfaceMessage
{
    public readonly int Choice;

    public EvolveBuiMessage(int choice)
    {
        Choice = choice;
    }
}
