namespace MickeyInfoUtility.Models
{
    public class RenovationItem
    {
        public string ItemName { get; set; }
        public decimal Quantity { get; set; }
        public string Measurement { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string ShopName { get; set; }
        public string Salesperson { get; set; }
        public string Contact { get; set; }
        public string InvoiceQuotationNumber { get; set; }
        public string Category { get; set; }
    }
}
