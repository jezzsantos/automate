namespace Automate.Domain
{
    public interface IPersistable
    {
        PersistableProperties Dehydrate();
    }
}