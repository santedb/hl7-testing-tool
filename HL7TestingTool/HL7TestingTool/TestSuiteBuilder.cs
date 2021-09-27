using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  public class TestSuiteBuilder : TestSuiteBuilderBase
  {
    public TestSuiteBuilder() { }

    private List<TestStep> _testSteps;
    public override List<TestStep> TestSteps { get => _testSteps; set => _testSteps = value; }

    public override List<TestStep> GetTestCases(int caseNumber) => TestSteps.Where(ts => ts.CaseNumber == caseNumber).ToList();

    public override TestStep GetTestStep(int caseNumber, int stepNumber) => TestSteps.Find(ts => ts.CaseNumber == caseNumber && ts.StepNumber == stepNumber);

    public override List<ExpectedResult> GetExpectedResults(TestStep testStep) { throw new NotImplementedException(); }
  }
}
