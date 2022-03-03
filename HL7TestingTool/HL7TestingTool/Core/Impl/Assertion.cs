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
        /// 
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
        public string TerserString { get; set; }

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
            return this.Alternates.Any() ? $"Expected: ['{this.Value}, {string.Join(", ",this.Alternates.Select(c=> c.Value))}'] at '{this.TerserString}'"
                : this.Missing ? $"Assert missing value at '{this.TerserString}'"
                : $"Expected: '{this.Value}' at '{this.TerserString}'";
        }
    }
}