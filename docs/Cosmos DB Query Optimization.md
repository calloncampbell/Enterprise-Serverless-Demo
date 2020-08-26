# Cosmos DB Query Optimization

It's import to use the correct query when looking up data in Cosmos DB. As defined in the supporting [documentation](https://docs.microsoft.com/en-us/azure/cosmos-db/optimize-cost-queries)...

```
Reads in Azure Cosmos DB are typically ordered from fastest/most efficient to slower/less efficient in terms of throughput as follows:

- Point reads (key/value lookup on a single item ID and partition key).
- Query with a filter clause within a single partition key.
- Query without an equality or range filter clause on any property.
- Query without filters.

Because key/value lookups on the item ID are the most efficient kind of read, you should make sure item ID has a meaningful value.
```


A common mistake I've seen when people start working with Cosmos DB is to use the `container.GetItemQueryIterator` method which will return back a `FeedIterator`. Instead you should be using `container.ReadItemAsync` to lookup a specific value. Here is an example of both types and the results and cost associated.

Here is the less optimal way. Use the `container.GetItemQueryIterator` method, and when you look at the diagnostics it shows it uses **2.83 RU**.

```csharp
    // Cosmos DB queries are case sensitive.
    var query = @"SELECT * FROM c WHERE c.Number = @number";

    QueryDefinition queryDefinition = new QueryDefinition(query)
        .WithParameter("@number", number);

    List<RoutingRecord> results = new List<RoutingRecord>();
    using (FeedIterator<RoutingRecord> resultSetIterator = container.GetItemQueryIterator<RoutingRecord>(queryDefinition))
    {
        while (resultSetIterator.HasMoreResults)
        {
            FeedResponse<RoutingRecord> response = await resultSetIterator.ReadNextAsync();
            results.AddRange(response);
#if DEBUG
            if (response.Diagnostics != null)
            {
                _logger.LogDebug($"GetRouteDataAsync Cosmos DB diagnostics: {response.Diagnostics.ToString()}");
            }
#endif
        }
    }

    var result = results.Take(1).FirstOrDefault();
    if (result == null)
    {
        return null;
    }

    return result;
```


Here is the recommended and more optimized way. Use the `container.ReadItemAsync` method instead, and when you look at the diagnostics it shows it uses **1 RU**. A much faster and cheaper operation.

```csharp
    ItemResponse<RoutingRecord> response = await container.ReadItemAsync<RoutingRecord>(number, new PartitionKey(number));

#if DEBUG
    if (response.Diagnostics != null)
    {
        _logger.LogDebug($"GetRouteDataAsync Cosmos DB diagnostics: {response.Diagnostics.ToString()}");
    }
#endif

    if (response.StatusCode == HttpStatusCode.NotFound)
    {
        _logger.LogDebug("Number routing details for '{0}' not found", number);
        return null;
    }

    return response;
```

The second method is the preferred way to lookup a specific item as it does the operation in **1 RU** vs **2.83 RU**. You will need to check the `response.StatusCode` for an appropriate message to handle if the data doesnt exist, etc.


## References

https://docs.microsoft.com/en-us/azure/cosmos-db/optimize-cost-queries