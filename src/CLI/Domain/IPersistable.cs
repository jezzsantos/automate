namespace Automate.CLI.Domain
{
    internal interface IPersistable
    {
        PersistableProperties Dehydrate();
    }
}