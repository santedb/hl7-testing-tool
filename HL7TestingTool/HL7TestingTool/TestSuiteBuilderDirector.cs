using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  public class TestSuiteBuilderDirector
  {
    /// <summary>
    /// Builder that imoports test data from files and results in a list of test steps organizaed by test case number and test step number.
    /// </summary>
    private TestSuiteBuilder _testSuiteBuilder;

    /// <summary>
    /// Path to the files containing messages.
    /// </summary>
    public string FilePath { get; set; }
    
    /// <summary>
    /// Director that can access a TestSuiteBuilder's interface methods and overridden abstract methods used to build a test suite.
    /// </summary>
    /// <param name="testSuiteBuilder"></param>
    /// <param name="filePath"></param>
    public TestSuiteBuilderDirector(TestSuiteBuilder testSuiteBuilder, string filePath)
    {
      _testSuiteBuilder = testSuiteBuilder;
      FilePath = filePath;
    }

    /// <summary>
    /// Director builds the test suite by calling the implemented ImportTestData() method.
    /// </summary>
    public void BuildFromXml()
    {
      _testSuiteBuilder.Build(_testSuiteBuilder.Import(FilePath));
    }

    /// <summary>
    /// Access to abstract base method for retrieving all test steps.
    /// </summary>
    /// <returns></returns>
    public List<TestStep> GetTestSuite() => _testSuiteBuilder.GetTestSuite();

    /// <summary>
    /// Access to abstract base method for retrieving a list of test steps for a specific test case.
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <returns></returns>
    public List<TestStep> GetTestCase(int caseNumber) => _testSuiteBuilder.GetTestCase(caseNumber);

    /// <summary>
    /// Access to abstract base method for retrieving a specific test step that belongs to a specific test case.
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <returns></returns>
    public TestStep GetTestStep(int caseNumber, int stepNumber) => _testSuiteBuilder.GetTestStep(caseNumber, stepNumber);

  }
}
