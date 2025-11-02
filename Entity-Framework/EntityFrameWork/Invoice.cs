public class Invoice
{
    public int InvoiceID { get; set; }                // Klucz główny faktury
    public string InvoiceNumber { get; set; } = string.Empty;   // Numer faktury

    // Nawigacja — lista produktów przypisanych do faktury
    public ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();
}