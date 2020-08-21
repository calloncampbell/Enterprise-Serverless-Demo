using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseServerless.FunctionApp.RequestModels
{
    public class IncomingCall
    {
        public string CallSid { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string Language { get; set; }
    }
}
