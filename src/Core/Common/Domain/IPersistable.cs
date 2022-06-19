namespace Automate.Common.Domain
{
    public interface IPersistable
    {
        PersistableProperties Dehydrate();
    }
}