using System;

namespace EnterpriseServerless.FunctionApp.Abstractions.Models
{
    public class RouteDetails
    {
        public string terminationNumber { get; set; }
        public bool callRecordEnabled { get; set; }
        public bool? callRecordConfirmationRequired { get; set; }
        public string callRecordNotificationText { get; set; }
        public string callRecordNotificationFileId { get; set; }
        public string callRecordNotificationFileName { get; set; }
        public int? callRecordNotificationFileType { get; set; }
        public string callServiceUrlParameter { get; set; }
    }
}