using System;
using System.Threading.Tasks;
using Konfidence.Base;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ToolClasses;
using ToolClasses.ExtensionMethods;
using ToolClasses.Projects;
using ToolClasses.Solutions;
using ToolInterfaces;

namespace ProjectReferencesTool;

internal class Program
{
    private static IServiceProvider? ServiceProvider { get; set; }

    private static async Task Main(string[] args)
    {
        string[] commandLineArguments = args.ExpandSwitchArguments(CommandLineExtensions.SwitchArguments);

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

                .AddSingleton<ProjectReader>()
                .AddSingleton<ProjectNames>();
        });

        ServiceProvider = hostBuilder.Build().Services;

        ProjectReferencesEngine? projectReferencesEngine = ServiceProvider.GetService<ProjectReferencesEngine>();

        if (projectReferencesEngine.IsAssigned())
        {
            await projectReferencesEngine.Execute();
        }
    }
}
