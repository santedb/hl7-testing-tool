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
    }
    public string TerserString { get; set; }
    public string Value { get; set; }

    public QBP_Q21 exampleTerser()
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

      return message;
    }

  }
}
