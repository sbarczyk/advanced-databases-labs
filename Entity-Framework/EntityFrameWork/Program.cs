using var context = new ProdContext();

var supplier = new SupplierCompany
{
    CompanyName = "TechWorld",
    Street = "Elektronowa 12",
    City = "Poznań",
    ZipCode = "60-123",
    BankAccountNumber = "1234567890"
};

var customer = new Customer
{
    CompanyName = "Jan Nowak",
    Street = "Słoneczna 5",
    City = "Warszawa",
    ZipCode = "00-001",
    Discount = 0.15
};

context.Add(supplier);
context.Add(customer);
context.SaveChanges();

Console.WriteLine("Dodano dostawcę i klienta w modelu dziedziczenia TPH.");