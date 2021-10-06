using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  /// <summary>
  /// 
  /// </summary>
  public abstract class TestCase
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public TestCase() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caseNumber"></param>
    public TestCase(int caseNumber)
    {
      CaseNumber = caseNumber;
    }

    /// <summary>
    /// 
    /// </summary>
    public int CaseNumber { get; private set; }
  }
}
