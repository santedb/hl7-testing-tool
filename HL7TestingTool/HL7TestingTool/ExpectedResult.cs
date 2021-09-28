using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  public class ExpectedResult
  {
    public ExpectedResult() { }
    public ExpectedResult(string number) { Number = number; }
    public string Number { get; set; }
    public List<Assertion> Assertions { get; set; }
  }
}
