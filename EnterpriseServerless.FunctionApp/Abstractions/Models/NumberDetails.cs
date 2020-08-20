using System;
using System.Collections.Generic;
using System.Text;
using static EnterpriseServerless.FunctionApp.Abstractions.Constants.Constants;

namespace EnterpriseServerless.FunctionApp.Abstractions.Models
{
    public class NumberDetails
    {
        public string id { get; set; }
        public string number { get; set; }
        public DateTime lastUpdateDate { get; set; }
        public bool isActive { get; set; }
        public NumberRouteDetails routeDetails { get; set; }
        public int ttl { get; set; } = CosmosDb.DefaultTimeToLive;
    }
}