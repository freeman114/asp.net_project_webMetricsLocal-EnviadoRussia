using System;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace webMetrics
{
    [XmlRoot(ElementName = "error")]
    public class Error
    {
        [XmlElement(ElementName = "code")]
        public string Code { get; set; }
        [XmlElement(ElementName = "message")]
        public string Message { get; set; }
    }

    [XmlRoot(ElementName = "errors")]
    public class Errors
    {
        [XmlElement(ElementName = "error")]
        public Error Error { get; set; }
    }

}
