namespace DocumentDB1
{
    class Program
    {
        static void Main(string[] args)
        {
            var repository = new SalesOrderRepository();
            
            repository.SetupOrderTestData();
        }
    }
}
