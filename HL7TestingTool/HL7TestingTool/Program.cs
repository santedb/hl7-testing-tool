using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHapi.Model.V25.Message;
using NHapi.Model.V231.Message;
using NHapi.Model.V231.Datatype;
using NHapi.Base.Util;
using NHapi.Base.Parser;
using NHapi.Base.Model;
using System.Text.RegularExpressions;
using System.Collections;
using NHapi.Base;
using System.Configuration;
using NHapi.Base.Model.Configuration;
using System.IO;

namespace HL7TestingTool
{
  class Program
  {
    const string URI = "llp://127.0.0.1:2100";
    const string DATA = "data";

    /// <summary>
    /// Method used to convert a string of hexadecimal values (2 characters each).
    /// This is used in this program to ensure that CRLF line endings appear in all messages being parsed.
    /// HL7 messages always expect CR to separate segments and having LF line endings will throw an HL7Exception.
    /// </summary>
    /// <param name="hexString"></param>
    /// <returns></returns>
    public static string ConvertHex(String hexString)
    {
      try
      {
        string ascii = string.Empty;

        for (int i = 0; i < hexString.Length; i += 2)
        {
          String hs = string.Empty;

          hs = hexString.Substring(i, 2);
          uint decval = System.Convert.ToUInt32(hs, 16);
          char character = System.Convert.ToChar(decval);
          ascii += character;

        }

        return ascii;
      }
      catch (Exception ex) { Console.WriteLine(ex.Message); }

      return string.Empty;
    }

    static string ConvertLineEndings(string message)
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
            //TODO: handle the case with LF on a line by itself (blank line)
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

    static IMessage SendHL7Message(TestStep t)
    {
      PipeParser parser = new PipeParser();
      string crlfString = ConvertLineEndings(t.Message);  // Converting LF line endings to CRLF line endings
      IMessage encodedMessage = parser.Parse(crlfString); // Encoded as an HL7 message from a string

      // Use MllPMessageSender class to get back the response after sending a message
      MllpMessageSender sender = new MllpMessageSender(new Uri(URI));
      IMessage response = sender.SendAndReceive(encodedMessage);
      Console.WriteLine($"Sending and receiving MLLP message at {URI} ...");
      Console.WriteLine($"Message for TEST-CR-{t.CaseNumber}-{t.StepNumber}");
      Console.WriteLine(crlfString);
      Console.WriteLine($"\nResponse for message: TEST-CR-{t.CaseNumber}-{t.StepNumber}");
      Console.WriteLine(parser.Encode(response));
      Console.WriteLine("==============================================================\n\n");
      return response;
    }

    static List<IMessage> ExecuteTestSteps(List<TestStep> testSteps)
    {
      List<IMessage> responses = new List<IMessage>();
      foreach (TestStep t in testSteps)
      {
        try
        {
          IMessage response = SendHL7Message(t);
          responses.Add(response); // Send encoded message with MLLP
          //TOOD: Add a method here that processes assertions for each step.
          Assert(t, response);

        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }
      return responses;
    }

    static void Assert(TestStep testStep, IMessage response)
    {
      Terser terser = new Terser(response);
      foreach(Assertion a in testStep.Assertions)
      {
        if (terser.Get(a.TerserString) == a.Value)
          Console.WriteLine($"------------\nCheck for '{a.Value}' at '{a.TerserString}': PASSED");
        else
          Console.WriteLine($"------------\nCheck for '{a.Value}' at '{a.TerserString}': FAILED");

        Console.WriteLine($"FOUND: {terser.Get(a.TerserString)}\n------------");
      }
    }


    static void Main(string[] args)
    {
      string dataPath = Path.GetFullPath($"{DATA}");
      TestSuiteBuilderDirector director = new TestSuiteBuilderDirector(new TestSuiteBuilder(), dataPath);
      director.BuildFromXml();

      // Call on helper to execute all test steps in test suite
      //ExecuteTestSteps(director.GetTestSuite());
      
      // Call on helper to execute all test steps only from case 2
      //ExecuteTestSteps(director.GetTestCase(2));

      // Call on helper to execute for test case 3, test step 20 (note that this must be a list, but the director's method only returns a TestStep)
      List<IMessage> responses = ExecuteTestSteps(new List<TestStep> { director.GetTestStep(3, 10) });

      Console.ReadKey();
    }
  }
}
