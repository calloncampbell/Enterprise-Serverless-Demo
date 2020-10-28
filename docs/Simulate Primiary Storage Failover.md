# Simulate Primiary Storage Failover

## Prerequisites

- Set replication type to `Read-access geo-redundant storage (RA-GRS)` on the storage account. This is done in the **Configuration** section under **Settings**. 
- Update the application to support geo redundant storage by configuring the **BlobClientOptions** and setting the `GeoRedundantSecondaryUri` to that of the **Secondary Blob Service Endpoint**. This works for read access only as the secondary does not allow writes unless a failure has occured and its taken over.

    ```csharp
    var options = new BlobClientOptions
    {
        Diagnostics = { IsLoggingEnabled = true },
        GeoRedundantSecondaryUri = new Uri(_configuration[Constants.StorageAccount.SecondaryConnectionUrl]),
        Retry =
        {
            Mode = RetryMode.Exponential,
            MaxRetries = int.Parse(_configuration[Constants.StorageAccount.GeoRedundantStorageMaxRetries] ?? "3"),
            Delay = TimeSpan.FromSeconds(double.Parse(_configuration[Constants.StorageAccount.GeoRedundantStorageDelayInSeconds] ?? "0.1")),
            MaxDelay = TimeSpan.FromSeconds(double.Parse(_configuration[Constants.StorageAccount.GeoRedundantStorageMaxDelayInSeconds] ?? "2"))
        }
    };

    BlobClient blobClient = new BlobClient(_configuration[Constants.StorageAccount.ConnectionString], Constants.StorageAccount.TwilioMediaFilesBlobContainer, cloudFile, options);
    ```

## Getting started

First validate that the primary and secondary storage account URL works for a given file. 

Sample file accessed from Storage account URL:
- https://stentserverless101-secondary.blob.core.windows.net/media-files/1234/CallRecordNotificationFiles/51/51a1de47-681d-4c4e-83f3-4bbf0ca45e6a-Default_Notify.wav


Sample file accessed from Storage account secondary URL:
- https://stentserverless101-secondary.blob.core.windows.net/media-files/1234/CallRecordNotificationFiles/51/51a1de47-681d-4c4e-83f3-4bbf0ca45e6a-Default_Notify.wav

Let's now make a valid Function App `MediaFile` request and ensure it returns the expected media file. This should return back almost immediately (155ms). The following is a the payload coming from Twilio when requesting the above media file.
    
    ```http
    GET http://localhost:7071/api/mediafile?mrid=NTFhMWRlNDctNjgxZC00YzRlLTgzZjMtNGJiZjBjYTQ1ZTZhLURlZmF1bHRfTm90aWZ5Lndhdg&tenantId=1234
    ```

Now lets simulate a failure by creating an invalid static route. This will apply to all requests to the primary endpoint of your read-access geo-redundant (RA-GRS) storage account.

### Simulate primary endpoint failure

1. Open a command prompt on Windows as an administrator or run terminal as root on Linux.
1. Get information about the storage account primary endpoint domain by entering the following command on a command prompt or terminal, replacing `STORAGEACCOUNTNAME` with the name of your storage account.

    ```powershell
    nslookup STORAGEACCOUNTNAME.blob.core.windows.net 
    ```
1. Copy to the IP address of your storage account to a text editor for later use.
1. Next get the IP address of your local host, type `ipconfig` on the Windows command prompt, or `ifconfig` on the Linux terminal.
1. To add a static route for a destination host, type the following command on a Windows command prompt or Linux terminal, replacing <destination_ip> with your storage account IP address and <gateway_ip> with your local host IP address. If your testing this locally the <gateway_ip> should be your router IP = 192.168.0.1 for example.

    Linux:
    ```powershell
    route add <destination_ip> gw <gateway_ip>
    ```

    Windows:
    ```powershell
    route add <destination_ip> <gateway_ip>
    ```
1. Now try accessing the media file from the Storage account primary URL as shown above. It should fail with a timeout.
    - https://stentserverless101-secondary.blob.core.windows.net/media-files/1234/CallRecordNotificationFiles/51/51a1de47-681d-4c4e-83f3-4bbf0ca45e6a-Default_Notify.wav

1. Now make another call to the Function App `MediaFile` request and this time it should work buy take much longer (21534ms) due to the retry configuration that was highlighted in the **Prerequisites** section.

    ```http
    GET http://localhost:7071/api/mediafile?mrid=NTFhMWRlNDctNjgxZC00YzRlLTgzZjMtNGJiZjBjYTQ1ZTZhLURlZmF1bHRfTm90aWZ5Lndhdg&tenantId=1234
    ```

### Simulate primary endpoint restoration

To simulate the primary endpoint becoming functional again, delete the invalid static route from the routing table. This allows all requests to the primary endpoint to be routed through the default gateway. Type the following command on a Windows command prompt or Linux terminal.

Linux:
```powershell
route del <destination_ip> gw <gateway_ip>
```

Windows:
```powershell
route delete <destination_ip>
```

You can then resume the application or press the appropriate key to download the sample file again, this time confirming that it once again comes from primary storage.


## References

https://docs.microsoft.com/en-us/azure/storage/common/storage-redundancy
https://docs.microsoft.com/en-us/azure/storage/blobs/simulate-primary-region-failure