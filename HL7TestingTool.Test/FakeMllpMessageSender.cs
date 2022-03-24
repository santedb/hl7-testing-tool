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
 * Date: 2022-03-24
 */

using System;
using System.Diagnostics.CodeAnalysis;
using HL7TestingTool.Interop;

namespace HL7TestingTool.Test
{
    /// <summary>
    /// Represents a fake TCP listener for unit testing purposes.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class FakeMllpMessageSender : IMllpMessageSender
    {
        /// <summary>
        /// Sends and receives a message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>Returns the response message.</returns>
        public string SendAndReceive(string message)
        {
            return $"MSH|^~\\&|_X_|_X_|TEST_HARNESS|TEST|20220308142703||ACK^A01^ACK|{Guid.NewGuid()}||2.3.1\rMSA|CE|TEST-CR-13-10|Data not found|||204^Error processing assigning authority\r";
        }
    }
}
