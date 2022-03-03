using HL7TestingTool.Core;
using HL7TestingTool.Core.Impl;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using HL7TestingTool.Interop;

namespace HL7TestingTool
{
    internal class Program
    {
        /// <summary>
        /// The working directory.
        /// </summary>
        private static string workingDirectory;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        private static async Task Main(string[] args)
        {

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

                    var testExecutor = host.Services.GetService<ITestExecutor>();

                    testExecutor.ExecuteTestSteps();
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

            string environment = null;

            builder.ConfigureHostConfiguration(configurationHost =>
            {
                configurationHost.Properties.Clear();
                configurationHost.Sources.Clear();

                configurationHost.SetBasePath(workingDirectory);
                configurationHost.AddJsonFile("appsettings.json", false);
            });

            builder.ConfigureAppConfiguration((hostingContext, configurationHost) =>
            {
                environment = hostingContext.Configuration.GetValue<string>("Environment");

                var endpoint = hostingContext.Configuration.GetValue<string>("Endpoint");

                if (!Uri.IsWellFormedUriString(endpoint, UriKind.Absolute))
                {
                    throw new ConfigurationErrorsException($"Endpoint: {endpoint} is not a well formed URI");
                }
            });

            builder.ConfigureServices(serviceCollection =>
            {
                serviceCollection.AddSingleton<ITestExecutor, TestExecutor>();
                serviceCollection.AddSingleton<IMllpMessageSender, MllpMessageSender>();
                serviceCollection.AddLogging();
            });

            builder.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration));

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

            builder.UseConsoleLifetime();

            return builder;
        }
    }
}