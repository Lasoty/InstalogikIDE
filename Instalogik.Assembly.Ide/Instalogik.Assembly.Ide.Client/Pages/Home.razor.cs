using System.Collections.ObjectModel;
using Instalogik.Assembly.Ide.Client.Model;
using Instalogik.Assembly.Ide.Client.Services;
using MudBlazor;

namespace Instalogik.Assembly.Ide.Client.Pages;

public partial class Home
{
    const string STEPS_ZONE = "Edytor";
    const string LIST_ZONE = "Instrukcje";

    private bool alertClosed;
    private bool isLoaded;
    private bool _isRunning;
    private bool _isInputingValue;
    private bool _isFinished;
    private bool _isFailed;

    private int? _inputValue;
    private string? _curentBoxInput;

    private MudDropContainer<Instruction> _dropContainer;

    private List<Box> _boxes =
    [
        new("A"),
        new("B"),
        new("C"),
        new("D"),
    ];

    private List<DropZone> _zones =
    [
        new() { Name = STEPS_ZONE },
        new() { Name = LIST_ZONE }
    ];

    private List<Instruction> _items =
    [
        new PrintBoxInstruction(LIST_ZONE),
        new PrintTextInstruction(LIST_ZONE),
        new NewLineInstruction(LIST_ZONE),
        new LoadInstruction(LIST_ZONE),
        new SetInstruction(LIST_ZONE),
        new IncrementInstruction(LIST_ZONE),
        new DecrementInstruction(LIST_ZONE),
        new IfInstruction(LIST_ZONE),
        new JumpInstruction(LIST_ZONE)
    ];

    private ObservableCollection<KonsoleItemArgs> Konsole = [];

    private Interpreter _interpreter;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _interpreter = new Interpreter();
        _interpreter.OnConsoleOutput += AddToConsole;
        _interpreter.InputRequired += (_, b) =>
        {
            _curentBoxInput = b;
            _isInputingValue = true;
        };
        _interpreter.BoxChanged += (_, e) => _boxes.First(x => x.Name == e.BoxName).Value = e.Value;
        _interpreter.OnFinished += FinishProgram;

    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            isLoaded = true;
            StateHasChanged();
        }
    }

    private void ItemUpdated(MudItemDropInfo<Instruction> dropItem)
    {
        if (dropItem.DropzoneIdentifier == _zones[0].Name && dropItem.Item.Zone == _zones[1].Name)
        {
            Instruction item = dropItem.Item.CopyTo(STEPS_ZONE) as Instruction;
            _items.Insert(dropItem.IndexInZone + 9, item);
        }
        else
        {
            dropItem.Item.Zone = dropItem.DropzoneIdentifier;
        }

        RefreshDropZone();
    }

    private void RefreshDropZone()
    {
        for (int i = 9; i < _items.Count; i++)
        {
            _items[i].Step = i - 8;
        }

        _dropContainer.Items = _items;

        ThreadPool.QueueUserWorkItem(_ =>
        {
            _dropContainer.Refresh();
            StateHasChanged();
        });
    }

    private List<string> GetJumpSteps(bool withNext = false)
    {
        List<string> result = [];
        if (withNext) result.Add("następnej");
        result.AddRange(_items.Where(x => x.Step > 0).Select(x => x.Step.ToString()).ToList());
        result.Add("końca");
        return result;
    }

    private async Task DeleteItem(Instruction item)
    {
        Instruction? localItem = _items.FirstOrDefault(x => x.Id == item.Id);

        if (localItem != null)
        {
            _items.Remove(localItem);
            StateHasChanged();
            RefreshDropZone();
        }
    }

    private async Task RunProgram()
    {
        CleanProgram();

        _isRunning = true;
        _interpreter.Run(_items.Where(i => i.Step > 0));
    }

    private void CleanProgram()
    {
        _isFailed = false;
        _isFinished = false;
        foreach (Box box in _boxes)
        {
            box.Value = 0;
        }
        Konsole.Clear();
        _inputValue = null;
    }

    private void StopProgram()
    {
        _isRunning = false;
    }

    private async Task NextStep()
    {
        await _interpreter.ExecuteStep();
    }

    private async Task InputValue()
    {
        if (_inputValue.HasValue)
        {
            await _interpreter.SetBox(_curentBoxInput!, _inputValue.Value);
            _isInputingValue = false;
        }
    }

    private void AddToConsole(object? sender, KonsoleItemArgs e)
    {
        if (e == null)
            return;

        if (!Konsole.Any())
        {
            Konsole.Add(new KonsoleItemArgs(e.IsInput, e.Text));
            return;
        }

        if (Konsole.Last().IsInput != e.IsInput)
        {
            Konsole.Add(new KonsoleItemArgs(e.IsInput, e.Text));
            return;
        }

        if (e.IsNewLine)
        {
            Konsole.Add(new KonsoleItemArgs(e.IsInput, e.Text));
            return;
        }

        if (e.IsInput)
            Konsole.Last().Text += "<br />";

        Konsole.Last().Text += e.Text;
    }

    private void FinishProgram(object? sender, bool isFail)
    {
        _isFinished = true;
        _isFailed = isFail;
    }

    private async Task AutoRun()
    {
        await _interpreter.Run(_items.Where(i => i.Step > 0), true);
    }
}