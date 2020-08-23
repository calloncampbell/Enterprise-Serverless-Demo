using System.Xml.Serialization;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class TwilioSay
    {
        [XmlAttribute("loop")]
        public int Loop { get; set; }

        [XmlAttribute("voice")]
        public string Voice { get; set; }

        [XmlAttribute("language")]
        public string Language { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
