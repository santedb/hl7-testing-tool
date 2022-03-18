/*
 * Copyright (C) 2021 - 2022, SanteSuite Inc. and the SanteSuite Contributors (See NOTICE.md for full copyright notices)
 * Copyright (C) 2019 - 2021, Fyfe Software Inc. and the SanteSuite Contributors
 * Portions Copyright (C) 2015-2018 Mohawk College of Applied Arts and Technology
 * 
 * Licensed under the Apache License, Version 2.0 (the "License"); you 
 * may not use this file except in compliance with the License. You may 
 * obtain a copy of the License at 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0 
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the 
 * License for the specific language governing permissions and limitations under 
 * the License.
 * 
 * User: Nityan Khanna
 * Date: 2022-03-16
 */

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
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private static string ReadResponse(Stream stream)
        {
            var response = new StringBuilder();

            var buffer = new byte[BufferSize];

            while (!buffer.Contains((byte)0x1c)) // Read into 1024 byte buffer until buffer contains FS character
            {
                var byteCount = stream.Read(buffer, 0, BufferSize);
                var offset = 0;

                if (buffer[offset] == '\v') // Adjust start and count of bytes read when starting with '|' and skip it
                {
                    offset = 1;
                    byteCount--;
                }

                response.Append(Encoding.ASCII.GetString(buffer, offset, byteCount));
            }

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

                switch (endpoint.Scheme.ToLowerInvariant())
                {
                    case "mllp":
                    case "llp":
                    {
                        using var networkStream = client.GetStream();
                        networkStream.Write(memoryStream.ToArray(), 0, (int)memoryStream.Position);
                        networkStream.Flush();
                        response = ReadResponse(networkStream);
                        break;
                    }
                    case "sllp":
                    {
                        using var sslStream = new SslStream(client.GetStream(), false, this.RemoteCertificateValidation);
                        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);

                        store.Open(OpenFlags.ReadOnly);

                        var clientCertificate = this.configuration.GetValue<string>("ClientCertificateThumbprint");

                        if (clientCertificate == null)
                        {
                            this.logger.LogError("ClientCertificateThumbprint cannot be null when using SLLP");
                            throw new InvalidOperationException("ClientCertificateThumbprint cannot be null when using SLLP");
                        }

                        var clientCertificates = store.Certificates.Find(X509FindType.FindByThumbprint, clientCertificate, true);

                        if (clientCertificates.Count > 1 || clientCertificates.Count < 1)
                        {
                            throw new ArgumentNullException();
                        }

                        sslStream.AuthenticateAsClient(endpoint.Host, clientCertificates, true);
                        sslStream.Write(memoryStream.ToArray(), 0, (int)memoryStream.Position);
                        sslStream.Flush();

                        response = ReadResponse(sslStream);
                        break;
                    }
                    default:
                        this.logger.LogError("Endpoint protocol not supported.");
                        throw new InvalidOperationException($"Protocol not supported: {endpoint.Scheme}");
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