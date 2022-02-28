using System.Xml.Serialization;

namespace HL7TestingTool
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class TestCase
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public int CaseNumber { get; set; }
    }
}