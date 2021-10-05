using NHapi.Base.Model;
using NHapi.Base.Util;
using NHapi.Model.V25.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  public class Assertion
  {
    public Assertion() { }
    public Assertion(string terserString, string value)
    {
      TerserString = terserString;
      Value = value;
      Alternate = false;
    }
    public Assertion(string terserString, string value, bool alternate)
    {
      TerserString = terserString;
      Value = value;
      Alternate = alternate;
    }
    public string TerserString { get; set; }
    public string Value { get; set; }
    public bool Alternate { get; set; }
    public bool? Outcome { get; set; }

    public override string ToString() => Alternate ? 
      $"Assert alternate value '{Value}' at '{TerserString}' has outcome of '{Outcome}'" 
      : $"Assert mandatory value '{Value}' at '{TerserString}' has outcome of '{Outcome}'";
  }
}
