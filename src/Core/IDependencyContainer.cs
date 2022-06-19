namespace Automate
{
    public interface IDependencyContainer
    {
        TService Resolve<TService>();
    }
}