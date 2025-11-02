public class Product
{
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public int UnitsInStock { get; set; }

    public int SupplierID { get; set; } // teraz nie nullable, bo każdy produkt musi mieć dostawcę
    public Supplier Supplier { get; set; } = null!;

    // Nawigacja — lista wszystkich faktur, na których ten produkt występuje
    public ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();
}