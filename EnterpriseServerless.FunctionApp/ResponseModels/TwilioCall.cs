using System;

namespace EnterpriseServerless.FunctionApp.ResponseModels
{
    public class TwilioCall
    {
        public string CallSid { get; set; }
        public string DialCallSid { get; set; }
        public string To { get; set; }
        public string From { get; set; }
        public string RecordingSid { get; set; }
        public string RecordingUrl { get; set; }
        public int RecordingDuration { get; set; }
        public string DialCallStatus { get; set; }
        public string CallStatus { get; set; }
        public int CallDuration { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string RouteKey { get; set; }
        public bool IsRecordingStatusCallback { get; set; }
    }
}
