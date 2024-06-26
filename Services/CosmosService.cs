using Microsoft.Azure.Cosmos.Samples.Web.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace Microsoft.Azure.Cosmos.Samples.Web.Services;

public class CosmosService : ICosmosService
{ 
    private readonly CosmosClient _client;

    private IConfiguration _configuration;
    
    public CosmosService()
    {
        _client = new CosmosClient(
            connectionString: "AccountEndpoint=https://cosmosrgeastus35e99bac-a9a5-4aae-98dcdb.documents.azure.com:443/;AccountKey=eW8h8Z3nyA9MbL1IpMiXPRt77eYm4h53eC8EuZY4V1xcbN2ctFmXkhnmy6diAHjYGVxuQzCQPdMWACDbVNAE5g==;",
        
        new CosmosClientOptions()
        {
            ApplicationRegion = Regions.FranceCentral,
        });
    }

    private Container container
    {
        get => _client.GetDatabase("cosmicworks").GetContainer("products");
    }

    public async Task<IEnumerable<Product>> RetrieveAllProductsAsync()
    { 
        var queryable = container.GetItemLinqQueryable<Product>();

        using FeedIterator<Product> feed = queryable
            .Where(p => p.price < 2000m)
            .OrderByDescending(p => p.price)
            .ToFeedIterator();

        List<Product> results = new();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            foreach (Product item in response)
            {
                results.Add(item);
            }
        }

        return results;
    }

    public async Task<IEnumerable<Product>> RetrieveActiveProductsAsync()
    { 
        string sql = """
        SELECT
            p.id,
            p.categoryId,
            p.categoryName,
            p.sku,
            p.name,
            p.description,
            p.price,
            p.tags
        FROM products p
        JOIN t IN p.tags
        WHERE t.name = @tagFilter
        """;

        var query = new QueryDefinition(
            query: sql
        )
            .WithParameter("@tagFilter", "Tag-75");

        using FeedIterator<Product> feed = container.GetItemQueryIterator<Product>(
            queryDefinition: query
        );

        List<Product> results = new();

        while (feed.HasMoreResults)
        {
            var response = await feed.ReadNextAsync();
            foreach (Product item in response)
            {
                results.Add(item);
            }
        }

        return results;
    }
}
