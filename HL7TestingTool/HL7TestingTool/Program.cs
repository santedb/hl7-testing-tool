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

namespace HL7TestingTool
{
    internal class Program
    {
        /// <summary>
        /// 
        /// </summary>
        private const string DATA = "data";

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
            //var dataPath = Path.GetFullPath($"{DATA}");
            //var director = new TestSuiteBuilderDirector(new TestSuiteBuilder(), dataPath);
            //director.BuildFromXml();

            //var option = 0;
            //while (option != 4) // Show main menu and exit if option 4 is entered
            //{
            //    option = MainMenu(director);
            //    if (option < 0 || option > 4)
            //    {
            //        Console.WriteLine($"\n {option} is not an option! Try again...");
            //    }
            //    else if (option != 4)
            //    {
            //        Console.WriteLine("Press any key to return to main menu...");
            //        Console.ReadKey();
            //        Console.Clear();
            //    }
            //}

            Console.ResetColor();

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

            if (logger == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Critical Error, NO LOGGER HAS BEEN CONFIGURED");
                throw new Exception();
            }

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

                    //testExecutor.ExecuteTestSteps()
                    //var dataPath = Path.GetFullPath("data");
                    //var director = new TestSuiteBuilderDirector(new TestSuiteBuilder(), dataPath);
                    //director.BuildFromXml();

                    //director.ExecuteTestSteps(director.GetTestSuite());
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
                serviceCollection.AddSingleton<ITestExecutor, TestSuiteBuilderDirector>();
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

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <returns>Option selected by user in main menu</returns>
        //private static int MainMenu(TestSuiteBuilderDirector director)
        //{
        //    var option = 0;
        //    Console.ForegroundColor = ConsoleColor.Cyan;
        //    Console.WriteLine("======================================");
        //    Console.WriteLine("| Welcome to the HL7v2 Testing Tool! |");
        //    Console.WriteLine("======================================\n");

        //    Console.ForegroundColor = ConsoleColor.White;
        //    Console.WriteLine("Please enter an integer option: ");
        //    Console.ForegroundColor = ConsoleColor.Blue;
        //    Console.WriteLine("\t(1) Execute all test steps in the test suite");
        //    Console.WriteLine("\t(2) Execute all test steps of a specific test case");
        //    Console.WriteLine("\t(3) Execute a specific test step"); //get rid of this
        //    Console.WriteLine("\t(4) Exit");
        //    try
        //    {
        //        Console.ForegroundColor = ConsoleColor.Yellow;
        //        option = int.Parse(Console.ReadLine());
        //        Console.Clear();
        //        switch (option)
        //        {
        //            case 1:
        //                //Call on helper to execute all test steps in test suite
        //                director.ExecuteTestSteps(director.GetTestSuite());
        //                break;
        //            case 2:
        //                Console.Write("Enter Case #:\t");
        //                int.TryParse(Console.ReadLine(), out var caseNumber);
        //                //Call on helper to execute all test steps only from a specific case 
        //                try
        //                {
        //                    ValidateCaseNumber(director, caseNumber);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex.Message);
        //                    break;
        //                }

        //                director.ExecuteTestSteps(director.GetTestCase(caseNumber));
        //                break;
        //            case 3:
        //                Console.Write("Enter Case #:\t");
        //                int.TryParse(Console.ReadLine(), out caseNumber);
        //                try
        //                {
        //                    ValidateCaseNumber(director, caseNumber);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex.Message);
        //                    break;
        //                }

        //                Console.Write("Enter Step #:\t");
        //                int.TryParse(Console.ReadLine(), out var stepNumber);
        //                try
        //                {
        //                    ValidateStepNumber(director, caseNumber, stepNumber);
        //                }
        //                catch (Exception ex)
        //                {
        //                    Console.WriteLine(ex.Message);
        //                    break;
        //                }

        //                //Call on helper to execute a specific test step(note that this must be a list, but the director's method only returns a TestStep)
        //                var testSteps = new List<TestStep> {director.GetTestStep(caseNumber, stepNumber)};
        //                director.ExecuteTestSteps(testSteps);
        //                break;
        //            case 4:
        //                break;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }

        //    Console.ForegroundColor = ConsoleColor.White;
        //    return option;
        //}

        //private static void ValidateCaseNumber(TestSuiteBuilderDirector director, int caseNumber)
        //{
        //    if (director.GetTestCase(caseNumber).Count == 0)
        //    {
        //        throw new Exception("No test case with that number found.");
        //    }
        //}

        //private static void ValidateStepNumber(TestSuiteBuilderDirector director, int caseNumber, int stepNumber)
        //{
        //    if (director.GetTestStep(caseNumber, stepNumber) == null)
        //    {
        //        throw new Exception("No test steps with that number found.");
        //    }
        //}
    }
}