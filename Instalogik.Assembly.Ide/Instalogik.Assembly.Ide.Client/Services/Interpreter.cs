using Instalogik.Assembly.Ide.Client.Model;
using MudBlazor;

namespace Instalogik.Assembly.Ide.Client.Services;

public class Interpreter
{
    public event EventHandler<KonsoleItemArgs> OnConsoleOutput;
    public event EventHandler<string> InputRequired;
    public event EventHandler<BoxChangedArgs> BoxChanged;
    public event EventHandler<bool> OnFinished;

    private string NormalColor = Colors.BlueGray.Lighten5;
    private string SelectedColor = Colors.Green.Lighten3;

    private IEnumerable<Instruction> _instructions;
    private Instruction? _currentInstruction;

    private bool _autorunning;

    Dictionary<string, int> _boxes = [];

    public async Task Run(IEnumerable<Instruction> instructions, bool autorun = false)
    {
        if (!instructions.Any())
            return;

        _autorunning = autorun;

        _boxes.Clear();
        _boxes.Add("A", 0);
        _boxes.Add("B", 0);
        _boxes.Add("C", 0);
        _boxes.Add("D", 0);

        _instructions = instructions;
        _currentInstruction = instructions.First();
        _currentInstruction.Background = SelectedColor;

        if (_autorunning)
        {
            await Task.Delay(300);
            await ExecuteStep();
        }
    }

    public async Task NextStep()
    {
        Instruction nextInstruction = _instructions.FirstOrDefault(i => i.Step == _currentInstruction.Step + 1);

        if (nextInstruction != null)
        {
            SetInstruction(nextInstruction);
            if (_autorunning)
            {
                await Task.Delay(300);
                await ExecuteStep();
            }
        }
        else
        {
            End(false);
        }
    }

    public async Task ExecuteStep()
    {
        if (_currentInstruction == null)
        {
            End(false);
            return;
        }

        switch (_currentInstruction)
        {
            case DecrementInstruction decrementInstruction:
                Decrement(decrementInstruction);
                await NextStep();
                break;
            case IfInstruction ifInstruction:
                If(ifInstruction);
                break;
            case IncrementInstruction incrementInstruction:
                Increment(incrementInstruction);
                await NextStep();
                break;
            case JumpInstruction jumpInstruction:
                Jump(jumpInstruction);
                break;
            case LoadInstruction loadInstruction:
                Load(loadInstruction);
                break;
            case NewLineInstruction:
                OnConsoleOutput.Invoke(this, new KonsoleItemArgs(false, ""));
                await NextStep();
                break;
            case PrintBoxInstruction printBoxInstruction:
                PrintBox(printBoxInstruction);
                await NextStep();
                break;
            case PrintTextInstruction printTextInstruction:
                OnConsoleOutput.Invoke(this, new KonsoleItemArgs(false, printTextInstruction.Text, false));
                await NextStep();
                break;
            case SetInstruction setInstruction:
                Set(setInstruction);
                await NextStep();
                break;
        }
    }

    private void If(IfInstruction instruction)
    {
        int left = _boxes[instruction.Box];
        int right = 0;
        if (_boxes.Keys.Contains(instruction.Value))
        {
            right = _boxes[instruction.Value];
        }
        else if (int.TryParse(instruction.Value, out int value))
        {
            right = value;
        }
        else
        {
            End(true);
        }

        switch (instruction.Operator)
        {
            case "<":
                Jump(left < right ? instruction.JumpIfTrue : instruction.JumpIfFalse);
                break;
            case "\u2264":
                Jump(left <= right ? instruction.JumpIfTrue : instruction.JumpIfFalse);
                break;
            case "=":
                Jump(left == right ? instruction.JumpIfTrue : instruction.JumpIfFalse);
                break;
            case "\u2260":
                Jump(left != right ? instruction.JumpIfTrue : instruction.JumpIfFalse);
                break;
            case "\u2265":
                Jump(left >= right ? instruction.JumpIfTrue : instruction.JumpIfFalse);
                break;
            case ">":
                Jump(left > right ? instruction.JumpIfTrue : instruction.JumpIfFalse);
                break;

        }


    }

    private void Set(SetInstruction instruction)
    {
        if (_boxes.Keys.Contains(instruction.Value))
        {
            _boxes[instruction.Box] = _boxes[instruction.Value];
        }
        else if (int.TryParse(instruction.Value, out int value))
        {
            _boxes[instruction.Box] = value;
        }
        else
        {
            End(true);
        }
    }

    private void PrintBox(PrintBoxInstruction instruction)
    {
        string value = _boxes[instruction.Box].ToString();
        OnConsoleOutput.Invoke(this, new KonsoleItemArgs(false, value, false));
    }

    private void SetInstruction(Instruction? nextInstruction)
    {
        if (nextInstruction == null)
        {
            End(true);
            return;
        }

        _currentInstruction.Background = NormalColor;
        _currentInstruction = nextInstruction;
        _currentInstruction.Background = SelectedColor;
    }

    public async Task SetBox(string box, int value)
    {
        _boxes[box] = value;
        BoxChanged.Invoke(this, new BoxChangedArgs(box, value));
        OnConsoleOutput.Invoke(this, new KonsoleItemArgs(true, value.ToString()));
        await NextStep();
    }

    private void Load(LoadInstruction instruction)
    {
        InputRequired.Invoke(this, instruction.Box);
    }

    private void Jump(JumpInstruction instruction)
    {
        if (instruction.Step.Equals("końca", StringComparison.InvariantCultureIgnoreCase))
        {
            SetInstruction(_instructions.Last());
        }
        else if (instruction.Step.Equals("następnej", StringComparison.InvariantCultureIgnoreCase))
        {
            SetInstruction(_instructions.First(i => i.Step == _currentInstruction.Step++));
        }
        else if (int.TryParse(instruction.Step, out int step))
        {
            SetInstruction(_instructions.FirstOrDefault(i => i.Step == step));
        }
        else
        {
            End(true);
        }
    }

    private void Jump(string step)
    {
        if (step.Equals("końca", StringComparison.InvariantCultureIgnoreCase))
        {
            SetInstruction(_instructions.Last());
        }
        else if (int.TryParse(step, out int s))
        {
            SetInstruction(_instructions.FirstOrDefault(i => i.Step == s));
        }
        else
        {
            End(false);

        }
    }

    private void Increment(IncrementInstruction instruction)
    {
        if (_boxes.Keys.Contains(instruction.Value))
        {
            _boxes[instruction.Box] += _boxes[instruction.Value];
        }
        else if (int.TryParse(instruction.Value, out int value))
        {
            _boxes[instruction.Box] += value;
        }
        else
        {
            End(true);
        }
    }

    private void Decrement(DecrementInstruction instruction)
    {
        if (_boxes.Keys.Contains(instruction.Value))
        {
            _boxes[instruction.Box] -= _boxes[instruction.Value];
        }
        else if (int.TryParse(instruction.Value, out int value))
        {
            _boxes[instruction.Box] -= value;
        }
        else
        {
            End(true);
        }
    }

    private void End(bool isFailed)
    {
        OnFinished.Invoke(this, isFailed);
        if (_currentInstruction != null) 
            _currentInstruction.Background = NormalColor;
    }
}