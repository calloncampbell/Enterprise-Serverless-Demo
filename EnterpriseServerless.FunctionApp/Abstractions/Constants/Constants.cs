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
            public const string DatabaseId = "EnterpriseServerless01";

            public const string CallLogCollection = "CallLog";
            public const string NumberRoutesCollection = "NumberRoutes";
        }
    }
}
