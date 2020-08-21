using System.ComponentModel;
using System.Xml.Serialization;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class TwilioGather
    {
        [XmlAttribute("action")]
        public string Action { get; set; }

        [XmlAttribute("numDigits")]
        public int NumDigits { get; set; }

        [XmlElement("Say"), DefaultValue(null)]
        public TwilioSay Saying { get; set; }

        [XmlElement("Play"), DefaultValue(null)]
        public TwilioPlay Play { get; set; }

        [XmlElement("Pause"), DefaultValue(null)]
        public TwilioPause Pause {get;set;}

        [XmlElement("Say"), DefaultValue(null)]
        public TwilioSay Saying2 { get; set; }

        [XmlElement("Play"), DefaultValue(null)]
        public TwilioPlay Play2 { get; set; }

        [XmlElement("Pause"), DefaultValue(null)]
        public TwilioPause Pause2 { get; set; }

        [XmlElement("Say"), DefaultValue(null)]
        public TwilioSay Saying3 { get; set; }

        [XmlElement("Play"), DefaultValue(null)]
        public TwilioPlay Play3 { get; set; }

        [XmlElement("Pause"), DefaultValue(null)]
        public TwilioPause Pause3 { get; set; }
    }
}
