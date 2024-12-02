namespace Instalogik.Assembly.Ide.Client.Model;

public class KonsoleItemArgs(bool isInput, string text, bool isNewLine = true) : EventArgs
{
    public string Text { get; set; } = text;

    public bool IsInput { get; set; } = isInput;

    public bool IsNewLine { get; set; } = isNewLine;
}