using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHapi.Base.Model;
using NHapi.Base.Parser;
using System.Net.Sockets;
using System.Diagnostics;
using NHapi.Base;

namespace HL7TestingTool
{
  /// <summary>
  /// MLLP Message Sender
  /// </summary>
  public class MllpMessageSender
  {

    // Endpoint
    private Uri m_endpoint = null;

    /// <summary>
    /// Creates a new message sender
    /// </summary>
    /// <param name="endpoint">The endpoint in the form : llp://ipaddress:port</param>
    public MllpMessageSender(Uri endpoint)
    {
      this.m_endpoint = endpoint;
    }

    /// <summary>
    /// Send a message and receive the message
    /// </summary>
    public IMessage SendAndReceive(IMessage message)
    {

      // Encode the message
      var parser = new PipeParser();
      string strMessage = String.Empty;

      try
      {
        strMessage = parser.Encode(message);
      }
      catch (Exception e)
      {
        Debug.WriteLine(e.ToString());
        throw new HL7Exception(e.Message);
      }

      // Open a TCP port
      using (TcpClient client = new TcpClient(AddressFamily.InterNetwork))
      {

        try
        {
          // Connect on the socket
          client.Connect(this.m_endpoint.Host, this.m_endpoint.Port);
          // Get the stream
          using (var stream = client.GetStream())
          {
            // Write start message
            stream.Write(new byte[] { 0x0b }, 0, 1);  // 0x0b equivalent to the VT (vertical tab character)
                                                      // Write message in ASCII encoding
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(strMessage);
            stream.Write(buffer, 0, buffer.Length);
            // Write end message
            // 0x0d is the CR character (carriage return) and 0x1c is the FS character (information separator)
            stream.Write(new byte[] { 0x1c, 0x0d }, 0, 2);
            stream.Flush(); // Ensure all bytes get sent down the wire

            // Now read the response
            StringBuilder response = new StringBuilder();
            buffer = new byte[1024];
            while (!buffer.Contains((byte)0x1c)) // HACK: Keep reading until the buffer has the FS character
            {
              int br = stream.Read(buffer, 0, 1024);

              int ofs = 0;
              if (buffer[ofs] == '\v')
              {
                ofs = 1;
                br = br - 1;
              }
              response.Append(System.Text.Encoding.UTF8.GetString(buffer, ofs, br));
            }

            // Parse the response
            return parser.Parse(response.ToString());
          }

        }
        catch (Exception e)
        {
          Debug.WriteLine(e.ToString());
          throw;
        }
      }

    }


  }
}
