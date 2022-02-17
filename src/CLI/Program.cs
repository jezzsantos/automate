using System;
using automate.Infrastructure;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace automate
{
    internal class Program
    {
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    //services.AddTransient<MyService>();
                });
        }

        [UsedImplicitly]
        private static int Main(string[] args)
        {
            using (var host = CreateHostBuilder(args).Build())
            {
                host.Start();

                var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();

                try
                {
                    var result = CommandLineApi.Execute(args);
                    lifetime.StopApplication();
                    host.WaitForShutdownAsync().GetAwaiter().GetResult();

                    return result;
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.Message);
                    return 1;
                }
            }
        }
    }
}