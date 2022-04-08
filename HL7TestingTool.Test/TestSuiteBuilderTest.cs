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

using HL7TestingTool.Core.Impl;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;

namespace HL7TestingTool.Test
{
    /// <summary>
    /// Contains tests for the <see cref="TestSuiteBuilder"/> class.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class TestSuiteBuilderTest : TestBase
    {
        /// <summary>
        /// The builder.
        /// </summary>
        private TestSuiteBuilder builder;

        /// <summary>
        /// The data directory.
        /// </summary>
        private string dataDirectory;

        /// <summary>
        /// Performs a one time initialization before any tests are run.
        /// </summary>
        [OneTimeSetUp]
        public override void Initialize()
        {
            base.Initialize();
            this.builder = new TestSuiteBuilder();
            this.dataDirectory = this.configuration.GetValue<string>("TestDirectory");
        }

        /// <summary>
        /// Tests the build method.
        /// </summary>
        [Test]
        public void TestBuild()
        {
            this.builder.Build(this.dataDirectory);

            Assert.AreEqual(3, this.builder.GetTestSuite().Count);
        }

        /// <summary>
        /// Tests the build method.
        /// </summary>
        [Test]
        public void TestBuildEmpty()
        {
            Assert.Throws<ArgumentNullException>(() => this.builder.Build(string.Empty));
        }

        /// <summary>
        /// Tests the build method.
        /// </summary>
        [Test]
        public void TestBuildNull()
        {
            Assert.Throws<ArgumentNullException>(() => this.builder.Build(null));
        }
    }
}
