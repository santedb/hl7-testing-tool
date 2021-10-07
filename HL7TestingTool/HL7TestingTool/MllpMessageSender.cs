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
    /// <param name="endpoint">The endpoint in the form llp://ipaddress:port</param>
    public MllpMessageSender(Uri endpoint)
    {
      this.m_endpoint = endpoint;
    }

    /// <summary>
    /// Read the response from a NetworkStream and return a string representation of it
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    private string ReadResponse(NetworkStream stream)
    {
      StringBuilder response = new StringBuilder();
      byte[] buffer = new byte[2048];
      while (!buffer.Contains((byte)0x1c)) // Keep reading until the buffer has FS character
      {
        int br = stream.Read(buffer, 0, 2048);

        int ofs = 0;
        if (buffer[ofs] == '\v')
        {
          ofs = 1;
          br--;
        }
        response.Append(Encoding.ASCII.GetString(buffer, ofs, br));
      }
      Console.WriteLine($"'{response}'");
      return response.ToString();
    }

    /// <summary>
    /// Write a message to a NetworkStream with beginning and ending characters according to MLLP protocol
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private NetworkStream WriteToStream(NetworkStream stream, string message)
    {
      // Start message
      stream.Write(new byte[] { 0x0b }, 0, 1);  // 0x0b = VT (vertical tab character)

      // Message body
      byte[] buffer = Encoding.ASCII.GetBytes(message);
      stream.Write(buffer, 0, buffer.Length);

      // End message
      // 0x0d = CR (carriage return) and 0x1c = FS (information separator)
      stream.Write(new byte[] { 0x1c, 0x0d }, 0, 2);
      stream.Flush(); // Ensure all bytes get sent down the wire
      return stream;
    }
    
    /// <summary>
    /// Use a TcpClient to write to a stream, read the response, and parse the response as an IMessage.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private IMessage UseTcpClient(PipeParser parser, string message)
    {
      // Open a TCP port
      using (TcpClient client = new TcpClient(AddressFamily.InterNetwork))
      {
        try
        {
          client.Connect(this.m_endpoint.Host, this.m_endpoint.Port); // Connect on the socket
          using (NetworkStream stream = client.GetStream())           // Get the stream
          {
            WriteToStream(stream, message);                           // Write to stream
            string resp = ReadResponse(stream);
            Console.WriteLine(resp);
            return parser.Parse(resp);                // Parse response
          }
        }
        catch (Exception e)
        {
          Debug.WriteLine(e.ToString());
          throw;
        }
      }
    }

    /// <summary>
    /// Send a message as a string and recieve an IMessage
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public IMessage SendAndReceive(string message)
    {
      return UseTcpClient(new PipeParser(), message);
    }

    /// <summary>
    /// Send and receive an IMessage
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public IMessage SendAndReceive(IMessage message)
    {
      var parser = new PipeParser();  // Encode the message
      string strMessage;
      try
      {
        strMessage = parser.Encode(message);
      }
      catch (Exception e)
      {
        throw new HL7Exception(e.Message);
      }

      // Helper method writes to stream and parses response
      return UseTcpClient(parser, strMessage);
    }
  }
}
