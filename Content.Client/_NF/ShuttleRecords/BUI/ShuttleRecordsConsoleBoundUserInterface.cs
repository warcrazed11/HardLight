using Content.Client._NF.ShuttleRecords.UI;
using Content.Shared._NF.ShuttleRecords;
using Content.Shared._NF.ShuttleRecords.Components;
using Content.Shared._NF.ShuttleRecords.Events;
using Content.Shared.Containers.ItemSlots;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using static Robust.Client.UserInterface.Controls.BaseButton;

// Suppress naming style rule for the _NF namespace prefix (project convention)
#pragma warning disable IDE1006
namespace Content.Client._NF.ShuttleRecords.BUI;

public sealed class ShuttleRecordsConsoleBoundUserInterface(
    EntityUid owner,
    Enum uiKey
) : BoundUserInterface(owner, uiKey)
{
    private ShuttleRecordsWindow? _window;
    private ItemList? _dockedGridsList;
    private Button? _createDeedButton;
    private NetEntity? _selectedDockedGrid;

    protected override void Open()
    {
        base.Open();

        if (_window == null)
        {
            _window = this.CreateWindow<ShuttleRecordsWindow>();
            _window.OnCopyDeed += CopyDeed;
            _window.TargetIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent(ShuttleRecordsConsoleComponent.TargetIdCardSlotId));

            _dockedGridsList = _window.FindControl<ItemList>("DockedGridsList");
            _createDeedButton = _window.FindControl<Button>("CreateDeedButton");
            if (_dockedGridsList != null)
                _dockedGridsList.OnItemSelected += OnDockedGridSelected;
            if (_createDeedButton != null)
                _createDeedButton.OnPressed += OnCreateDeedPressed;
        }
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (_window == null || state is not ShuttleRecordsConsoleInterfaceState shuttleRecordsConsoleInterfaceState)
            return;

        _window?.UpdateState(shuttleRecordsConsoleInterfaceState);

        // Populate docked grids list in window
        if (_dockedGridsList != null)
        {
            _dockedGridsList.Clear();
            foreach (var entry in shuttleRecordsConsoleInterfaceState.DockedGrids)
            {
                var item = _dockedGridsList.AddItem(entry.Name);
                item.Metadata = entry.Grid;
            }
            if (_createDeedButton != null)
                _createDeedButton.Disabled = shuttleRecordsConsoleInterfaceState.DockedGrids.Count == 0;
        }
    }

    private void CopyDeed(ShuttleRecord shuttleRecord)
    {
        if (!EntMan.GetEntity(shuttleRecord.EntityUid).Valid)
            return;

        SendMessage(new CopyDeedMessage(shuttleRecord.EntityUid));
    }

    private void OnDockedGridSelected(ItemList.ItemListSelectedEventArgs args)
    {
        if (_dockedGridsList == null)
            return;
        var item = _dockedGridsList[args.ItemIndex];
        _selectedDockedGrid = (NetEntity)item.Metadata!;
        if (_createDeedButton != null)
            _createDeedButton.Disabled = false;
    }

    private void OnCreateDeedPressed(BaseButton.ButtonEventArgs args)
    {
        if (_selectedDockedGrid == null)
            return;
        SendMessage(new CreateDeedFromDockedGridMessage(_selectedDockedGrid.Value));
    }

}
