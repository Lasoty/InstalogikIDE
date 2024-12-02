namespace Instalogik.Assembly.Ide.Client.Model
{
    public class BoxChangedArgs(string box, int value) : EventArgs
    {
        public string BoxName { get; set; } = box;

        public int Value { get; set; } = value;
    }
}
