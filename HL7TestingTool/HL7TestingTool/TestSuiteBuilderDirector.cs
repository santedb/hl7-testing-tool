using NHapi.Base;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using NHapi.Base.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace HL7TestingTool
{
  public class TestSuiteBuilderDirector
  {
    /// <summary>
    /// Builder that imoports test data from files and results in a list of test steps organizaed by test case number and test step number.
    /// </summary>
    private TestSuiteBuilder _testSuiteBuilder;

    /// <summary>
    /// 
    /// </summary>
    private const string URI = "llp://127.0.0.1:2100";

    /// <summary>
    /// Path to the files containing messages.
    /// </summary>
    public string FilePath { get; set; }
    
    /// <summary>
    /// Director that can access a TestSuiteBuilder's interface methods and overridden abstract methods used to build a test suite.
    /// </summary>
    /// <param name="testSuiteBuilder"></param>
    /// <param name="filePath"></param>
    public TestSuiteBuilderDirector(TestSuiteBuilder testSuiteBuilder, string filePath)
    {
      _testSuiteBuilder = testSuiteBuilder;
      FilePath = filePath;
    }

    /// <summary>
    /// Access to abstract base method for retrieving all test steps.
    /// </summary>
    /// <returns></returns>
    public List<TestStep> GetTestSuite() => _testSuiteBuilder.GetTestSuite();

    /// <summary>
    /// Access to abstract base method for retrieving a list of test steps for a specific test case.
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <returns></returns>
    public List<TestStep> GetTestCase(int caseNumber) => _testSuiteBuilder.GetTestCase(caseNumber);

    /// <summary>
    /// Access to abstract base method for retrieving a specific test step that belongs to a specific test case.
    /// </summary>
    /// <param name="caseNumber"></param>
    /// <param name="stepNumber"></param>
    /// <returns></returns>
    public TestStep GetTestStep(int caseNumber, int stepNumber) => _testSuiteBuilder.GetTestStep(caseNumber, stepNumber);

    /// <summary>
    /// Director builds the test suite by calling the implemented ImportTestData() method.
    /// </summary>
    public void BuildFromXml()
    {
      _testSuiteBuilder.Build(_testSuiteBuilder.Import(FilePath));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="testSteps"></param>
    /// <returns></returns>
    public List<IMessage> ExecuteTestSteps(List<TestStep> testSteps)
    {
      List<IMessage> responses = new List<IMessage>();
      foreach (TestStep t in testSteps)
      {
        try
        {
          IMessage response = null;
          // Check for messages before sending and asserting
          if (t.Message != null)
          {
            response = SendHL7Message(t);  // Send encoded message with MllpMessageSender
            responses.Add(response);       // Add response to list to be returned
            if (t.Assertions.Count > 0)    // Check for assertions before asserting values
              Assert(t, response);         // Process assertions for a step 
          }
          else
          {
            //NOTE: Find out about pre-conditions and what the assumption is for those
            //  add this to the test step model as a constructor available to the
            //  TestSuiteBuilder and overload with another AddTestStep method
            Console.WriteLine($"{t}: {t.Description}");
          }
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
          Debug.WriteLine(e.ToString());
          throw new HL7Exception(e.Message);
        }
      }
      return responses;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    private string ConvertLineEndings(string message)
    {
      byte[] bytes = Encoding.ASCII.GetBytes(message);
      string hex = BitConverter.ToString(bytes).Replace("-", "");
      StringBuilder ASCIIHexString = new StringBuilder();
      for (int i = 0; i < hex.Length; i++)
      {
        if (i > 0)
        {
          //Check for a LF (line feed)
          if (hex[i - 1] == '0' && hex[i] == 'A')
          {
            //Check for a CR (carriage return)
            //handle the case with LF on a line by itself (blank line)
            if (hex[i - 3] != '0' && hex[i - 2] != 'D')
            {
              ASCIIHexString.Append("D0");
              ASCIIHexString.Append(hex[i]);
            }
          }
          else
            ASCIIHexString.Append(hex[i]);
        }
        else
          ASCIIHexString.Append(hex[i]);
      }
      return ConvertHex(ASCIIHexString.ToString());
    }

    /// <summary>
    /// Method used to convert a string of hexadecimal values (2 characters each).
    /// This is used in this program to ensure that CRLF line endings appear in all messages being parsed.
    /// HL7 messages always expect CR to separate segments and having LF line endings will throw an HL7Exception.
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    private string ConvertHex(string hexString)
    {
      try
      {
        string ascii = string.Empty;

        for (int i = 0; i < hexString.Length; i += 2)
        {
          string hs = string.Empty;

          hs = hexString.Substring(i, 2);
          uint decval = Convert.ToUInt32(hs, 16);
          char character = Convert.ToChar(decval);
          ascii += character;

        }
        return ascii;
      }
      catch (Exception ex) { Console.WriteLine(ex.Message); }

      return string.Empty;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private IMessage SendHL7Message(TestStep t)
    {
      PipeParser parser = new PipeParser();
      string crlfString = ConvertLineEndings(t.Message);  // Converting LF line endings to CRLF line endings

      // Use MllPMessageSender class to get back the response after sending a message
      MllpMessageSender sender = new MllpMessageSender(new Uri(URI));
      IMessage response = sender.SendAndReceive(crlfString);
      Console.WriteLine($"{t} description: {t.Description}\n");
      Console.WriteLine(crlfString);
      Console.WriteLine($"\nSending and receiving {t} MLLP message at {URI} ...");
      Console.WriteLine("\nResponse:");
      Console.WriteLine(parser.Encode(response));
      return response;
    }

    private void Assert(TestStep testStep, IMessage response)
    {
      Terser terser = new Terser(response);
      bool testFail = false;
      foreach (Assertion a in testStep.Assertions)
      {
        string found = terser.Get(a.TerserString);
        a.Outcome = found == a.Value;
        string status = (bool)a.Outcome ? "PASSED" : "FAILED";

        // Check if current assertion is an alternate for some terser string
        if (a.Alternate)
        {
          if (!(bool)a.Outcome) // Assertion outcome is false (failed) or null
          {
            // Check if any other alternates for the same terser string did not pass or have null outcome (not tested yet)
            if (!testStep.Assertions.Exists(o => o.TerserString == a.TerserString && (o.Outcome == true || o.Outcome == null)))
              testFail = true;
            else
              testFail = false;
          }
        }
        else
          testFail = (bool)a.Outcome ? testFail : true;

        // Output a positive outcome as green and negative as red
        if ((bool)a.Outcome)
          Console.ForegroundColor = ConsoleColor.Green;
        else
          Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine($"------------\n{a}: {status}");
        Console.WriteLine($"FOUND: '{terser.Get(a.TerserString)}'\n");
      }
      outputTestResult(testStep, testFail);
      Console.ForegroundColor = ConsoleColor.Yellow;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="testStep"></param>
    /// <param name="testFail"></param>
    void outputTestResult(TestStep testStep, bool testFail)
    {
      string resultString = testFail ? "FAILED" : "PASSED";
      if (testFail)
        Console.ForegroundColor = ConsoleColor.Red;
      else
        Console.ForegroundColor = ConsoleColor.Green;

      Console.WriteLine("____________________________________");
      Console.WriteLine($"\n\t{resultString} {testStep}");
      Console.WriteLine("____________________________________\n");
    }
  }
}
