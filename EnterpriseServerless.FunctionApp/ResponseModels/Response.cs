using System.ComponentModel;
using System.Xml.Serialization;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class Response
    {
        [XmlElement("Gather"), DefaultValue(null)]
        public TwilioGather Gather { get; set; }

        [XmlElement("Say"), DefaultValue(null)]
        public TwilioSay Saying { get; set; }

        [XmlElement("Play"), DefaultValue(null)]
        public TwilioPlay Play { get; set; }

        [XmlElement("Dial"), DefaultValue(null)]
        public TwilioDial Dialing { get; set; }

        [XmlElement("Hangup"), DefaultValue(null)]
        public TwilioHangup Hangup { get; set; }

        [XmlElement("Reject"), DefaultValue(null)]
        public TwilioReject Reject { get; set; }
    }
}
