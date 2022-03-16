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
 * User: Shihab Khan, Nityan Khanna
 * Date: 2022-03-16
 */

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// Represents an assertion.
    /// </summary>
    public class Assertion
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Assertion"/> class.
        /// </summary>
        public Assertion()
        {
            this.Alternates = new List<Alternate>();
        }

        /// <summary>
        /// Gets or sets the list of alternate values for a given assertion.
        /// </summary>
        [XmlElement("alternate")]
        public List<Alternate> Alternates { get; set; }

        /// <summary>
        /// Gets or sets the value of missing.
        /// </summary>
        [XmlAttribute("missing")]
        public bool Missing { get; set; }

        /// <summary>
        /// Gets or sets the outcome.
        /// </summary>
        [XmlIgnore]
        public bool? Outcome { get; set; }

        /// <summary>
        /// Gets or sets the terser string.
        /// </summary>
        [XmlAttribute("terser")]
        public string Terser { get; set; }

        /// <summary>
        /// Gets or sets the value to assert against.
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }

        /// <summary>
        /// Returns this instance as a string representation.
        /// </summary>
        /// <returns>Returns this instance as a string representation.</returns>
        public override string ToString()
        {
            return this.Alternates.Any() ? $"Expected: ['{this.Value}, {string.Join(", ",this.Alternates.Select(c=> c.Value))}'] at '{this.Terser}'"
                : this.Missing ? $"Assert missing value at '{this.Terser}'"
                : $"Expected: '{this.Value}' at '{this.Terser}'";
        }
    }
}