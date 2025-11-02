public class InvoiceProduct
{
    public int InvoiceID { get; set; }          // Klucz obcy do faktury
    public Invoice Invoice { get; set; } = null!;

    public int ProductID { get; set; }          // Klucz obcy do produktu
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }           // Ilość sprzedanych sztuk tego produktu na tej fakturze
}