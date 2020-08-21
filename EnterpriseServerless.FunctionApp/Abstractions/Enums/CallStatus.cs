using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseServerless.FunctionApp.Abstractions.Enums
{
    public enum CallStatus
    {
        Accepted = 0,
        NoInput = 1,
        NotAssigned = 2,
        Blocked = 3,
        StopCall = 4,
        Error = 5
    }
}
