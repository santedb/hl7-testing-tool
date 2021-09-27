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
    public abstract List<TestStep> GetTestCases(int caseNumber);
    public abstract TestStep GetTestStep(int caseNumber, int stepNumber);
    public abstract List<ExpectedResult> GetExpectedResults(TestStep testStep);

    private List<TestStep> _testSteps = new List<TestStep>();
    public List<TestStep> TestSteps { get => _testSteps; set => _testSteps = value; }

    public void AddExpectedResults(TestStep testStep, List<ExpectedResult> expectedResults)
    {
      throw new NotImplementedException();
    }

    public void AddTestCase(int caseNumber) => TestSteps.Add(new TestStep(caseNumber));
    public void AddTestStep(int caseNumber, int stepNumber, string message) => TestSteps.Add(new TestStep(caseNumber, stepNumber, message));
    public void ImportTestData(string filePath)
    {
      string[] testDataFiles = Directory.GetFiles(filePath);
      for (int i = 0; i < testDataFiles.Length; i++)//Iterating through all the files in the array
      {
        string testStepPath = testDataFiles[i].ToString(); //getting the path of the file including the file name

        // =========================================================  Parsing filenames based on convention 'TEST-CR-##-##'

        string caseNumber = testStepPath.Substring(testStepPath.Length - 9, 2); // getting the test case number of the file from its name
        Int32.TryParse(caseNumber, out int testCaseNumber); //parsing the test case number to an int

        string stepNumber = testStepPath.Substring(testStepPath.Length - 6, 2); //getting the test step number of the file from its name
        Int32.TryParse(stepNumber, out int testStepNumber);// parsing the test step number to an int
        
        string message = File.ReadAllText($@"{testStepPath}");  //getting the message of the file
        AddTestStep(testCaseNumber, testStepNumber, message);   //Create a test step from the data and add it to the list of test steps for a test suite
      }
    }
  }
}
