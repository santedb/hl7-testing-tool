using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// Represents an alternate value for an assertion.
    /// </summary>
    public class Alternate
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
