namespace Automate.CLI.Domain
{
    internal interface ICloneable<out TObject>
    {
        TObject Clone();
    }
}