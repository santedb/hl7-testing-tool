﻿using HL7TestingTool.Interop;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NHapi.Base;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Serilog;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// Represents a test suite director.
    /// </summary>
    public class TestSuiteBuilderDirector : ITestExecutor
    {
        /// <summary>
        /// 
        /// </summary>
        private const string URI = "llp://127.0.0.1:2100";

        /// <summary>
        /// Builder that imoports test data from files and results in a list of test steps organizaed by test case number and test step number.
        /// </summary>
        private readonly TestSuiteBuilder testSuiteBuilder;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<TestSuiteBuilderDirector> logger;

        private readonly IMllpMessageSender messageSender;

        /// <summary>
        /// Director that can access a TestSuiteBuilder's interface methods and overridden abstract methods used to build a test suite.
        /// </summary>
        /// <param name="testSuiteBuilder"></param>
        /// <param name="filePath"></param>
        public TestSuiteBuilderDirector(IConfiguration configuration, ILogger<TestSuiteBuilderDirector> logger, IMllpMessageSender messageSender)
        {
            this.testSuiteBuilder = new TestSuiteBuilder();
            this.configuration = configuration;
            this.logger = logger;
            this.messageSender = messageSender;
            this.FilePath = this.configuration.GetValue<string>("TestDirectory");
            this.BuildFromXml();
        }

        /// <summary>
        /// Path to the files containing messages.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testStep"></param>
        /// <param name="response"></param>
        private void Assert(TestStep testStep, IMessage response)
        {
            var terser = new Terser(response);
            string found;
            var testFail = false;
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("***Remove this: Assertion Count:" + testStep.Assertions.Count + "***");
            foreach (var assertion in testStep.Assertions)
            {
                try // Getting a value from a response terser
                {
                    found = terser.Get(assertion.TerserString);
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
                            found = assertion.TerserString;
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
                            found = assertion.TerserString;
                            assertion.Outcome = true;
                        }
                    }
                }

                
                if (!(bool)assertion.Outcome)
                {
                    assertion.Outcome = assertion.Alternates.Any(a => a.Value == found);
                } 
                var status = (bool) assertion.Outcome ? "PASSED" : "FAILED";

                this.logger.LogInformation($"{assertion}: {status}, Actual: '{found}'");
            }

            testFail = testStep.Assertions.Any(c => c.Outcome.HasValue && !c.Outcome.Value);
            if (testFail)
            {
                var pipeParserResponse = new PipeParser().Encode(response);
                this.logger.LogInformation(pipeParserResponse);
            }

        }

        /// <summary>
        /// Director builds the test suite by calling the implemented Import() method to get filepaths as a string array for the Build() method.
        /// </summary>
        public void BuildFromXml()
        {
            this.testSuiteBuilder.Build(this.testSuiteBuilder.Import(this.FilePath));
        }

        /// <summary>
        /// Method used to convert a string of hexadecimal values (2 characters each).
        /// This is used in this program to ensure that CRLF line endings appear in all messages being parsed.
        /// HL7 messages always expect CR to separate segments and having LF line endings will throw an Exception.
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public string ConvertHex(string hexString)
        {
            try
            {
                var ascii = string.Empty;

                for (var i = 0; i < hexString.Length; i += 2)
                {
                    var hs = string.Empty;

                    hs = hexString.Substring(i, 2);
                    var decval = Convert.ToUInt32(hs, 16);
                    var character = Convert.ToChar(decval);
                    ascii += character;
                }

                return ascii;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex.Message);
                return ex.Message;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private string ConvertLineEndings(string message)
        {
            var bytes = Encoding.ASCII.GetBytes(message);
            var hex = BitConverter.ToString(bytes).Replace("-", "");


            var ASCIIHexString = new StringBuilder();
            for (var i = 1; i < hex.Length; i += 2)
            {
                //Check for a LF (line feed)
                if (hex[i - 1] == '0' && hex[i] == 'A') // Check for 0A at current index while incrementing by 2
                {
                    //Check for a CR (carriage return)
                    //handle the case with LF on a line by itself (blank line)
                    if (hex[i - 2] == 'D') // Checking: _0A   Current ASCIIHexString = ...0 
                    {
                        if (hex[i - 3] != '0') // Checking: _D0A   Current ASCIIHexString = ...0
                        {
                            ASCIIHexString.Append("0D");
                            ASCIIHexString.Append(hex[i - 1]);
                            ASCIIHexString.Append(hex[i]); // Needed to append D0A to ASCIIHexString = ...0 to make it as ...0D0A
                        }
                        else // Current ASCIIHexString = ...0D0 (no action needed - just append A)
                        {
                            ASCIIHexString.Append(hex[i - 1]);
                            ASCIIHexString.Append(hex[i]);
                        }
                    }
                    else // Current ASCIIHexString = ...**0
                    {
                        ASCIIHexString.Append("0D");
                        ASCIIHexString.Append(hex[i - 1]);
                        ASCIIHexString.Append(hex[i]); // Needed to append D0A to ASCIIHexString = ...0 to make it as ...0D0A
                    }
                }
                else
                {
                    ASCIIHexString.Append(hex[i - 1]);
                    ASCIIHexString.Append(hex[i]);
                }
            }

            return this.ConvertHex(ASCIIHexString.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testSteps"></param>
        /// <returns></returns>
        public IEnumerable<IMessage> ExecuteTestSteps()
        {
            var testSteps = this.testSuiteBuilder.GetTestSuite();
            var config = this.configuration.GetSection("TestOptions").GetSection("Execution")
                .Get<string[]>();

            if (config == null || config?.Any(c => c == "*") == true )
            {
                this.logger.LogWarning("No test execution configuration or * detected, therefore all tests will be executed");

            } 
            else
            {
                // HACK: left pad 0 when test case/test step numbers are less than 10 for comparisons
                testSteps = testSteps.Where(t => config.Contains($"OHIE-CR-{(t.CaseNumber < 10 ? "0"+ t.CaseNumber : t.CaseNumber)}-{(t.StepNumber <10 ? "0"+t.StepNumber : t.StepNumber)}")).ToList();
                if (!testSteps.Any())
                {
                    this.logger.LogError("Invalid test execution parameter(s)");
                    this.logger.LogDebug("Unable to find matching test(s) specified in configuration");
                    throw new InvalidOperationException("Invalid test execution parameter(s)");
                }
            }
            

            this.logger.LogInformation("Executing Test(s)");
            this.logger.LogInformation("Remote Address: " + URI);
            var responses = new List<IMessage>();
            foreach (var t in testSteps)
            {
                try
                {
                    IMessage response = null;
                    // Check for messages before sending and asserting
                    if (t.Message != null)
                    {
                        response = this.SendHL7Message(t); // Send encoded message with MllpMessageSender
                        responses.Add(response); // Add response to list to be returned
                        if (t.Assertions.Count > 0) // Check for assertions before asserting values
                        {
                            this.Assert(t, response); // Process assertions for a step 
                        }
                    }
                    //else
                    //{
                    //    //NOTE: Find out about pre-conditions and what the assumption is for those
                    //    //  add this to the test step model as a constructor available to the
                    //    //  TestSuiteBuilder and overload with another AddTestStep method
                    //    Console.WriteLine($"{t}: {t.Description}");
                    //}
                }
                catch (HL7Exception e) // Can catch an Exception with missing segment 
                {
                    if (e.SegmentName == null) //CAUGHT MISSING SEGMENT CONDITION
                    {
                        continue;
                    }

                    this.logger.LogError($"Exception thrown: {e.Message}\n");
                }
            }

            return responses;
        }

        /// <summary>
        /// Access to abstract base method for retrieving a list of test steps for a specific test case.
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public List<TestStep> GetTestCase(int caseNumber)
        {
            return this.testSuiteBuilder.GetTestCase(caseNumber);
        }

        /// <summary>
        /// Access to abstract base method for retrieving a specific test step that belongs to a specific test case.
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <returns></returns>
        public TestStep GetTestStep(int caseNumber, int stepNumber)
        {
            return this.testSuiteBuilder.GetTestStep(caseNumber, stepNumber);
        }

        /// <summary>
        /// Access to abstract base method for retrieving all test steps.
        /// </summary>
        /// <returns></returns>
        public List<TestStep> GetTestSuite()
        {
            return this.testSuiteBuilder.GetTestSuite();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private IMessage SendHL7Message(TestStep t)
        {
            var crlfString = this.ConvertLineEndings(t.Message); // Converting LF line endings to CRLF line endings
            this.logger.LogInformation(Environment.NewLine);
            this.logger.LogInformation( $"Test# {t}");

            // Use MllPMessageSender class to get back the response after sending a message
            var responseString = this.messageSender.SendAndReceive(crlfString);
            IMessage response;
            try
            {
                if (responseString.Split('|')[0] == "MSH")
                {
                    var parser = new PipeParser();
                    response = parser.Parse(responseString);
                }
                else
                {
                    var ex = new HL7Exception("Missing MSH")
                    {
                        SegmentName = null
                    };
                    throw ex;
                }
            }
            catch (HL7Exception e)
            {
                throw e;
            }

            return response;
        }
    }
}