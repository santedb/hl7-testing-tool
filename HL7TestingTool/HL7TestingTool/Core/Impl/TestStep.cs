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
        /// 
        /// </summary>
        [XmlArray("assertions")]
        [XmlArrayItem("assert", typeof(Assertion))]
        public List<Assertion> Assertions { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("description")]
        public string Description { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlElement("message")]
        public string Message { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public int? StepNumber { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"OHIE-CR-{this.CaseNumber}-{this.StepNumber}";
        }
    }
}