namespace DocumentDB1
{
    using Newtonsoft.Json;
    using System;

    public class SalesOrder
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string PurchaseOrderNumber { get; set; }
        public DateTime OrderDate { get; set; }
        public string AccountNumber { get; set; }
        public decimal SubTotal { get; set; }
    }
}
