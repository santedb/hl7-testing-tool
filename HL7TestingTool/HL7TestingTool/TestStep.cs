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
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="description"></param>
    public TestStep(string description, int caseNumber, int stepNumber) :base(caseNumber)
    {
      StepNumber = stepNumber;
      Description= description;
    }

    /// <summary>
    /// 
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
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="description"></param>
    public TestStep(string description, int caseNumber, int stepNumber, string message) : base(caseNumber) 
    {
      StepNumber = stepNumber;
      Message = message;
      Description = description;
    }

    /// <summary>
    /// Test step with all properties
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="assertions"></param>
    public TestStep(int caseNumber, int stepNumber, string message, List<Assertion> assertions) : base(caseNumber)
    {
      StepNumber = stepNumber;
      Message = message;
      Assertions = assertions;
    }

    /// <summary>
    /// Test step with all properties
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <param name="message"></param>
    /// <param name="description"></param>
    /// <param name="assertions"></param>
    public TestStep(string description, int caseNumber, int stepNumber, string message, List<Assertion> assertions) : base(caseNumber)
    {
      StepNumber = stepNumber;
      Assertions = assertions;
      Message = message;
      Description = description;
    }

    /// <summary>
    /// 
    /// </summary>
    public int? StepNumber { get; private set; }

    /// <summary>
    /// 
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<Assertion> Assertions { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString() => $"TEST-CR-{CaseNumber}-{StepNumber}";
  }
}
