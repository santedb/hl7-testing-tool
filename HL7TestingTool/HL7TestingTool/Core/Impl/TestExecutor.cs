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
        /// 
        /// </summary>
        /// <param name="testStep"></param>
        /// <param name="response"></param>
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

                var status = (bool) assertion.Outcome ? "PASSED" : "FAILED";

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
            this.testSuiteBuilder.Build(this.testSuiteBuilder.Import(this.filePath));
        }

        ///// <summary>
        ///// Method used to convert a string of hexadecimal values (2 characters each).
        ///// This is used in this program to ensure that CRLF line endings appear in all messages being parsed.
        ///// HL7 messages always expect CR to separate segments and having LF line endings will throw an Exception.
        ///// </summary>
        ///// <param name="hexString"></param>
        ///// <returns></returns>
        //public string ConvertHex(string hexString)
        //{
        //    try
        //    {
        //        var ascii = string.Empty;

        //        for (var i = 0; i < hexString.Length; i += 2)
        //        {
        //            var hs = string.Empty;

        //            hs = hexString.Substring(i, 2);
        //            var decval = Convert.ToUInt32(hs, 16);
        //            var character = Convert.ToChar(decval);
        //            ascii += character;
        //        }

        //        return ascii;
        //    }
        //    catch (Exception ex)
        //    {
        //        this.logger.LogError(ex.Message);
        //        return ex.Message;
        //    }
        //}

        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="message"></param>
        ///// <returns></returns>
        //private string ConvertLineEndings(string message)
        //{
        //    var bytes = Encoding.ASCII.GetBytes(message);
        //    var hex = BitConverter.ToString(bytes).Replace("-", "");
        //    var ASCIIHexString = new StringBuilder();

        //    for (var i = 1; i < hex.Length; i += 2)
        //    {
        //        //Check for a LF (line feed)
        //        if (hex[i - 1] == '0' && hex[i] == 'A') // Check for 0A at current index while incrementing by 2
        //        {
        //            //Check for a CR (carriage return)
        //            //handle the case with LF on a line by itself (blank line)
        //            if (hex[i - 2] == 'D') // Checking: _0A   Current ASCIIHexString = ...0 
        //            {
        //                if (hex[i - 3] != '0') // Checking: _D0A   Current ASCIIHexString = ...0
        //                {
        //                    ASCIIHexString.Append("0D");
        //                    ASCIIHexString.Append(hex[i - 1]);
        //                    ASCIIHexString.Append(hex[i]); // Needed to append D0A to ASCIIHexString = ...0 to make it as ...0D0A
        //                }
        //                else // Current ASCIIHexString = ...0D0 (no action needed - just append A)
        //                {
        //                    ASCIIHexString.Append(hex[i - 1]);
        //                    ASCIIHexString.Append(hex[i]);
        //                }
        //            }
        //            else // Current ASCIIHexString = ...**0
        //            {
        //                ASCIIHexString.Append("0D");
        //                ASCIIHexString.Append(hex[i - 1]);
        //                ASCIIHexString.Append(hex[i]); // Needed to append D0A to ASCIIHexString = ...0 to make it as ...0D0A
        //            }
        //        }
        //        else
        //        {
        //            ASCIIHexString.Append(hex[i - 1]);
        //            ASCIIHexString.Append(hex[i]);
        //        }
        //    }

        //    return this.ConvertHex(ASCIIHexString.ToString());
        //}

        /// <summary>
        /// Executes the tests.
        /// </summary>
        /// <returns>Returns the list of response messages.</returns>
        public IEnumerable<IMessage> ExecuteTestSteps()
        {
            var testSteps = this.testSuiteBuilder.GetTestSuite();
            var testConfiguration = this.configuration.GetSection("TestOptions:Execution").Get<string[]>();

            if (testConfiguration == null || testConfiguration?.Any(c => c == "*") == true )
            {
                this.logger.LogWarning("No test execution configuration or '*' detected, therefore all tests will be executed");
            }
            else
            {
                // HACK: left pad 0 when test case/test step numbers are less than 10 for comparisons
                testSteps = testSteps.Where(t => testConfiguration.Contains($"OHIE-CR-{(t.CaseNumber < 10 ? "0"+ t.CaseNumber : t.CaseNumber)}-{(t.StepNumber <10 ? "0"+t.StepNumber : t.StepNumber)}")).ToList();

                if (!testSteps.Any())
                {
                    this.logger.LogError("Unable to find matching test(s) specified in configuration");
                    throw new InvalidOperationException("Invalid test execution parameter(s)");
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

            return responses;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="testStep"></param>
        /// <returns></returns>
        private IMessage SendHl7Message(TestStep testStep)
        {
            //var crlfString = this.ConvertLineEndings(testStep.Message); // Converting LF line endings to CRLF line endings

            this.logger.LogInformation(Environment.NewLine);
            this.logger.LogInformation( $"Test# {testStep}");

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