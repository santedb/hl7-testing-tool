using System.Xml.Serialization;

namespace HL7TestingTool
{
    /// <summary>
    /// 
    /// </summary>

    public class Assertion
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("alternate")]
        public string Alternate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("missing")]
        public bool Missing { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlIgnore]
        public bool? Outcome { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("terserString")]
        public string TerserString { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [XmlAttribute("value")]
        public string Value { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Alternate != null ? $"Assert alternate value '{this.Value}' at '{this.TerserString}' has outcome of '{this.Outcome}'"
                : this.Missing ? $"Assert missing value at '{this.TerserString}' has outcome of '{this.Outcome}'"
                : $"Assert mandatory value '{this.Value}' at '{this.TerserString}' has outcome of '{this.Outcome}'";
        }
    }
}