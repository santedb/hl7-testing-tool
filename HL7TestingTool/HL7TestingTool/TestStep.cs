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
    /// Constructor for a test step with only case number (corresponding to the base class).
    /// </summary>
    /// <param name="caseNumber"></param>
    public TestStep(int caseNumber) : base(caseNumber) { }

    /// <summary>
    /// Test step that only has a step number.
    /// </summary>
    /// <param name="stepNumber"></param>
    /// <param name="caseNumber"></param>
    public TestStep(int caseNumber, int stepNumber) : base(caseNumber) { StepNumber = stepNumber; }

    /// <summary>
    /// Test step with only a message and specific test case number.
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    public TestStep(int caseNumber, int stepNumber, string message) : base(caseNumber)
    {
      StepNumber = stepNumber;
      Message = message;
    }

    /// <summary>
    /// Test step with all properties
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="expectedResults"></param>
    public TestStep(int caseNumber, int stepNumber, string message, List<ExpectedResult> expectedResults) : base(caseNumber)
    {
      StepNumber = stepNumber;
      ExpectedResults = expectedResults;
    }
    public int StepNumber { get; set; }
    public string Message { get; set; }
    public List<ExpectedResult> ExpectedResults { get; set; }
  }
}
