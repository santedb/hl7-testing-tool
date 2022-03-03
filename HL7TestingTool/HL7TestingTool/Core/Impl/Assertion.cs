using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
{
    /// <summary>
    /// 
    /// </summary>

    public class Assertion
    {
        /// <summary>
        /// 
        /// </summary>
        [XmlElement("alternate")]
        //[XmlArray("alternates")]
        //[XmlArrayItem("alternate", typeof(Assertion))]
        public List<Alternate> Alternates { get; set; }

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
            return this.Alternates.Any() ? $"Expected: ['{this.Value}, {string.Join(", ",this.Alternates.Select(c=> c.Value))}'] at '{this.TerserString}'"
                : this.Missing ? $"Assert missing value at '{this.TerserString}'"
                : $"Expected: '{this.Value}' at '{this.TerserString}'";
        }
    }
}