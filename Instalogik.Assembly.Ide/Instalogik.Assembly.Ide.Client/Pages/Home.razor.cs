using System.Collections.ObjectModel;
using System.Linq;
using Instalogik.Assembly.Ide.Client.Model;
using Instalogik.Assembly.Ide.Client.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Instalogik.Assembly.Ide.Client.Pages;

public partial class Home
{
    private const string OperationsZone = "Edytor";
    private const string InstructionsZone = "Instrukcje";

    private bool _alertClosed;
    private bool _isLoaded;
    private bool _isRunning;
    private bool _isInputingValue;
    private bool _isFinished;
    private bool _isFailed;

    private int? _inputValue;
    private string? _curentBoxInput;

    private MudDropContainer<Instruction>? _dropContainer;

    private List<Box> _boxes =
    [
        new("A"),
        new("B"),
        new("C"),
        new("D"),
    ];

    private readonly List<Instruction> _instructions =
    [
        new PrintBoxInstruction(InstructionsZone),
        new PrintTextInstruction(InstructionsZone),
        new NewLineInstruction(InstructionsZone),
        new LoadInstruction(InstructionsZone),
        new SetInstruction(InstructionsZone),
        new IncrementInstruction(InstructionsZone),
        new DecrementInstruction(InstructionsZone),
        new IfInstruction(InstructionsZone),
        new JumpInstruction(InstructionsZone)
    ];
    
    private List<Instruction> _operations = [];
    private List<Instruction> Items {get; set;}
    private int InitializeCount => _instructions.Count;

    private readonly ObservableCollection<KonsoleItemArgs> _konsole = [];

    private Interpreter? _interpreter;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        
        Items = [.._instructions];
        
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

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (firstRender)
        {
            _isLoaded = true;
            StateHasChanged();
        }
        
    }
[Inject] public ILogger<Home> Logger { get; set; }

private async Task ItemUpdated(MudItemDropInfo<Instruction> dropItem)
{
    ArgumentNullException.ThrowIfNull(dropItem.Item);

    Logger.LogInformation(
        $"ItemUpdated called with dropItem: {dropItem.Item.Name}, dropzone: {dropItem.DropzoneIdentifier}, index: {dropItem.IndexInZone}, current index: {dropItem.Item.Step}");

    var operation = dropItem.Item.CloneForZone(OperationsZone) as Instruction ?? throw new InvalidOperationException();

    if (IsExistingOperation(dropItem))
    {
        UpdateExistingOperation(dropItem, operation);
    }
    else if (IsNewOperation(dropItem))
    {
        UpdateForNewOperation(operation);
        _operations.Add(operation);
    }
    else
    {
        Logger.LogInformation($"Item not added to Operations: {operation.Name}");
        throw new InvalidOperationException($"Item not added to Operations: {operation.Name}");
    }

    await RefreshDropZone();
    Logger.LogInformation("Drop zone refreshed");
}

private void UpdateExistingOperation(MudItemDropInfo<Instruction> item, Instruction secondItem)
{
    var dropItem = item.Item;
    ArgumentNullException.ThrowIfNull(dropItem);
    
    var indexOfDropItem = _operations.IndexOf(dropItem);
    var indexOfToSwitchItem = item.IndexInZone;
    
    var firstItem = _operations[indexOfToSwitchItem];
    
    _operations[indexOfToSwitchItem] = dropItem;
    _operations[indexOfDropItem] = firstItem;
   
    Logger.LogInformation($"Items switched: {firstItem.Name} and {secondItem.Name}");
}

private void UpdateForNewOperation(Instruction operation)
{
    operation.Step = _operations.Count != 0 ? _operations.Count + 1 : 1;
    Logger.LogInformation($"Item added to Operations: {operation.Name}, Step: {operation.Step}");
}

private static bool IsExistingOperation(MudItemDropInfo<Instruction> dropItem)
{
    return dropItem.DropzoneIdentifier == OperationsZone && dropItem.Item!.Zone == OperationsZone;
}

private static bool IsNewOperation(MudItemDropInfo<Instruction> dropItem)
{
    return dropItem.DropzoneIdentifier == OperationsZone && dropItem.Item!.Zone == InstructionsZone;
}


    private async Task RefreshDropZone()
    {
        _operations.ForEach(x => x.Step = _operations.IndexOf(x) + 1 + InitializeCount);
        Items = [.._instructions.Concat(_operations).OrderBy(x => x.Step)];
        
        await InvokeAsync(StateHasChanged);
    }

    private List<string> GetJumpSteps(bool withNext = false)
    {
        List<string> result = [];
        if (withNext) result.Add("następnej");
        result.AddRange(Items.Where(x => x.Step > 0).Select(x => x.Step.ToString()).ToList());
        result.Add("końca");
        return result;
    }

    private async Task DeleteItem(Instruction item)
    {
        _operations = _operations.Where(x => x.Id != item.Id).ToList();

       await RefreshDropZone();
    }

    private async Task RunProgram()
    {
        CleanProgram();

        _isRunning = true;
        _interpreter?.Run(Items.Where(i => i.Step > 0));
    }

    private void CleanProgram()
    {
        _isFailed = false;
        _isFinished = false;
        foreach (Box box in _boxes)
        {
            box.Value = 0;
        }
        _konsole.Clear();
        _inputValue = null;
    }

    private void StopProgram()
    {
        _isRunning = false;
    }

    private async Task NextStep()
    {
        await _interpreter?.ExecuteStep()!;
    }

    private async Task InputValue()
    {
        if (_inputValue.HasValue)
        {
            await _interpreter?.SetBox(_curentBoxInput!, _inputValue.Value)!;
            _isInputingValue = false;
        }
    }

    private void AddToConsole(object? sender, KonsoleItemArgs? e)
    {
        if (e == null)
            return;

        if (!_konsole.Any())
        {
            _konsole.Add(new KonsoleItemArgs(e.IsInput, e.Text));
            return;
        }

        if (_konsole.Last().IsInput != e.IsInput)
        {
            _konsole.Add(new KonsoleItemArgs(e.IsInput, e.Text));
            return;
        }

        if (e.IsNewLine)
        {
            _konsole.Add(new KonsoleItemArgs(e.IsInput, e.Text));
            return;
        }

        if (e.IsInput)
            _konsole.Last().Text += "<br />";

        _konsole.Last().Text += e.Text;
    }

    private void FinishProgram(object? sender, bool isFail)
    {
        _isFinished = true;
        _isFailed = isFail;
    }

    private async Task AutoRun()
    {
        await _interpreter?.Run(Items.Where(i => i.Step > 0), true)!;
    }
}