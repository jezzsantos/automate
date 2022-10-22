using System;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Automate.CLI.Infrastructure
{
    public class DotNetDependencyContainer : IDependencyContainer
    {
        private readonly IServiceProvider serviceProvider;

        public DotNetDependencyContainer(IServiceProvider serviceProvider)
        {
            serviceProvider.GuardAgainstNull(nameof(serviceProvider));
            this.serviceProvider = serviceProvider;
        }

        public TService Resolve<TService>()
        {
            return this.serviceProvider.GetRequiredService<TService>();
        }
    }
}