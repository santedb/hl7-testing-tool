using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  /// <summary>
  /// Represents a step for a test case that has a list of ExpectedResults.
  /// </summary>
  public class TestStep : TestCase
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public TestStep() { }

    /// <summary>
    /// Test step with all properties
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="expectedResults"></param>
    public TestStep(int caseNumber, int stepNumber, string message, List<Assertion> assertions) : base(caseNumber)
    {
      StepNumber = stepNumber;
      Assertions = assertions;
      Message = message;
    }
    public int StepNumber { get; private set; }
    public string Message { get; set; }
    public List<Assertion> Assertions { get; set; }

    public override string ToString() => $"TEST-CR-{CaseNumber}-{StepNumber}";
  }
}
