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