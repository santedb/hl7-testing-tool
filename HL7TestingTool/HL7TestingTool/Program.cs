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
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

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

    static IMessage SendHL7Message(TestStep t)
    {
      PipeParser parser = new PipeParser();
      string crlfString = ConvertLineEndings(t.Message);  // Converting LF line endings to CRLF line endings

      // Use MllPMessageSender class to get back the response after sending a message
      MllpMessageSender sender = new MllpMessageSender(new Uri(URI));
      IMessage response = sender.SendAndReceive(crlfString);
      Console.WriteLine($"Sending and receiving {t} MLLP message at {URI} ...\n");
      Console.WriteLine(t.Description);
       Console.WriteLine();
      Console.WriteLine(crlfString);
      Console.WriteLine("\nResponse:");
      Console.WriteLine(parser.Encode(response));
      Console.WriteLine("==============================================================\n");
      return response;
    }

    static List<IMessage> ExecuteTestSteps(List<TestStep> testSteps)
    {
      List<IMessage> responses = new List<IMessage>();
      foreach (TestStep t in testSteps)
      {
        try
        {
          IMessage response = SendHL7Message(t);  // Send encoded message with MllpMessageSender
          responses.Add(response);                // Add response to list to be returned
          Assert(t, response);                    // Process assertions for a step 

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

    static void Assert(TestStep testStep, IMessage response)
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
          if (!(bool)a.Outcome) // Assertion outcome is false (failed)
          {
            // Check if any other alternates for the same terser string did not pass or have null outcome (not tested yet)
            if (!testStep.Assertions.Exists(o => o.TerserString == a.TerserString && (o.Outcome == true || o.Outcome == null)))
              testFail = true;
            else
              testFail = false;
          }
        }
        Console.WriteLine($"------------\n{a}: {status}");
        Console.WriteLine($"FOUND: '{terser.Get(a.TerserString)}'\n");
      }
      Console.WriteLine("____________________________________");
      if (testFail) Console.WriteLine($"\n\tFAILED {testStep}");
      else Console.WriteLine($"\n\tPASSED {testStep}");
      Console.WriteLine("____________________________________\n");
    }


    static void Main(string[] args)
    {

            MainMenu();
            Console.ReadKey();
    }
    static void MainMenu()
    {
        string dataPath = Path.GetFullPath($"{DATA}");
        TestSuiteBuilderDirector director = new TestSuiteBuilderDirector(new TestSuiteBuilder(), dataPath);
        director.BuildFromXml();
        
        Console.WriteLine("Welcome to HL7Testing Tool!");
        Console.WriteLine("Please enter \n" + "1 (to execute all test steps in the test suite)\n" + "2 (to execute all test steps of a specific test case)\n" + "3 (to execute a specific test step)\n" + "4 (to exit)"
            );


    string option = Console.ReadLine();
    switch (option)
    {
        case "1":
            //Call on helper to execute all test steps in test suite
            ExecuteTestSteps(director.GetTestSuite());
            break;
        case "2":
            {
                Console.WriteLine("Please enter the Test Case Number: ");
                int caseNumber = int.Parse(Console.ReadLine());
                //Call on helper to execute all test steps only from a specific case 
                ExecuteTestSteps(director.GetTestCase(caseNumber));

                break;
            }
        case "3":
            {
                Console.WriteLine("Please enter the Test Case Number: ");
                int caseNumber = int.Parse(Console.ReadLine());
                Console.WriteLine("Please enter the Test Step Number: ");
                int stepNumber = int.Parse(Console.ReadLine());
                //Call on helper to execute a specific test step(note that this must be a list, but the director's method only returns a TestStep)
                ExecuteTestSteps(new List<TestStep> { director.GetTestStep(caseNumber, stepNumber) });
                break;
            }
        case "4":
            {
                        return;

            }

    }

    MainMenu();
}
      
  }
}
