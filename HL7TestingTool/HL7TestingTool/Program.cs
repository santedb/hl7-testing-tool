using HL7TestingTool.Core;
using HL7TestingTool.Core.Impl;
using HL7TestingTool.Interop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace HL7TestingTool
{
    /// <summary>
    /// Represents the main program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// The working directory.
        /// </summary>
        private static string workingDirectory;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static async Task Main(string[] args)
        {
            if (args?.Any() == true && args[0]?.ToLowerInvariant().StartsWith("--h") == true)
            {
                PrintHelp();
                Environment.Exit(0);
            }

            var entryAssembly = Assembly.GetEntryAssembly() ?? typeof(Program).Assembly;

            workingDirectory = Path.GetDirectoryName(entryAssembly.Location);
            Directory.SetCurrentDirectory(workingDirectory);

            if (string.IsNullOrEmpty(workingDirectory))
            {
                throw new ArgumentNullException(nameof(workingDirectory), "Value cannot be null");
            }

            var builder = CreateHostBuilder(args);
            var host = builder.Build();

            var logger = host.Services.GetService<ILogger<Program>>();

            try
            {
                logger.LogInformation($"HL7 Testing Tool: v{entryAssembly.GetName().Version}");
                logger.LogInformation($"HL7 Testing Tool Working Directory : {Path.GetDirectoryName(entryAssembly.Location)}");
                logger.LogInformation($"HL7 Testing Tool Running Environment : {host.Services.GetService<IHostEnvironment>().EnvironmentName}");
                logger.LogInformation($"Operating System: {Environment.OSVersion.Platform} {Environment.OSVersion.VersionString}");
                logger.LogInformation($"CLI Version: {Environment.Version}");

                var applicationLifetime = host.Services.GetService<IHostApplicationLifetime>();

                if (applicationLifetime == null)
                {
                    throw new InvalidOperationException($"Unable to locate instance for {typeof(IHostApplicationLifetime).AssemblyQualifiedName}");
                }

                applicationLifetime.ApplicationStarted.Register(() =>
                {
                    logger.LogInformation("Application started");

                    var stopwatch = new Stopwatch();

                    var testExecutor = host.Services.GetService<ITestExecutor>();

                    stopwatch.Start();

                    testExecutor.ExecuteTestSteps();

                    stopwatch.Stop();

                    logger.LogInformation($"All tests have completed execution in {stopwatch.Elapsed.TotalMilliseconds} ms");
                });

                applicationLifetime.ApplicationStopping.Register(() =>
                {
                    logger.LogInformation("Application stopping");
                });

                applicationLifetime.ApplicationStopped.Register(() =>
                {
                    logger.LogInformation("Application stopped");
                });

                await host.RunAsync();
            }
            catch (Exception e)
            {
                logger.LogCritical($"Unable to start application: {e}");
                throw;
            }
        }

        /// <summary>
        /// Creates the host builder.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>Returns a host builder.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            var builder = Host.CreateDefaultBuilder(args);

            builder.ConfigureHostConfiguration(configurationHost =>
            {
                configurationHost.Properties.Clear();
                configurationHost.Sources.Clear();

                configurationHost.SetBasePath(workingDirectory);
                configurationHost.AddJsonFile("appsettings.json", false);
            });

            builder.ConfigureAppConfiguration((hostingContext, configurationHost) =>
            {
                var environment = hostingContext.Configuration.GetValue<string>("Environment");

                var endpoint = hostingContext.Configuration.GetValue<string>("Endpoint");

                if (!Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
                {
                    throw new ConfigurationErrorsException($"Endpoint: {endpoint} is not a well formed URI");
                }

                switch (environment)
                {
                    case "Development":
#if !DEBUG
                        throw new ArgumentException($"Invalid environment: {environment} with a release build");
#endif
                    case "Staging":
                    case "Production":
                        builder.UseEnvironment(environment);
                        break;
                }
            });

            builder.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton<ITestExecutor, TestExecutor>();
                serviceCollection.AddSingleton<IMllpMessageSender, MllpMessageSender>();
                serviceCollection.AddLogging();
            });

            builder.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

            builder.UseConsoleLifetime();

            return builder;
        }

        /// <summary>
        /// Prints help text.
        /// </summary>
        private static void PrintHelp()
        {
            Console.WriteLine("HL7 Testing Tool Help" + Environment.NewLine);
            Console.WriteLine("The appsettings.json contains the configuration settings for the tool.");
            Console.WriteLine("The following properties in configuration file are required to operate the tool:");
            Console.WriteLine("\"MinimumLevel\" - This property changes the log level, and supports the following values Debug, Information, Warning, Error, Fatal.");
            Console.WriteLine("\"Endpoint\" - This is target server to which the tool will attempt to connect.");
            Console.WriteLine("\"TestDirectory\" - The directory where the test files are located.");
            Console.WriteLine("\"Execution\" - This controls the execution of the tests. Setting this to '*' will execute all the tests. Optionally this can be set to a list of tests to be executed (e.g. [ \"OHIE-01-10\", \"OHIE-02-05\", \"OHIE-03-10\" ]).");
        }
    }
}