using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// Represents a test case
    /// </summary>
    public abstract class TestCase
    {
        /// <summary>
        /// Gets or sets the case number.
        /// </summary>
        [XmlIgnore]
        public int CaseNumber { get; set; }
    }
}