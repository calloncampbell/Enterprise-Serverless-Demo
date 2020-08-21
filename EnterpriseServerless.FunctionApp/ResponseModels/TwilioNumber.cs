using System.Xml.Serialization;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class TwilioNumber
    {
        [XmlAttribute("url")]
        public string Url { get; set; }

        [XmlAttribute("method")]
        public string Method { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
