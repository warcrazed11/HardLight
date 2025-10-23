using Content.Shared.CM14.Xenos;
using Content.Shared.CM14.Xenos.Construction;
using Content.Client._Shitmed.Xenonids.UI; // XenoChoiceControl
using JetBrains.Annotations;
using Robust.Client.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.Client.CM14.Xenos.Construction;

[UsedImplicitly]
public sealed class XenoChooseStructureBui : BoundUserInterface
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    private readonly SpriteSystem _sprite;

    [ViewVariables]
    private XenoChooseStructureWindow? _window;

    public XenoChooseStructureBui(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _sprite = EntMan.System<SpriteSystem>();
    }

    protected override void Open()
    {
        _window = new XenoChooseStructureWindow();
        _window.OnClose += Close;

        if (EntMan.TryGetComponent(Owner, out XenoComponent? xeno))
        {
            var selected = xeno.BuildChoice;
            foreach (var structureId in xeno.CanBuild)
            {
                if (!_prototype.TryIndex(structureId, out var structure))
                    continue;

                var control = new XenoChoiceControl();
                control.Set(structure.Name, _sprite.Frame0(structure));

                // Visual selection: toggle highlight and send selection without closing immediately
                if (selected != null && selected.Value.Equals(structureId))
                    control.Button.AddStyleClass("ButtonColorGreen");

                control.Button.OnPressed += _ =>
                {
                    // Clear previous highlights
                    foreach (var child in _window!.StructureContainer.Children)
                    {
                        if (child is XenoChoiceControl c)
                            c.Button.RemoveStyleClass("ButtonColorGreen");
                    }

                    // Mark this one and send selection
                    control.Button.AddStyleClass("ButtonColorGreen");
                    SendMessage(new XenoChooseStructureBuiMessage(structureId));

                    // Predictive UX: close the window immediately after selecting.
                    // The server will also close the UI authoritatively when it applies the choice.
                    _window.Close();
                };

                _window.StructureContainer.AddChild(control);
            }
        }

        _window.OpenCentered();
    }

    protected override void Dispose(bool disposing)
    {
        // Ensure base lifecycle runs so the UI system tracks open state properly
        base.Open();
        base.Dispose(disposing);
        if (disposing)
        {
            _window?.Dispose();
            _window = null;
        }
    }
}
