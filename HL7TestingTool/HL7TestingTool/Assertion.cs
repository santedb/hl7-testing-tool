using NHapi.Base.Model;
using NHapi.Base.Util;
using NHapi.Model.V25.Message;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  /// <summary>
  /// 
  /// </summary>
  public class Assertion
  {
    /// <summary>
    /// Default constructor.
    /// </summary>
    public Assertion() { }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="terserString"></param>
    /// <param name="value"></param>
    public Assertion(string terserString, string value)
    {
      TerserString = terserString;
      Value = value;
      Alternate = false;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="terserString"></param>
    /// <param name="missing"></param>
    public Assertion(string terserString, bool missing)
    {
      TerserString = terserString;
      Missing = missing;
      Alternate = false;
      Value = null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="terserString"></param>
    /// <param name="value"></param>
    /// <param name="alternate"></param>
    public Assertion(string terserString, string value, bool alternate)
    {
      TerserString = terserString;
      Value = value;
      Alternate = alternate;
    }

    /// <summary>
    /// 
    /// </summary>
    public string TerserString { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Alternate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Missing { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool? Outcome { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override string ToString() => Alternate ? $"Assert alternate value '{Value}' at '{TerserString}' has outcome of '{Outcome}'" 
      : Missing ? $"Assert missing value at '{TerserString}' has outcome of '{Outcome}'" 
      : $"Assert mandatory value '{Value}' at '{TerserString}' has outcome of '{Outcome}'";
  }
}
