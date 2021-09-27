using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace HL7TestingTool
{
  public abstract class TestSuiteBuilderBase : ITestSuiteBuilder
  {
    /// <summary>
    /// Property containing this test suites list of test steps.
    /// </summary>
    public abstract List<TestStep> TestSteps { get; set; }
    public abstract List<TestStep> GetTestCases(int caseNumber);
    public abstract TestStep GetTestStep(int caseNumber, int stepNumber);
    public abstract List<ExpectedResult> GetExpectedResults(TestStep testStep);

    public List<ExpectedResult> AddExpectedResults(TestStep testStep, List<ExpectedResult> expectedResults)
    {
      throw new NotImplementedException();
    }

    public TestStep AddTestCase(int caseNumber) => new TestStep(caseNumber);

    public TestStep AddTestStep(TestStep testStep, int stepNumber, string message)
    {
      if (testStep.CaseNumber is null)
        throw new Exception("Error: No test case exists for this test step.");

      testStep.StepNumber = stepNumber;
      testStep.Message = message;
      return testStep;
    }
    public void ImportTestData(string filePath)
    {
      string[] testDataFiles = Directory.GetFiles(filePath);
      for (int i = 0; i < testDataFiles.Length; i++)//Iterating through all the files in the array
      {
        string testStepPath = testDataFiles[i].ToString(); //getting the path of the file including the file name

        // =========================================================  Parsing filenames based on convention 'TEST-CR-##-##'
        // Get case number and add the test case.
        string caseNumber = testStepPath.Substring(testStepPath.Length - 9, 2); // getting the test case number of the file from its name
        Int32.TryParse(caseNumber, out int testCaseNumber); //parsing the test case number to an int
        TestStep testStep = AddTestCase(testCaseNumber);

        // Get step number and add it to the test step for the same test case abstract object.
        string stepNumber = testStepPath.Substring(testStepPath.Length - 6, 2); //getting the test step number of the file from its name
        Int32.TryParse(stepNumber, out int testStepNumber);// parsing the test step number to an int
        testStep.StepNumber = testStepNumber;

        // Get message and add it to the test step.
        testStep.Message = System.IO.File.ReadAllText($@"{testStepPath}"); //getting the message of the file

        TestSteps.Add(testStep);
      }
    }
  }
}
