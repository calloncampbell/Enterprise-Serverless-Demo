using System;
using System.Collections.Generic;

namespace EnterpriseServerless.FunctionApp.Abstractions.Models
{
    public class NumberRouteDetails
    {
        public bool featureA { get; set; }
        public bool featureB { get; set; }
        public bool featureC { get; set; }
        public List<Route> routes { get; set; }

    }
}