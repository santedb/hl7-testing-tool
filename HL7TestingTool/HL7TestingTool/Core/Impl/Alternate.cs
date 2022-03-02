using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace HL7TestingTool.Core.Impl
{
    public class Alternate
    {
        [XmlAttribute("value")]
        public string Value { get; set; }
    }
}
