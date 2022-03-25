/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: Nityan Khanna
 * Date: 2022-03-24
 */

using HL7TestingTool.Core;
using HL7TestingTool.Core.Impl;
using HL7TestingTool.Interop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HL7TestingTool.Test
{
    /// <summary>
    /// Represents a service provider utility.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal static class ServiceProviderUtility
    {
        /// <summary>
        /// Builds a service provider.
        /// </summary>
        /// <returns></returns>
        public static IServiceProvider BuildServiceProvider()
        {
            var serviceCollection = new ServiceCollection();

            serviceCollection.AddSingleton<IConfiguration>(options => new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", false)
                .Build());

            serviceCollection.AddSingleton<ITestExecutor, TestExecutor>();
            serviceCollection.AddSingleton<IMllpMessageSender, FakeMllpMessageSender>();
            serviceCollection.AddLogging();

            return serviceCollection.BuildServiceProvider();
        }
    }
}
