using MudBlazor;

namespace Instalogik.Assembly.Ide.Client.Model;

public abstract class Instruction
{
    public Instruction(string zone)
    {
        Zone = zone;
        Id = Guid.NewGuid();
    }

    public Guid Id { get; init; }

    public string Zone { get; set; }

    public string Name { get; init; }

    public int Step { get; set; }

    public string Background { get; set; } = Colors.BlueGray.Lighten5;

    public abstract object CopyTo(string zone);
}


public class PrintBoxInstruction : Instruction
{
    public PrintBoxInstruction(string zone) : base(zone)
    {
        Name = "Wypisz pudełko";
    }

    public string Box { get; set; } = "A";
    public override object CopyTo(string zone) => new PrintBoxInstruction(zone);
}

public class PrintTextInstruction : Instruction
{
    public PrintTextInstruction(string zone) : base(zone)
    {
        Name = "Wypisz napis";
    }
    public string Text { get; set; }
    public override object CopyTo(string zone) => new PrintTextInstruction(zone);
}

public class NewLineInstruction : Instruction
{
    public NewLineInstruction(string zone) : base(zone)
    {
        Name = "Przejdź do nowej linii";
    }
    public override object CopyTo(string zone) => new NewLineInstruction(zone);
}

/// <summary>
/// Wczytaj wartość
/// </summary>
public class LoadInstruction : Instruction
{
    public LoadInstruction(string zone) : base(zone)
    {
        Name = "Wczytaj";
    }
    public string Box { get; set; } = "A";
    public override object CopyTo(string zone) => new LoadInstruction(zone);
}

public class SetInstruction : Instruction
{
    public SetInstruction(string zone) : base(zone)
    {
        Name = "Ustaw";
    }
    public string Box { get; set; } = "A";
    public string Value { get; set; }
    public override object CopyTo(string zone) => new SetInstruction(zone);
}

public class IncrementInstruction : Instruction
{
    public IncrementInstruction(string zone) : base(zone)
    {
        Name = "Zwiększ";
    }
    public string Box { get; set; } = "A";
    public string Value { get; set; }
    public override object CopyTo(string zone) => new IncrementInstruction(zone);
}

public class DecrementInstruction : Instruction
{
    public DecrementInstruction(string zone) : base(zone)
    {
        Name = "Zmniejsz";
    }
    public string Box { get; set; } = "A";
    public string Value { get; set; }
    public override object CopyTo(string zone) => new DecrementInstruction(zone);
}

public class IfInstruction : Instruction
{
    public IfInstruction(string zone) : base(zone)
    {
        Name = "Jeżeli";
    }
    public string Box { get; set; } = "A";
    public string Operator { get; set; }
    public string Value { get; set; }
    public string JumpIfTrue { get; set; }
    public string JumpIfFalse { get; set; }
    public override object CopyTo(string zone) => new IfInstruction(zone);
}

public class JumpInstruction : Instruction
{
    public JumpInstruction(string zone) : base(zone)
    {
        Name = "Skocz do";
    }
    public string Step { get; set; }
    public override object CopyTo(string zone) => new JumpInstruction(zone);
}

