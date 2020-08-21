using System.ComponentModel;
using System.Xml.Serialization;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class TwilioDial
    {
        [XmlAttribute("action")]
        public string Action { get; set; }

        [XmlAttribute("timeLimit")]
        public string TimeLimit { get; set; }

        [XmlAttribute("timeout")]
        public string Timeout { get; set; }

        [XmlAttribute("record")]
        public string Record { get; set; }

        [XmlAttribute("recordingStatusCallback")]
        public string RecordingStatusCallback { get; set; }

        [XmlText]
        public string Text { get; set; }

        [XmlElement("Number"), DefaultValue(null)]
        public TwilioNumber Number { get; set; }
    }
}
