using System;
using static EnterpriseServerless.FunctionApp.Abstractions.Constants.Constants;

namespace EnterpriseServerless.FunctionApp.Abstractions.Models
{
    public class CallLog
    {
        public string id { get; private set; }
        public string callSid { get; private set; }
        public string callerNumber { get; set; }
        public string calledNumber { get; set; }
        public DateTime startTime { get; private set; }
        public DateTime? endTime { get; set; }
        public int statusId { get; set; }
        public string statusName { get; set; }
        public string routeKey { get; set; }
        public DateTime lastUpdateDate { get; set; }
        public int ttl { get; set; }

        public CallLog(string callSid)
        {
            this.callSid = callSid;
            this.id = callSid;
            this.startTime = DateTime.UtcNow;
            this.routeKey = string.Empty;
            this.lastUpdateDate = DateTime.UtcNow;
            this.ttl = CosmosDb.DefaultTimeToLive;
        }
    }
}