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
    /// <summary>
    /// 
    /// </summary>
    private const string DATA = "data";

    /// <summary>
    /// 
    /// </summary>
    /// <returns>Option selected by user in main menu</returns>
    static int MainMenu(TestSuiteBuilderDirector director)
    {
      int option = 0;
      Console.ForegroundColor = ConsoleColor.Cyan;
      Console.WriteLine("======================================");
      Console.WriteLine("| Welcome to the HL7v2 Testing Tool! |");
      Console.WriteLine("======================================\n");

      Console.ForegroundColor = ConsoleColor.White;
      Console.WriteLine("Please enter an integer option: ");
      Console.ForegroundColor = ConsoleColor.Blue;
      Console.WriteLine("\t(1) Execute all test steps in the test suite");
      Console.WriteLine("\t(2) Execute all test steps of a specific test case");
      Console.WriteLine("\t(3) Execute a specific test step");
      Console.WriteLine("\t(4) Exit");
      try
      {
        Console.ForegroundColor = ConsoleColor.Yellow;
        option = int.Parse(Console.ReadLine());
        Console.Clear();
        switch (option)
        {
          case 1:
            //Call on helper to execute all test steps in test suite
            director.ExecuteTestSteps(director.GetTestSuite());
            break;
          case 2:
            Console.Write("Enter Case #:\t");
            int.TryParse(Console.ReadLine(), out int caseNumber);
            //Call on helper to execute all test steps only from a specific case 
            try { ValidateCaseNumber(director, caseNumber); } catch (Exception ex) { Console.WriteLine(ex.Message); break; }
            director.ExecuteTestSteps(director.GetTestCase(caseNumber));
            break;
          case 3:
            Console.Write("Enter Case #:\t");
            int.TryParse(Console.ReadLine(), out caseNumber);
            try { ValidateCaseNumber(director, caseNumber); } catch (Exception ex) { Console.WriteLine(ex.Message); break; }
            Console.Write("Enter Step #:\t");
            int.TryParse(Console.ReadLine(), out int stepNumber);
            try { ValidateStepNumber(director, caseNumber, stepNumber); } catch (Exception ex) { Console.WriteLine(ex.Message); break; }
            //Call on helper to execute a specific test step(note that this must be a list, but the director's method only returns a TestStep)
            List<TestStep> testSteps = new List<TestStep> { director.GetTestStep(caseNumber, stepNumber) };
            director.ExecuteTestSteps(testSteps);
            break;
          case 4:
            break;
          default:
            break;
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.Message);
      }
      Console.ForegroundColor = ConsoleColor.White;
      return option;
    }

    static void ValidateCaseNumber(TestSuiteBuilderDirector director, int caseNumber)
    {
      if (director.GetTestCase(caseNumber).Count == 0)
        throw new Exception("No test case with that number found.");
    }

    static void ValidateStepNumber(TestSuiteBuilderDirector director, int caseNumber, int stepNumber)
    {
      if (director.GetTestStep(caseNumber, stepNumber) == null)
        throw new Exception("No test steps with that number found.");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {
      string dataPath = Path.GetFullPath($"{DATA}");
      TestSuiteBuilderDirector director = new TestSuiteBuilderDirector(new TestSuiteBuilder(), dataPath);
      director.BuildFromXml();

      int option = 0;
      while (option != 4) // Show main menu and exit if option 4 is entered
      {
        option = MainMenu(director);
        if (option < 0 || option > 4)
          Console.WriteLine($"\n {option} is not an option! Try again...");
        else if (option != 4) 
        {
          Console.WriteLine("Press any key to return to main menu...");
          Console.ReadKey();
          Console.Clear();
        }
      }
    }
  }
}
