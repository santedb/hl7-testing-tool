using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace HL7TestingTool
{
    /// <summary>
    /// 
    /// </summary>
    public class TestSuiteBuilder
    {
        /// <summary>
        /// Property containing this test suites list of test steps.
        /// </summary>
        private List<TestStep> _testSteps = new List<TestStep>();

        /// <summary>
        /// 
        /// </summary>
        private List<TestStep> TestSteps
        {
            get => this._testSteps;
            set => this._testSteps = value;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <param name="description"></param>
        private void AddTestStep(int caseNumber, int stepNumber, string message)
        {
            this.TestSteps.Add(new TestStep(caseNumber, stepNumber, message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <param name="description"></param>
        private void AddTestStep(string description, int caseNumber, int stepNumber)
        {
            this.TestSteps.Add(new TestStep(description, caseNumber, stepNumber));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <param name="message"></param>
        /// <param name="description"></param>
        private void AddTestStep(string description, int caseNumber, int stepNumber, string message)
        {
            this.TestSteps.Add(new TestStep(description, caseNumber, stepNumber, message));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <param name="message"></param>
        /// <param name="description"></param>
        /// <param name="assertions"></param>
        private void AddTestStep(int caseNumber, int stepNumber, string message, List<Assertion> assertions)
        {
            this.TestSteps.Add(new TestStep(caseNumber, stepNumber, message, assertions));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <param name="message"></param>
        /// <param name="description"></param>
        /// <param name="assertions"></param>
        private void AddTestStep(string description, int caseNumber, int stepNumber, string message, List<Assertion> assertions)
        {
            this.TestSteps.Add(new TestStep(description, caseNumber, stepNumber, message, assertions));
        }

        /// <summary>
        /// Parsing test step file names with convention 'OHIE-CR-##-##' and the contents of each XML file to build a test suite.
        /// </summary>
        /// <param name="testStepPaths"></param>
        public void Build(List<string> testStepPaths)
        {
            foreach (var path in testStepPaths) //Iterating through all the files in the array
            {
                var splitPath = path.Split('\\');
                var stepAssertions = new List<Assertion>();
                int.TryParse(splitPath[splitPath.Length - 1].Split('-')[2], out var testCaseNumber); // parse case number as int
                int.TryParse(splitPath[splitPath.Length - 1].Split('-')[3].Split('.')[0], out var testStepNumber); // parse step number as int

                var xml = XDocument.Load(path); // Load XML file at the path for current case and step
                var rootDescendants = xml.Root.Descendants();

                // Parse this step's description from XML with LINQ
                var description = from elements in rootDescendants
                    where elements.Name == "description"
                    select elements;
                // Parse this step's message from XML with LINQ
                var message = from elements in rootDescendants
                    where elements.Name == "message"
                    select elements;
                // Parse this step's assertions from XML with LINQ
                var assertions = from elements in rootDescendants
                    where elements.Name == "assert"
                    select elements;

                foreach (var a in assertions) // Create an assertions list to add to this step.
                {
                    if (a.Attribute("alternate") != null) // ALTERNATE ASSERTION
                    {
                        stepAssertions.Add(new Assertion(a.Attribute("terserString").Value, a.Attribute("value").Value, a.Attribute("alternate").Value));
                    }
                    else // Check for other possible non-mandatory assertion ('missing' assertion)
                    {
                        if (a.Attribute("missing") != null) // POSSIBLY A MISSING SEGMENT ASSERTION
                        {
                            if (a.Attribute("missing").Value.ToLower() == "true") // Value of true (not case-sensitive) for 'missing' assertion
                            {
                                stepAssertions.Add(new Assertion(a.Attribute("terserString").Value, true));
                            }
                            else // NOT MISSING ASSERTION
                            {
                                stepAssertions.Add(new Assertion(a.Attribute("terserString").Value, false));
                            }
                        }
                        else // DEFAULT: MANDATORY ASSERTION
                        {
                            stepAssertions.Add(new Assertion(a.Attribute("terserString").Value, a.Attribute("value").Value));
                        }
                    }
                }

                //Create test steps from XML configuration data and add it to the test suite based on test step's XML configuration
                // 1. Only description
                if (description.Any() && !message.Any() && !assertions.Any())
                {
                    this.AddTestStep(description.First().Value, testCaseNumber, testStepNumber);
                }

                // 2. Only message, no description, no assertions
                else if (!description.Any() && message.Any() && !assertions.Any())
                {
                    this.AddTestStep(testCaseNumber, testStepNumber, message.First().Value);
                }

                // 3. No description, with message, with assertions
                else if (!description.Any() && message.Any() && assertions.Any())
                {
                    this.AddTestStep(testCaseNumber, testStepNumber, message.First().Value, stepAssertions);
                }

                // 4. With description, with message
                else if (description.Any() && message.Any() && !assertions.Any())
                {
                    this.AddTestStep(description.First().Value, testCaseNumber, testStepNumber, message.First().Value);
                }

                // 5. Full test step (description, message, assertions)
                else if (description.Any() && message.Any() && assertions.Any())
                {
                    this.AddTestStep(description.First().Value, testCaseNumber, testStepNumber, message.First().Value, stepAssertions);
                }

                // 6. Any other test steps
                else
                {
                    throw new Exception($"ERROR: Test Case #{testCaseNumber} Step #{testStepNumber} could not be read from test suite XML configuration data. Either no description");
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <returns></returns>
        public List<TestStep> GetTestCase(int caseNumber)
        {
            try
            {
                return this.TestSteps.Where(ts => ts.CaseNumber == caseNumber).ToList();
            }
            catch
            {
                return new List<TestStep>();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseNumber"></param>
        /// <param name="stepNumber"></param>
        /// <returns></returns>
        public TestStep GetTestStep(int caseNumber, int stepNumber)
        {
            try
            {
                return this.TestSteps.Find(ts => ts.CaseNumber == caseNumber && ts.StepNumber == stepNumber);
            }
            catch
            {
                return new TestStep();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<TestStep> GetTestSuite()
        {
            return this.TestSteps;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public List<string> Import(string filePath)
        {
            return Directory.EnumerateFiles(filePath).OrderBy(Path.GetFileName).ToList();
        }
    }
}