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

namespace HL7TestingTool
{
  class Program
  {
    static void Main(string[] args)
    {
      /// --------------------------------------------------------------  MESSAGES
      string TEST_CR_02_10 = @"MSH|^~\&|TEST_HARNESS^^|TEST^^|CR1|MOH_CAAT|20141104174451|TEST_HARNESS+TEST_HARNESS|ADT^A01^ADT_A01|TEST-CR-02-10|P|2.3.1
EVN||20101020
PID|||RJ-438^^^TEST||JOHNSTON^ROBERT^^^^^L|MURRAY^^^^^^L|19830205|M|||1220 Centennial Farm Road^^ELLIOTT^IA^51532||^PRN^PH^^^712^7670867
PV1||I";

      string TEST_CR_02_20 = @"MSH|^~\&|TEST_HARNESS|TEST|CR1|MOH_CAAT|20090223144546||QBP^Q23^QBP_Q21|TEST-CR-02-20|P|2.5
QPD|IHE PIX Query|Q0220|RJ-438^^^&2.16.840.1.113883.3.72.5.9.1&ISO^PI
RCP|I";

      string TEST_CR_02_30 = @"MSH|^~\&|TEST_HARNESS^^|TEST^^|SANTEMPI|DOCKER|20141104174451|TEST_HARNESS+TEST_HARNESS|ADT^A01^ADT_A01|TEST-CR-02-30|P|2.3.1
EVN||20101020
PID|||RJ-439^^^TEST&&||JONES^JENNIFER^^^^^L|SMITH^^^^^^L|19840125|F|||123 Main Street West ^^NEWARK^NJ^30293||^PRN^PH^^^409^30495||||||
PV1||I";

      string TEST_CR_03_10 = @"MSH|^~\&|TEST_HARNESS^^|TEST^^|CR1^^|MOH_CAAT^^|20141104174451|TEST_HARNESS+TEST_HARNESS|ADT^A01^ADT_A01|TEST-CR-03-10|P|2.3.1
EVN||20101020
PID|||RJ-999-2^^^&2.16.840.1.113883.3.72.5.9.1&ISO||THAMES^ROBERT^^^^^L| |1983|M|||1220 Centennial Farm Road^^ELLIOTT^IA^51532||^PRN^PH^^^712^7670867
PV1||I";

      /// ================================================== HOW TO USE A TERSER
      // Search - Construct a v2 message
      QBP_Q21 message = new QBP_Q21();

      // Message header
      message.MSH.AcceptAcknowledgmentType.Value = "AL"; // Always send response
      message.MSH.DateTimeOfMessage.Time.Value = DateTime.Now.ToString("yyyyMMddHHmmss"); // Date/time of creation of message
      message.MSH.MessageControlID.Value = Guid.NewGuid().ToString(); // Unique id for message
      message.MSH.MessageType.MessageStructure.Value = "QBP_Q21"; // Message structure type (Query By Parameter Type 21)
      message.MSH.MessageType.MessageCode.Value = "QBP"; // Message Structure Code (Query By Parameter)
      message.MSH.MessageType.TriggerEvent.Value = "Q22"; // Trigger event (Event Query 22)
      message.MSH.ProcessingID.ProcessingID.Value = "P"; // Production
      message.MSH.ReceivingApplication.NamespaceID.Value = "SANTEMPI"; // SanteMPI
      message.MSH.ReceivingFacility.NamespaceID.Value = "DOCKER"; // Docker instance of SanteDB with SanteMPI
      message.MSH.SendingApplication.NamespaceID.Value = "TEST_HARNESS"; // Application must be created beforehand as a test harness
      message.MSH.SendingFacility.NamespaceID.Value = "TEST"; //Facility name that was specified during creation of TEST_HARNESS application
      message.MSH.Security.Value = "SECRET+TEST_HARNESS"; // Authentication secret coming from OAuth

      // Message query
      message.QPD.MessageQueryName.Identifier.Value = Guid.NewGuid().ToString(); // Unique query name
                                                                                 // Sometimes it is easier to use a terser
      Terser terser = new Terser(message);
      terser.Set("/.QPD-3(0)-1", "@PID.5.1");
      terser.Set("/.QPD-3(0)-2", "familyName");
      terser.Set("/.QPD-3(1)-1", "@PID.5.2");
      terser.Set("/.QPD-3(1)-2", "givenName");

      /// --------------------------------------------------------------- LOOPING THROUGH TESTS 
      // Test steps as strings for now.
      List<TestStep> testSteps = new List<TestStep> {
        new TestStep { CaseNumber = 02, StepNumber = 10, Message = TEST_CR_02_10},
        new TestStep { CaseNumber = 02, StepNumber = 20, Message = TEST_CR_02_20},
        new TestStep { CaseNumber = 02, StepNumber = 30, Message = TEST_CR_02_30}
      };

      // Count the number of test cases.
      int caseCount = 1;
      while (true)
      {
        if (testSteps.Exists(s => s.CaseNumber == caseCount))
          caseCount++;
        else
          break;
      }

      // Now loop through each of the test cases 
      for (int i = 0; i < caseCount; i++)
      {
        // Find number of test steps for this test case
        int stepCount = testSteps.Where(s => s.CaseNumber == i).ToList().Count;

        // Loop through test steps
        for (int j = 0; j < stepCount; j++)
        {
          //TODO: Modify the code for parsing and sending to generically handle all messages.
        }
      }

      // ----------------------------------------------------------------------- TESTING NEW DESIGN
      TestSuiteBuilderDirector director = new TestSuiteBuilderDirector(new TestSuiteBuilder(), @"D:\MEDIC\HL7TestingTool\HL7TestingTool\data\");
      List<TestStep> allSteps = director.GetResult();
      foreach (TestStep t in testSteps)
      {
        try
        {
          PipeParser Parser = new PipeParser();
          var adta01 = Parser.Parse(t.Message);
          MllpMessageSender sender = new MllpMessageSender(new Uri("llp://127.0.0.1:2100"));
          Console.WriteLine("Sending and receiving MLLP message at llp:127.0.0.1:2100 ...");

          Console.WriteLine($"Message for TEST-CR-{t.CaseNumber}-{t.StepNumber}");
          Console.WriteLine(t.Message);
          Console.WriteLine();
          Console.WriteLine($"Response for message: TEST-CR-{t.CaseNumber}-{t.StepNumber}");
          Console.WriteLine(Parser.Encode(sender.SendAndReceive(adta01)));
          Console.WriteLine("-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }
      Console.ReadKey();

        /// -------------------------------------------------------------------- PARSE AND SEND
        //PipeParser Parser = new PipeParser();
        //IMessage m = Parser.Parse(msg);
        //NHapi.Model.V231.Message.ADT_A01 adta01 = m as NHapi.Model.V231.Message.ADT_A01;

      //  // Use var here to ensure that the correct type (representing message structure) is used at runtime
      //  var adta01_CR_02_10 = Parser.Parse(TEST_CR_02_10);
      //Type a = adta01_CR_02_10.GetType();
      //Console.WriteLine("type: " + a);
      //var qpbq21_CR02_20 = Parser.Parse(TEST_CR_02_20);
      //a = qpbq21_CR02_20.GetType();
      //Console.WriteLine("type: " + a);

      //var adta01_CR_02_30 = Parser.Parse(TEST_CR_02_30);
      //var adta01_CR_03_10 = Parser.Parse(TEST_CR_03_10);

      //// Send ADT_A01 message and process returned message
      //MllpMessageSender sender = new MllpMessageSender(new Uri("llp://127.0.0.1:2100"));
      //Console.WriteLine("Sending and receiving MLLP message at llp:127.0.0.1:2100 ...");
      //while (true)
      //{
      //  Console.WriteLine("Response CR-02-10:");
      //  Console.WriteLine(Parser.Encode(sender.SendAndReceive(adta01_CR_02_10)));

      //  Console.WriteLine("\nResponse CR-02-20:");
      //  Console.WriteLine(Parser.Encode(sender.SendAndReceive(qpbq21_CR02_20)));

      //  //Console.WriteLine("\nResponse CR-02-30:");
      //  //Console.WriteLine(Parser.Encode(sender.SendAndReceive(adta01_CR_01_30)));

      //  Console.WriteLine("\nResponse CR-03-10:");
      //  Console.WriteLine(Parser.Encode(sender.SendAndReceive(adta01_CR_03_10)));

      //  Console.WriteLine("\nResponse CR-03-10:");
      //  Console.WriteLine(Parser.Encode(message));

      //  Console.ReadKey();  // Press a key in the console to send messages again.
      //}
    }
  }
}
