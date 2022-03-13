using System;

namespace Automate.CLI.Domain
{
    internal interface IPersistableFactory
    {
        IPersistable Rehydrate<TPersistable>(PersistableProperties properties)
            where TPersistable : IPersistable;

        IPersistable Rehydrate(Type persistableType, PersistableProperties properties);
    }
}