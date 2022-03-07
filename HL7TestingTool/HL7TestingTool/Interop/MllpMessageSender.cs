using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace HL7TestingTool.Interop
{
    /// <summary>
    /// Represents an MLLP message sender.
    /// </summary>
    public class MllpMessageSender : IMllpMessageSender
    {
        /// <summary>
        /// The default buffer size.
        /// </summary>
        private const int BufferSize = 1024;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger<MllpMessageSender> logger;

        /// <summary>
        /// The configuration.
        /// </summary>
        private readonly IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MllpMessageSender"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public MllpMessageSender(ILogger<MllpMessageSender> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        /// <summary>
        /// Read the response from a NetworkStream and return a string representation of it
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string ReadResponse(NetworkStream stream)
        {
            var response = new StringBuilder();
            var buffer = new byte[BufferSize];
            while (!buffer.Contains((byte) 0x1c)) // Read into 1024 byte buffer until buffer contains FS character
            {
                var byteCount = stream.Read(buffer, 0, BufferSize);
                var offset = 0;
                if (buffer[offset] == '\v') // Adjust start and count of bytes read when starting with '|' and skip it
                {
                    offset = 1;
                    byteCount--;
                }

                var buffString = Encoding.ASCII.GetString(buffer, offset, byteCount);
                response.Append(buffString);
            }

            // No response when missing message header
            //if (response.ToString().Split('|')[0] != "MSH")
            //{
            //    throw new Exception($"No message header returned from {this.configuration.GetValue<Uri>("Endpoint")} ... (MLLP response body could be missing)");
            //}

            return response.ToString();
        }

        /// <summary>
        /// Read the response from a NetworkStream and return a string representation of it
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private string ReadResponse(SslStream stream)
        {
            var response = new StringBuilder();
            var buffer = new byte[BufferSize];
            while (!buffer.Contains((byte) 0x1c)) // Read into 1024 byte buffer until buffer contains FS character
            {
                var byteCount = stream.Read(buffer, 0, BufferSize);
                var offset = 0;
                if (buffer[offset] == '\v') // Adjust start and count of bytes read when starting with '|' and skip it
                {
                    offset = 1;
                    byteCount--;
                }

                var buffString = Encoding.ASCII.GetString(buffer, offset, byteCount);
                response.Append(buffString);
            }

            // No response when missing message header
            //if (response.ToString().Split('|')[0] != "MSH")
            //{
            //    throw new Exception($"No message header returned from {this.configuration.GetValue<Uri>("Endpoint")} ... (MLLP response body could be missing)");
            //}

            return response.ToString();
        }


        /// <summary>
        /// Validation for certificates
        /// </summary>
        private bool RemoteCertificateValidation(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // TODO: Validate chain
            return sslPolicyErrors == SslPolicyErrors.None;
        }


        /// <summary>
        /// Sends and receives a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Returns the response message.</returns>
        public string SendAndReceive(string message)
        {
            string response = null;
            using var client = new TcpClient(AddressFamily.InterNetwork);
            try
            {
                var endpoint = this.configuration.GetValue<Uri>("Endpoint");

                client.Connect(endpoint.Host, endpoint.Port);

                using var memoryStream = new MemoryStream();
                using var streamWriter = new StreamWriter(memoryStream);

                // VT
                memoryStream.Write(new byte[] { 0x0b }, 0, 1);

                streamWriter.Write(message);
                streamWriter.Flush();

                // FS CR
                memoryStream.Write(new byte[] { 0x1c, 0x0d }, 0, 2);

                switch (endpoint.Scheme)
                {
                    case "mllp":
                    case "llp":
                    {
                        using var networkStream = client.GetStream();
                        networkStream.Write(memoryStream.ToArray(), 0, (int)memoryStream.Position);
                        networkStream.Flush();
                        response = this.ReadResponse(networkStream);
                        break;
                    }
                    case "sllp":
                    {
                        using var sslStream = new SslStream(client.GetStream(), false, 
                            new RemoteCertificateValidationCallback(this.RemoteCertificateValidation));
                            //sslStream.AuthenticateAsClient("SSN");
                        var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        store.Open(OpenFlags.ReadOnly);
                        var clientCertificate = this.configuration.GetValue<string>("ClientCertificateThumbprint");
                        if (clientCertificate == null)
                        {
                            this.logger.LogError("Invalid property: ClientCertificateThumbprint cannot be null when using SLLP.");
                            throw new InvalidOperationException();
                        }
                        var clientCertificates = store.Certificates.Find(X509FindType.FindByThumbprint, clientCertificate, true);
                        if (clientCertificates.Count < 1 || clientCertificates.Count > 1)
                        {
                            throw new ArgumentNullException();
                        }

                        sslStream.AuthenticateAsClient(endpoint.Host, clientCertificates, true);
                        sslStream.Write(memoryStream.ToArray(), 0, (int)memoryStream.Position);
                        sslStream.Flush();
                        response = this.ReadResponse(sslStream);
                        break;
                    }
                    default:
                        this.logger.LogError("Endpoint protocol not supported.");
                        throw new InvalidOperationException("Protocol not supported.");
                }
                

                
            }
            catch (Exception e)
            {
                this.logger.LogError($"Error processing HL7 response: {e}");
            }

            return response;
        }
    }
}