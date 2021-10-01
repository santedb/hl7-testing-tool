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
        static void Main(string[] args)
        {
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

            /// -------------------------------------------------------------------- PARSE AND SEND
            //PipeParser Parser = new PipeParser();
            //IMessage m = Parser.Parse(msg);

            // ----------------------------------------------------------------------- TESTING NEW DESIGN
            const string DATA = "data";
            string dataPath = Path.GetFullPath($"{DATA}");
            TestSuiteBuilderDirector director = new TestSuiteBuilderDirector(new TestSuiteBuilder(), dataPath);
            director.BuildFromXml();
            List<TestStep> allSteps = director.GetTestSuite();
            foreach (TestStep t in allSteps)
            {
                try
                {
                    PipeParser parser = new PipeParser();
                    //TESTING WHAT BYTES ARE PRESENT
                    byte[] bytes = Encoding.ASCII.GetBytes(t.Message);
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
                    string crlfString = ConvertHex(ASCIIHexString.ToString());
                    IMessage encodedMessage = parser.Parse(crlfString);
                    MllpMessageSender sender = new MllpMessageSender(new Uri("llp://127.0.0.1:2100"));
                    Console.WriteLine("Sending and receiving MLLP message at llp:127.0.0.1:2100 ...");
                    Console.WriteLine($"Message for TEST-CR-{t.CaseNumber}-{t.StepNumber}");
                    Console.WriteLine(crlfString);
                    Console.WriteLine($"Response for message: TEST-CR-{t.CaseNumber}-{t.StepNumber}");
                    Console.WriteLine(parser.Encode(sender.SendAndReceive(encodedMessage)));
                    Console.WriteLine("--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------");
                    }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            Console.ReadKey();
        }
    }
}
