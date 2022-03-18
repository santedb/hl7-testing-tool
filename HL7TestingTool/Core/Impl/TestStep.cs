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
 * User: Tommy Zieba, Azabelle Tale, Shihab Khan, Nityan Khanna
 * Date: 2022-03-16
 */

using System.Collections.Generic;
using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// Represents a step for a test case that has a list of ExpectedResults.
    /// </summary>
    [XmlRoot("testStep")]
    public class TestStep : TestCase
    {
     
        /// <summary>
        /// Gets or sets the list of assertions.
        /// </summary>
        [XmlArray("assertions")]
        [XmlArrayItem("assert", typeof(Assertion))]
        public List<Assertion> Assertions { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        [XmlElement("message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the test step number.
        /// </summary>
        [XmlIgnore]
        public int? StepNumber { get; set; }

        /// <summary>
        /// Returns this instance as a string representation.
        /// </summary>
        /// <returns>Returns this instance as a string representation.</returns>
        public override string ToString()
        {
            return $"OHIE-CR-{this.CaseNumber}-{this.StepNumber}";
        }
    }
}