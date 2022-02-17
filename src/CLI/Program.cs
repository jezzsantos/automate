using System;
using Automate.CLI.Infrastructure;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Automate.CLI
{
    internal class Program
    {
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureLogging((_, logging) => { logging.ClearProviders(); })
                .ConfigureServices((_, _) =>
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