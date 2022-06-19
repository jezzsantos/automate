namespace Automate.Common
{
    public interface IDependencyContainer
    {
        TService Resolve<TService>();
    }
}