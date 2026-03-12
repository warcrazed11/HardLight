using Content.Client.Eui;
using Content.Client.Silicons.Laws.SiliconLawEditUi;
using Content.Shared.Eui;
using Content.Shared.Silicons.Laws;

namespace Content.Client.HL.Silicons;

public sealed class GovernorLawAccessEui : BaseEui
{
    private readonly EntityManager _entityManager;

    private SiliconLawUi _siliconLawUi;
    private EntityUid _target;

    public GovernorLawAccessEui()
    {
        _entityManager = IoCManager.Resolve<EntityManager>();

        _siliconLawUi = new SiliconLawUi();
        _siliconLawUi.OnClose += () => SendMessage(new CloseEuiMessage());
        _siliconLawUi.Save.OnPressed += _ =>
            SendMessage(new SiliconLawsSaveMessage(_siliconLawUi.GetLaws(), _entityManager.GetNetEntity(_target)));
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not SiliconLawsEuiState lawsState)
            return;

        _target = _entityManager.GetEntity(lawsState.Target);
        _siliconLawUi.SetLaws(lawsState.Laws, allowCorruption: false);
    }

    public override void Opened()
    {
        _siliconLawUi.OpenCentered();
    }
}
