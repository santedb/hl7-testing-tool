using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  /// <summary>
  /// Interface supplied with methods for buiilding a test suite composed of various classes representing test cases, steps, and expected behaviours.
  /// </summary>
  partial interface ITestSuiteBuilder
  {
    /// <summary>
    /// Test suites need testing data including case numbers, step numbers, message, and expected results list.
    /// A file with this test data should be included with the project to test with by providing its path.
    /// </summary>
    /// <param name="file"></param>
    void ImportTestData(string filePath);

    /// <summary>
    /// Test suites are built using test steps and associated list of expected results.
    /// This method adds a test case to the test suite.
    /// </summary>
    /// <param name="caseNumber"></param>
    public void AddTestCase(int caseNumber);

    /// <summary>
    /// Test steps are built into a specific test case based on a test case number.
    /// </summary>
    /// <param name="caseNumber"></param>
    public void AddTestStep(int caseNumber, int stepNumber, string message);

    /// <summary>
    /// Expected results are built into a specific test step.
    /// This method adds an expected result to a test step.
    /// </summary>
    /// <param name="testStep"></param>
    public void AddExpectedResults(TestStep testStep, List<ExpectedResult> expectedResults);

    /// <summary>
    /// Specific test steps can have only the message updated.
    /// This method updates the message for a specific case number and step number.
    /// </summary>
    /// <param name="testStep"></param>
    /// <param name="message"></param>
    partial void UpdateTestStep(TestStep testStep, string message);

    /// <summary>
    /// Specific test steps can have only the expected results updated.
    /// This method updates the expected results for a specific case number and step number.
    /// </summary>
    /// <param name="testStep"></param>
    /// <param name="expectedResults"></param>
    partial void UpdateTestStep(TestStep testStep, List<ExpectedResult> expectedResults);

    /// <summary>
    /// Specific test steps can have both the message and expected results updated.
    /// This method updates the message and expected results for a specific case number and step number.
    /// </summary>
    /// <param name="testStep"></param>
    /// <param name="message"></param>
    /// <param name="expectedResults"></param>
    partial void UpdateTestStep(TestStep testStep, string message, List<ExpectedResult> expectedResults);
    public List<TestStep> TestSteps { get; set; }
  }
}
