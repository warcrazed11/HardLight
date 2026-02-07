using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class ExpeditionDiskInterfaceState
{
    public bool HasDisk;
    public string PlanetType;
    public int Difficulty;
    public string Objective;
    public bool OnCooldown;
    public TimeSpan CooldownRemaining;
    public bool CanActivate;
    public bool InExpedition;
    public bool CanEndExpedition;

    public ExpeditionDiskInterfaceState(
        bool hasDisk,
        string planetType,
        int difficulty,
        string objective,
        bool onCooldown,
        TimeSpan cooldownRemaining,
        bool canActivate,
        bool inExpedition,
        bool canEndExpedition)
    {
        HasDisk = hasDisk;
        PlanetType = planetType;
        Difficulty = difficulty;
        Objective = objective;
        OnCooldown = onCooldown;
        CooldownRemaining = cooldownRemaining;
        CanActivate = canActivate;
        InExpedition = inExpedition;
        CanEndExpedition = canEndExpedition;
    }
}
