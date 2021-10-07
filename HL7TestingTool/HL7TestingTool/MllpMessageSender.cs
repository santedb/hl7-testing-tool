using System;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace HL7TestingTool
{
  /// <summary>
  /// MLLP Message Sender
  /// </summary>
  public class MllpMessageSender
  {
    const int BUFFER_SIZE = 1024;
    private Uri m_endpoint = null; // Message endpoint for constructor

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
      byte[] buffer = new byte[BUFFER_SIZE];
      while (!buffer.Contains((byte)0x1c)) // Read into 1024 byte buffer until buffer contains FS character
      {
        int br = stream.Read(buffer, 0, BUFFER_SIZE);

        int ofs = 0;
        if (buffer[ofs] == '\v')
        {
          ofs = 1;
          br--;
        }
        response.Append(Encoding.ASCII.GetString(buffer, ofs, br));
      }
      
      // No response when missing message header
      if(response.ToString().Split('|')[0] != "MSH")
        throw new Exception($"No message header returned from {m_endpoint} ... (MLLP response body could be missing)");

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
    /// Use a TcpClient to write to a stream, then read the response as a string.
    /// </summary>
    /// <param name="parser"></param>
    /// <param name="message"></param>
    /// <returns>Response string</returns>
    public string SendAndReceive(string message)
    {
      string response;                                                      // Response to be returned
      using (TcpClient client = new TcpClient(AddressFamily.InterNetwork))  // Open a TCP port
      {
        try
        {
          client.Connect(this.m_endpoint.Host, this.m_endpoint.Port);       // Connect on the socket
          using (NetworkStream stream = client.GetStream())                 // Get the stream
          {
            WriteToStream(stream, message);                                 // Write to stream
            response = ReadResponse(stream);                                // Read response
          }
        }
        catch (Exception e)
        {
          Console.ForegroundColor = ConsoleColor.Red;
          Console.WriteLine(e.Message);
          response = e.Message;
        }
      }
      return response;
    }
  }
}
