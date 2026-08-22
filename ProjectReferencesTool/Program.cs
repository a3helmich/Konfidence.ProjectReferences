using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToolClasses;
using ToolClasses.ExtensionMethods;
using ToolClasses.Solutions;
using ToolInterfaces;

namespace ProjectReferencesTool;

internal class Program
{
    private static IServiceProvider? ServiceProvider { get; set; }

    private static void Main(string[] args)
    {
        string[] commandLineArguments = args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments);

        // CreateDefaultBuilder already adds the command line as the last configuration source
        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(commandLineArguments);

        hostBuilder.ConfigureServices((
            context,
            services) =>
        {
            ApplicationConfiguration applicationConfiguration = new(context.Configuration);

            services
                .AddSingleton(applicationConfiguration)
                .AddSingleton<ProjectReferencesEngine>()
                .AddSingleton<SolutionReader>()
                .AddSingleton<ISolution, Solution>()
                .AddSingleton<ArgumentParser>();
        });

        ServiceProvider = hostBuilder.Build().Services;

        ProjectReferencesEngine? projectReferencesEngine = ServiceProvider.GetService<ProjectReferencesEngine>();

        projectReferencesEngine?.Execute();
    }
}