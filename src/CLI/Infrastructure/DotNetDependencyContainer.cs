using System;
using Automate.Common;
using Automate.Common.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Automate.CLI.Infrastructure
{
    public class DotNetDependencyContainer : IDependencyContainer
    {
        private readonly IServiceCollection services;
        private IServiceProvider serviceProvider;

        public DotNetDependencyContainer(IServiceCollection services)
        {
            this.services = services;
        }

        public TService Resolve<TService>()
        {
            if (this.serviceProvider.NotExists())
            {
                this.serviceProvider = this.services.BuildServiceProvider();
            }

            return this.serviceProvider.GetRequiredService<TService>();
        }
    }
}