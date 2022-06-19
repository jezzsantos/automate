using System;

namespace Automate.Domain
{
    public interface IPersistableFactory
    {
        IPersistable Rehydrate<TPersistable>(PersistableProperties properties)
            where TPersistable : IPersistable;

        IPersistable Rehydrate(Type persistableType, PersistableProperties properties);
    }
}