using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
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