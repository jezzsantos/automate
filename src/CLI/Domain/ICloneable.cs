namespace automate.Domain
{
    internal interface ICloneable<out TObject>
    {
        TObject Clone();
    }
}