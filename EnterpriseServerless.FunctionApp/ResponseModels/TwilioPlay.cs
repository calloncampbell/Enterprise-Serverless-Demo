using System.Xml.Serialization;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class TwilioPlay
    {
        [XmlAttribute("loop")]
        public int Loop { get; set; }

        [XmlText]
        public string Text { get; set; }
    }
}
