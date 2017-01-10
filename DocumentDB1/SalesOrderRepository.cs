namespace DocumentDB1
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Azure.Documents.Client;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Documents;

    public class SalesOrderRepository
    {
        private DocumentClient client;
        private const string SalesDatabaseName = "Sales";
        private const string SalesOrderCollectionName = "SalesOrders";

        public SalesOrderRepository()
        {
            var serviceEndpoint = ConfigurationManager.AppSettings["ServiceEndpoint"];
            var authKey = ConfigurationManager.AppSettings["AuthKey"];
            client = new DocumentClient(new Uri(serviceEndpoint), authKey);
        }

        public void SetupOrderTestData()
        {
            try
            {
                RunTest().Wait();
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }
        }

        private async Task RunTest()
        {
            var database = await GetNewDatabaseAsync(SalesDatabaseName);

            await CreateCollection(database, SalesOrderCollectionName);

            await FillCollection(SalesDatabaseName, SalesOrderCollectionName);
        }

        /// <summary>
        /// Create a new database (delete any existing database with the same id)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private async Task<Database> GetNewDatabaseAsync(string id)
        {
            Database database = client.CreateDatabaseQuery().Where(c => c.Id == id).ToArray().SingleOrDefault();
            if (database != null)
            {
                await client.DeleteDatabaseAsync(database.SelfLink);
            }

            database = await client.CreateDatabaseAsync(new Database { Id = id });
            return database;
        }

        private async Task CreateCollection(Database database, string collectionId)
        {

            var collectionOptions = new DocumentCollection
            {
                Id = collectionId,
                IndexingPolicy =
                {
                    Automatic = true,
                    IndexingMode = IndexingMode.Consistent
                }
            };


            // Minimum Collection Throughput is 400RUs
            var collection =
                await
                    client.CreateDocumentCollectionAsync(database.SelfLink, collectionOptions,
                        new RequestOptions { OfferThroughput = 400 });

            Console.WriteLine("Collection {0} created with index policy \n{1}", collection.ActivityId, collection.StatusCode);
        }

        private async Task FillCollection(string databaseId, string collectionId)
        {
            Stopwatch s = new Stopwatch();

            s.Start();

            for (var outerIndex = 0; outerIndex < 10; outerIndex++)
            {
                var allTasks = new Task<ResourceResponse<Document>>[100];
                var collectionLink = UriFactory.CreateDocumentCollectionUri(databaseId, collectionId);

                for (var index = 0; index < 100; index++)
                {
                    allTasks[index] = this.client.CreateDocumentAsync(collectionLink,
                        CreateSalesOrder(index + (100 * outerIndex)));
                }

                await Task.WhenAll(allTasks);

                Console.WriteLine("Batch {0}", outerIndex);
            }

            s.Stop();

            Console.WriteLine(s.ElapsedMilliseconds);

        }

        private SalesOrder CreateSalesOrder(int index)
        {
            return new SalesOrder
            {
                Id = String.Format("SalesOrder-{0}", index),
                PurchaseOrderNumber = (index + 200).ToString(CultureInfo.InvariantCulture),
                AccountNumber = (index + 100).ToString(CultureInfo.InvariantCulture),
                OrderDate = DateTime.Today.AddMinutes(index),
                SubTotal = index + 200M
            };
        }
    }
}