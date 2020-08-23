using System.ComponentModel;
using System.Xml.Serialization;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class TwilioPause
    {
        [XmlAttribute("length")]
        public int Length { get; set; }
    }
}
