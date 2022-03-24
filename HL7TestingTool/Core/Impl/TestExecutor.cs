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
 * User: Tommy Zieba, Azabelle Tale, Shihab Khan, Nityan Khanna
 * Date: 2022-03-16
 */

using HL7TestingTool.Interop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHapi.Base;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// Represents a test executor.
    /// </summary>
    public class TestExecutor : ITestExecutor
    {
        /// <summary>
        /// The pipe parser.
        /// </summary>
        private static readonly PipeParser parser = new();

        /// <summary>
        /// The file path to the tests.
        /// </summary>
        private readonly string filePath;

        /// <summary>
        /// Builder that imports test data from files and results in a list of test steps organized by test case number and test step number.
        /// </summary>
        private readonly TestSuiteBuilder testSuiteBuilder;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<TestExecutor> logger;

        /// <summary>
        /// The message sender.
        /// </summary>
        private readonly IMllpMessageSender messageSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestExecutor"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="messageSender">The message sender.</param>
        public TestExecutor(IConfiguration configuration, ILogger<TestExecutor> logger, IMllpMessageSender messageSender)
        {
            this.testSuiteBuilder = new TestSuiteBuilder();
            this.configuration = configuration;
            this.logger = logger;
            this.messageSender = messageSender;
            this.filePath = this.configuration.GetValue<string>("TestDirectory");
            this.Initialize();
        }

        /// <summary>
        /// Checks the test step assertions against response to evaluate outcome.
        /// </summary>
        /// <param name="testStep">The test step.</param>
        /// <param name="response">The response.</param>
        private void Assert(TestStep testStep, IMessage response)
        {
            var terser = new Terser(response);
            string found;

            foreach (var assertion in testStep.Assertions)
            {
                try // Getting a value from a response terser
                {
                    found = terser.Get(assertion.Terser);
                    if (assertion.Value == null && assertion.Missing) // SHOULD be missing
                    {
                        assertion.Outcome = found == null;
                    }
                    else // SHOULD NOT be missing
                    {
                        assertion.Outcome = found == assertion.Value;
                    }
                }
                catch (HL7Exception ex) // and handle case where a missing segment occurs 
                {
                    found = "null";
                    if (assertion.Missing) // Check for a SegmentName in the HL7Exception
                    {
                        if (ex.SegmentName == null) // Assertion passes
                        {
                            assertion.Outcome = true;
                        }
                        else // Assertion fails
                        {
                            found = assertion.Terser;
                            assertion.Outcome = false;
                        }
                    }
                    else // Assertion is for required value (not missing)
                    {
                        if (ex.SegmentName == null) // Assertion fails
                        {
                            assertion.Outcome = false;
                        }
                        else // Assertion passes
                        {
                            found = assertion.Terser;
                            assertion.Outcome = true;
                        }
                    }
                }


                if (!(bool)assertion.Outcome)
                {
                    assertion.Outcome = assertion.Alternates.Any(a => a.Value == found);
                }

                var status = (bool)assertion.Outcome ? "PASSED" : "FAILED";

                this.logger.LogInformation($"{status} {assertion}: Actual: '{found}'");
            }

            var testFail = testStep.Assertions.Any(c => c.Outcome.HasValue && !c.Outcome.Value);

            if (testFail)
            {
                var encodedResponse = new PipeParser().Encode(response).Replace("\r", Environment.NewLine);

                this.logger.LogDebug(encodedResponse);
            }

        }

        /// <summary>
        /// Director builds the test suite by calling the implemented Import() method to get filepaths as a string array for the Build() method.
        /// </summary>
        private void Initialize()
        {
            this.testSuiteBuilder.Build(this.filePath);
        }

        /// <summary>
        /// Executes the tests.
        /// </summary>
        /// <returns>Returns the list of response messages.</returns>
        public IEnumerable<IMessage> ExecuteTestSteps()
        {
            var testSteps = this.testSuiteBuilder.GetTestSuite();
            var testConfiguration = this.configuration.GetSection("TestOptions:Execution").Get<string[]>();

            if (testConfiguration == null || testConfiguration?.Any(c => c == "*") == true)
            {
                this.logger.LogWarning("No test execution configuration or '*' detected, therefore all tests will be executed");
            }
            else
            {
                // HACK: left pad 0 when test case/test step numbers are less than 10 for comparisons
                testSteps = testSteps.Where(t => testConfiguration.Contains($"OHIE-CR-{(t.CaseNumber < 10 ? "0" + t.CaseNumber : t.CaseNumber)}-{(t.StepNumber < 10 ? "0" + t.StepNumber : t.StepNumber)}")).ToList();

                if (!testSteps.Any())
                {
                    this.logger.LogError("Unable to find matching test(s) specified in configuration");
                    throw new InvalidOperationException("Unable to find matching test(s) specified in configuration");
                }
            }

            this.logger.LogInformation("Executing Test(s)");
            this.logger.LogInformation($"Remote Address: {this.configuration.GetValue<string>("Endpoint")}");

            var responses = new List<IMessage>();

            foreach (var testStep in testSteps)
            {
                try
                {
                    if (testStep.Message == null)
                    {
                        this.logger.LogWarning($"Test: {testStep} does not have a message");
                        continue;
                    }

                    var response = this.SendHl7Message(testStep);
                    responses.Add(response);

                    if (testStep.Assertions.Any())
                    {
                        this.Assert(testStep, response); // Process assertions for a step 
                    }

                    this.logger.LogInformation($"Total Assertions: {testStep.Assertions.Count}. Passed: {testStep.Assertions.Count(c => c.Outcome.HasValue && c.Outcome.Value)}. Failed {testStep.Assertions.Count(c => c.Outcome.HasValue && !c.Outcome.Value)}. Not Run: {testStep.Assertions.Count(c => !c.Outcome.HasValue)}");
                }
                catch (HL7Exception e) // Can catch an Exception with missing segment 
                {
                    if (e.SegmentName == null) //CAUGHT MISSING SEGMENT CONDITION
                    {
                        continue;
                    }

                    this.logger.LogError($"Exception thrown: {e.Message}{Environment.NewLine}");
                }
            }

            this.logger.LogInformation(Environment.NewLine);
            this.logger.LogInformation($"Total Tests: {testSteps.Count}. Passed: {testSteps.Count(c => c.Assertions.All(x => x.Outcome.HasValue && x.Outcome.Value))}. Failed: {testSteps.Count(c => c.Assertions.Any(x => x.Outcome.HasValue && !x.Outcome.Value))}. Not Run: {testSteps.Count(c => c.Assertions.Any(x => !x.Outcome.HasValue))}");

            return responses;
        }

        /// <summary>
        /// Sends an HL7 message,
        /// </summary>
        /// <param name="testStep">The test step.</param>
        /// <returns>Returns the response message.</returns>
        private IMessage SendHl7Message(TestStep testStep)
        {
            this.logger.LogInformation(Environment.NewLine);
            this.logger.LogInformation($"Test# {testStep}");

            // replace all instances of CRLF and LF with CR
            var responseData = this.messageSender.SendAndReceive(testStep.Message.Replace("\n", "\r").Replace("\r\n", "\r"));

            IMessage response = null;

            try
            {
                response = parser.Parse(responseData);
            }
            catch (Exception e)
            {
                this.logger.LogError($"Error processing HL7 response: {e}");
            }

            return response;
        }
    }
}