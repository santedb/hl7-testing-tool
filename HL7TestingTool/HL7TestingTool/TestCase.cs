using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  public abstract class TestCase
  {
    public TestCase() { }
    public TestCase(int caseNumber)
    {
      CaseNumber = caseNumber;
    }
    public int? CaseNumber { get; /*private*/ set; }
  }
}
