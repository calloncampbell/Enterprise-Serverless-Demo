using System;
using System.Collections.Generic;
using System.Text;

namespace EnterpriseServerless.FunctionApp.Abstractions.Constants
{
    public class Constants
    {
        public class EnvironmentVariables
        {
            public const string RegionName = "REGION_NAME";
        }

        public class CosmosDb
        {
            public const int DefaultTimeToLive = -1;

            public const string Connection = "CosmosDB-ConnectionStringReadWrite";
            public const string DatabaseId = "EnterpriseServerless01";

            public const string CallLogCollection = "CallLog";
            public const string NumberRoutesCollection = "NumberRoutes";
        }

        public class StorageAccount
        {
            public const string ConnectionString = "FunctionStorageBindings-StorageConnectionString";

            // Read-access geo-redundant storage (RA-GRS)
            public const string SecondaryConnectionUrl = "GeoRedundantStorage-SecondaryUrl";
            public const string GeoRedundantStorageMaxRetries = "GeoRedundantStorage-MaxRetries";
            public const string GeoRedundantStorageDelayInSeconds = "GeoRedundantStorage-DelayInSeconds";
            public const string GeoRedundantStorageMaxDelayInSeconds = "GeoRedundantStorage-MaxDelayInSeconds";

            // Blob Containers
            public const string TwilioMediaFilesBlobContainer = "media-files";
        }
    }
}
