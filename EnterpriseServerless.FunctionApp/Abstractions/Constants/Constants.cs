using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseServerless.FunctionApp.Abstractions.Constants
{
    public class Constants
    {
        public class CosmosDb
        {
            public const int DefaultTimeToLive = -1;

            public const string Connection = "CosmosDB-ConnectionStringReadWrite";
            public const string DatabaseId = "Calls";

            public const string CallLogCollection = "CallLog";
            public const string NumberDetailsCollection = "NumberDetails";
        }
    }
}
