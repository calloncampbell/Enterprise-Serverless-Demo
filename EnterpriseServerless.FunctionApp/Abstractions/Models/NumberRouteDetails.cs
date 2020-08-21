using System;
using System.Collections.Generic;

namespace EnterpriseServerless.FunctionApp.Abstractions.Models
{
    public class NumberRouteDetails
    {
        public string number { get; set; }
        public bool featureA { get; set; }
        public bool featureB { get; set; }
        public bool featureC { get; set; }
        public List<RouteDetails> routes { get; set; }

    }
}