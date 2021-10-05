using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;
using System.Linq;

namespace HL7TestingTool
{
  public class TestSuiteBuilder
  {
    /// <summary>
    /// Property containing this test suites list of test steps.
    /// </summary>
    private List<TestStep> _testSteps = new List<TestStep>();
    private List<TestStep> TestSteps { get => _testSteps; set => _testSteps = value; }
    private void AddTestStep(int caseNumber, int stepNumber, string message, List<Assertion> assertions) => TestSteps.Add(new TestStep(caseNumber, stepNumber, message, assertions));
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
        IEnumerable<XElement> message = from elements in xml.Root.Descendants()
                                        where elements.Name == "message"
                                        select elements;

        // Parse this step's  assertions from XML with LINQ
        IEnumerable<XElement> assertions = from elements in xml.Root.Descendants()
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

        //string message = File.ReadAllText($@"{testStepPath}");  //getting the message of the file
        AddTestStep(testCaseNumber, testStepNumber, message.First().Value, stepAssertions);   //Create a test step from the data and add it to the list of test steps for a test suite
      }
    }
    public string[] Import(string filePath) { return Directory.GetFiles(filePath); }
    public List<TestStep> GetTestSuite() => TestSteps;
    public List<TestStep> GetTestCase(int caseNumber) => TestSteps.Where(ts => ts.CaseNumber == caseNumber).ToList();
    public TestStep GetTestStep(int caseNumber, int stepNumber) => TestSteps.Find(ts => ts.CaseNumber == caseNumber && ts.StepNumber == stepNumber);
  }
}
