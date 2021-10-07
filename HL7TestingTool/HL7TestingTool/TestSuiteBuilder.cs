using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;

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
    private List<TestStep> TestSteps { get => _testSteps; set => _testSteps = value; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public string[] Import(string filePath) { return Directory.GetFiles(filePath); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="description"></param>
    private void AddTestStep(int caseNumber, int stepNumber, string message) => TestSteps.Add(new TestStep(caseNumber, stepNumber, message));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="description"></param>
    private void AddTestStep(string description, int caseNumber, int stepNumber) => TestSteps.Add(new TestStep(description, caseNumber, stepNumber));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="description"></param>
    private void AddTestStep(string description, int caseNumber, int stepNumber, string message ) => TestSteps.Add(new TestStep(description, caseNumber, stepNumber, message));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="description"></param>
    /// <param name="assertions"></param>
    private void AddTestStep(int caseNumber, int stepNumber, string message, List<Assertion> assertions) => TestSteps.Add(new TestStep(caseNumber, stepNumber, message, assertions));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="description"></param>
    /// <param name="assertions"></param>
    private void AddTestStep(string description, int caseNumber, int stepNumber, string message, List<Assertion> assertions) => TestSteps.Add(new TestStep(description, caseNumber, stepNumber, message, assertions));

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<TestStep> GetTestSuite() => TestSteps;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <returns></returns>
    public List<TestStep> GetTestCase(int caseNumber)
    {
      try
      {
        return TestSteps.Where(ts => ts.CaseNumber == caseNumber).ToList();
      } 
      catch (Exception ex)
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
        return TestSteps.Find(ts => ts.CaseNumber == caseNumber && ts.StepNumber == stepNumber);
      }
      catch (Exception ex)
      {
        return new TestStep();
      }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="testStepPaths"></param>
    public void Build(string[] testStepPaths)
    {
      foreach (string path in testStepPaths)//Iterating through all the files in the array
      {
        List<Assertion> stepAssertions = new List<Assertion>();

        // =========================================================  Parsing filenames based on convention 'TEST-CR-##-##'
        string caseNumber = path.Substring(path.Length - 9, 2); // getting the test case number of the file from its name
        Int32.TryParse(caseNumber, out int testCaseNumber); //parsing the test case number to an int

        string stepNumber = path.Substring(path.Length - 6, 2); //getting the test step number of the file from its name
        Int32.TryParse(stepNumber, out int testStepNumber);// parsing the test step number to an int

        // Parse this step's message from XML with LINQ
        XDocument xml = XDocument.Load(path);
        IEnumerable<XElement> rootDescendants = xml.Root.Descendants();

        IEnumerable<XElement> description = from elements in rootDescendants
                                            where elements.Name == "description"
                                            select elements;

        IEnumerable<XElement> message = from elements in rootDescendants
                                        where elements.Name == "message"
                                        select elements;

        // Parse this step's  assertions from XML with LINQ
        IEnumerable<XElement> assertions = from elements in rootDescendants
                                           where elements.Name == "assert"
                                           select elements;

        // Create an assertions list to add to this step.
        foreach (XElement a in assertions)
        {
          if (a.Attribute("alternate") == null)
            stepAssertions.Add(new Assertion(a.Attribute("terserString").Value, a.Attribute("value").Value));
          else
          {
            if (a.Attribute("alternate").Value.ToLower() == "true")
              stepAssertions.Add(new Assertion(a.Attribute("terserString").Value, a.Attribute("value").Value, true));
            else
              stepAssertions.Add(new Assertion(a.Attribute("terserString").Value, a.Attribute("value").Value));
          }
        }

        //NOTE: Find out about pre-conditions and what the assumption is for those
        //  add this to the test step model as a constructor available to the
        //  TestSuiteBuilder and overload with another AddTestStep method

        //Create test steps from XML configuration data and add it to the test suite based on test step's XML configuration
        // 1. Only description
        if (description.Any() && !message.Any() && !assertions.Any())
          AddTestStep(description.First().Value, testCaseNumber, testStepNumber);

        // 2. Only message, no description, no assertions
        else if (!description.Any() && message.Any() && !assertions.Any())
          AddTestStep(testCaseNumber, testStepNumber, message.First().Value);

        // 3. No description, with message, with assertions
        else if (!description.Any() && message.Any() && assertions.Any())
          AddTestStep(testCaseNumber, testStepNumber, message.First().Value, stepAssertions);

        // 4. With description, with message
        else if (description.Any() && message.Any() && !assertions.Any())
          AddTestStep(description.First().Value, testCaseNumber, testStepNumber, message.First().Value);

        // 5. Full test step (description, message, assertions)
        else if (description.Any() && message.Any() && assertions.Any())
          AddTestStep(description.First().Value, testCaseNumber, testStepNumber, message.First().Value, stepAssertions);
        
        // 6. Any other test steps
        else
          throw new Exception($"ERROR: Test Case #{testCaseNumber} Step #{stepNumber} could not be read from test suite XML configuration data. Either no description");
      }
    }   
  }
}
