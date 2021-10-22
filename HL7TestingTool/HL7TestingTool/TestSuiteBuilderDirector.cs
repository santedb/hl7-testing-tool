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
    /// Director builds the test suite by calling the implemented Import() method to get filepaths as a string array for the Build() method.
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
        Console.ForegroundColor = ConsoleColor.Yellow;
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
        catch (HL7Exception e) // Can catch an Exception with missing segment 
        {
          if (e.SegmentName == null) //CAUGHT MISSING SEGMENT CONDITION
            continue;
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine($"Exception thrown: {e.Message}\n");
          //Console.WriteLine("____________________________________");
          //Console.WriteLine($"\n\t FAILED: {t}");
          //Console.WriteLine("____________________________________\n");
          OutputTestResult(t, true);
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
      for (int i = 1; i < hex.Length; i += 2)
      {
        //Check for a LF (line feed)
        if (hex[i - 1] == '0' && hex[i] == 'A') // Check for 0A at current index while incrementing by 2
        {
          //Check for a CR (carriage return)
          //handle the case with LF on a line by itself (blank line)
          if (hex[i - 2] == 'D')    // Checking: _0A   Current ASCIIHexString = ...0 
          {
            if (hex[i - 3] != '0')   // Checking: _D0A   Current ASCIIHexString = ...0
            {
              ASCIIHexString.Append("0D");
              ASCIIHexString.Append(hex[i - 1]);
              ASCIIHexString.Append(hex[i]);  // Needed to append D0A to ASCIIHexString = ...0 to make it as ...0D0A
            }
            else  // Current ASCIIHexString = ...0D0 (no action needed - just append A)
            {
              ASCIIHexString.Append(hex[i - 1]);
              ASCIIHexString.Append(hex[i]);
            }
          }
          else    // Current ASCIIHexString = ...**0
          {
            ASCIIHexString.Append("0D");
            ASCIIHexString.Append(hex[i - 1]);
            ASCIIHexString.Append(hex[i]);  // Needed to append D0A to ASCIIHexString = ...0 to make it as ...0D0A
          }
        }
        else
        {
          ASCIIHexString.Append(hex[i - 1]);
          ASCIIHexString.Append(hex[i]);
        }
      }

      return ConvertHex(ASCIIHexString.ToString());
    }

    /// <summary>
    /// Method used to convert a string of hexadecimal values (2 characters each).
    /// This is used in this program to ensure that CRLF line endings appear in all messages being parsed.
    /// HL7 messages always expect CR to separate segments and having LF line endings will throw an Exception.
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public string ConvertHex(string hexString)
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
      catch (Exception ex) { Console.WriteLine(ex.Message); return ex.Message; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="t"></param>
    /// <returns></returns>
    private IMessage SendHL7Message(TestStep t)
    {
      string crlfString = ConvertLineEndings(t.Message);  // Converting LF line endings to CRLF line endings

      Console.WriteLine($"{t}: {t.Description}\n");
      Console.WriteLine(crlfString);
      Console.WriteLine($"\nSending and receiving {t} as MLLP message at {URI} ...");
      Console.WriteLine("\nResponse:");

      // Use MllPMessageSender class to get back the response after sending a message
      MllpMessageSender sender = new MllpMessageSender(new Uri(URI));
      string responseString = sender.SendAndReceive(crlfString);
      IMessage response;
      try
      {
        if (responseString.Split('|')[0] == "MSH")
        {
          PipeParser parser = new PipeParser();
          response = parser.Parse(responseString);
        }
        else
        {
          HL7Exception ex = new HL7Exception("Missing MSH")
          {
            SegmentName = null
          };
          throw ex;
        }
      }
      catch (HL7Exception e) { throw e; }

      Debug.WriteLine(responseString);
      return response;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="testStep"></param>
    /// <param name="response"></param>
    private void Assert(TestStep testStep, IMessage response)
    {
      Terser terser = new Terser(response);
      string found;
      bool testFail = false;
      foreach (Assertion a in testStep.Assertions)
      {
        try  // Getting a value from a response terser
        {
          found = terser.Get(a.TerserString);
          if (a.Value == null && a.Missing == true) // SHOULD be missing
            a.Outcome = false;
          else // SHOULD NOT be missing
            a.Outcome = found == a.Value;
        }
        catch (HL7Exception ex) // and handle case where a missing segment occurs 
        {
          found = "No value";
          if (a.Missing)  // Check for a SegmentName in the HL7Exception
          {
            if (ex.SegmentName == null) // Assertion passes
              a.Outcome = true;
            else                        // Assertion fails
            {
              found = a.TerserString.ToString();
              a.Outcome = false;
            }
          }
          else            // Assertion is for required value (not missing)
          {
            if (ex.SegmentName == null) // Assertion fails
              a.Outcome = false;
            else                        // Assertion passes
            {
              found = a.TerserString.ToString();
              a.Outcome = true;
            }
          }
        }
        string status = (bool)a.Outcome ? "PASSED" : "FAILED";

        // Check if current assertion is an alternate for some terser string
        if (a.Alternate != null)  // Test step fails only if ALL alternate assertions with a matching TerserString fail
        {
          if (!(bool)a.Outcome) // Assertion outcome is false (failed) or null
          {
            // Check if any other alternates for the same terser string did not pass or have null outcome (not tested yet)
            if (!testStep.Assertions.Exists(o => o.Alternate == a.Alternate && (o.Outcome == true || o.Outcome == null)))
              testFail = true;
            else
              testFail = false;
          }
        }
        else  // Test step is either for missing or matching values and fails if any one of them fails in serial.
          testFail = (bool)a.Outcome ? testFail : true;

        // Output a positive outcome as green and negative as red
        if ((bool)a.Outcome)
          Console.ForegroundColor = ConsoleColor.Green;
        else
          Console.ForegroundColor = ConsoleColor.Red;

        Console.WriteLine($"------------\n{a}: {status}");
        Console.WriteLine($"FOUND: '{found}'\n");
      }
      OutputTestResult(testStep, testFail);
      Console.ForegroundColor = ConsoleColor.Yellow;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="testStep"></param>
    /// <param name="testFail"></param>
    void OutputTestResult(TestStep testStep, bool testFail)
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
