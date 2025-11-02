Autorzy rozwiązania:
Szymon Barczyk
Jan Dyląg
# Część pierwsza - przewodnikowa

WAŻNA INFORMACJA: pierwsza część sprawozdania jest o parta o najnowszą wersję dotnet'a (tj. 9.0.300), więc wszystkie dodatkowe pakiety będą instalowane bez korekty dotyczącej wersji.
## Klasa Product
```cs
public class Product

{

    public int ProductID { get; set; }

    public String? ProductName { get; set; }

    public int UnitsInStock { get; set; }

}
```

w klasie Product, zdefiniowane jest jakie pola posiada każdy z produktów.

## klasa ProdContext
```cs
using Microsoft.EntityFrameworkCore;

public class ProdContext : DbContext
{
	public DbSet<Product> Products { get; set; }
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

    {

        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlite("Datasource=MyProductDatabase");
    }

}
```

Klasa ProdContext jest niezbędna do działania całej bazy w Entity Framework, ona łączy kod w C# i bazą SQLite.

## klasa Program
```cs
using System;
using System.Linq;

Console.WriteLine("Podaj nazwę produktu: ");
Console.WriteLine("-----------");
String? prodName = Console.ReadLine();
Console.WriteLine("-----------");
Console.Write("Podaj liczbę sztuk w magazynie: ");
int unitsInStock = int.Parse(Console.ReadLine());

ProdContext ProdContext = new ProdContext();

Product product = new Product { ProductName = prodName, UnitsInStock = unitsInStock };

ProdContext.Products.Add(product);
ProdContext.SaveChanges();

  
var query = from prod in ProdContext.Products
			where prod.UnitsInStock > 0
			select prod;

Console.WriteLine("\nProdukty dostępne w magazynie:");
foreach (var p in query)
{
	Console.WriteLine($"Nazwa: {p.ProductName}, Ilość: {p.UnitsInStock}");
}
```

Ta klasa jest głową całego programu, za jej pośrednictwem wykonywane są operację na bazie danych.

### Zmiany
```cs
Console.WriteLine("-----------");
Console.Write("Podaj liczbę sztuk w magazynie: ");
int unitsInStock = int.Parse(Console.ReadLine());

Product product = new Product { ProductName = prodName, UnitsInStock = unitsInStock };
```

Warto wspomnieć że zmieniliśmy trochę proces dodawania produktów do bazy. Dodaliśmy opcję podania ilości sztuk danego produktu. Uznaliśmy, że skoro w klasie Product jest takie pole to można to wykorzystać i lekko rozbudować tą bazę.

```cs
var query = from prod in ProdContext.Products
			where prod.UnitsInStock > 0
			select prod;

Console.WriteLine("\nProdukty dostępne w magazynie:");
foreach (var p in query)
{
	Console.WriteLine($"Nazwa: {p.ProductName}, Ilość: {p.UnitsInStock}");
}
```

Również na rzecz ćwiczenia dla przejrzystości dodaliśmy taki fragment kodu który wypisuje w konsoli produkty które spełniają warunek __prod.UnitsInStock > 0__.

## Końcowy rezultat

```cmd
Podaj nazwę produktu: 
-----------
klej
-----------
Podaj liczbę sztuk w magazynie: 3

Produkty dostępne w magazynie:
Nazwa: dlugopis, Ilość: 2
Nazwa: klej, Ilość: 3
```

Tak prezentuje się w konsoli proces dodawania produktów do bazy. 

# Część samodzielna
## a. Zmodyfikuj model wprowadzając pojęcie Dostawcy

Krok 1: Dodajemy klasę Supplier
``` cs
public class Supplier
{
    public int SupplierID { get; set; }

    public string? CompanyName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
}
```

Krok 2: Modyfikujemy klasę Product
```cs
public class Product

{
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public int UnitsInStock { get; set; }

    public int SupplierID { get; set; } 
    public Supplier Supplier { get; set; } = null!;

}
```

Krok 3: Modyfikacja ProductContext:
```cs
using Microsoft.EntityFrameworkCore;

public class ProductContext : DbContext
{
	public DbSet<Product> Products { get; set; }
	public DbSet<Supplier> Suppliers { get; set; }
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)

{
	base.OnConfiguring(optionsBuilder);
	optionsBuilder.UseSqlite("Datasource=MyProductDatabase");
}

}
```

Krok 4: Aktualizacja bazy:
```bash
dotnet ef migrations add AddSupplier
dotnet ef database update
```

Krok 5: 
Znalezienie ostatniego dodanego produktu i przypisanie mu właśnie dodanego dostawcy:

```cs
using var context = new ProdContext();
// Tworzymy dostawcę
var supplier = new Supplier

{
CompanyName = "Kolor Hurt",
Street = "Tęczowa 5",
City = "Kraków"
};

context.Suppliers.Add(supplier);
context.SaveChanges();

// Przypisujemy dostawcę ostatniemu produktowi

var lastProduct = context.Products
.OrderByDescending(p => p.ProductID)
.First();

lastProduct.SupplierID = supplier.SupplierID;
context.SaveChanges();

Console.WriteLine($"Produkt '{lastProduct.ProductName}' przypisano do dostawcy '{supplier.CompanyName}'.");
```

![[Pasted image 20250521110045.png]]
Potwierdzenie wykonania polecenia.
![[Pasted image 20250521110100.png]]
![[Pasted image 20250521110108.png]]


## b. Zmodyfikuj model wprowadzając pojęcie Dostawcy jak poniżej

Aby wykonać punkt b modyfikujemy Product i Supplier:
```cs
public class Supplier
{
    public int SupplierID { get; set; }
    public string? CompanyName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
```

```cs
public class Product
{
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public int UnitsInStock { get; set; }
}
```

Dodanie dostawcy z kilkoma produktami:
```cs
using System;
using System.Collections.Generic;
using System.Linq;

class Program
{
    static void Main(string[] args)
    {
        using var context = new ProdContext();

        // Tworzymy nowego dostawcę z produktami
        var supplier = new Supplier
        {
            CompanyName = "BiuroMix",
            Street = "Papierowa 8",
            City = "Wrocław",
            Products = new List<Product>
            {
                new Product { ProductName = "Długopis", UnitsInStock = 50 },
                new Product { ProductName = "Cyrkiel", UnitsInStock = 30 },
                new Product { ProductName = "Zeszyt", UnitsInStock = 100 }
            }
        };

        context.Suppliers.Add(supplier);
        context.SaveChanges();

        Console.WriteLine($"Dodano dostawcę '{supplier.CompanyName}' z 3 produktami.");
    }
}
```
![[Pasted image 20250521110442.png]]

Zapytanie potwierdzające wykonanie:
```sql
SELECT
    s.CompanyName AS Dostawca,
    p.ProductName AS Produkt,
    p.UnitsInStock AS "Ilość w magazynie"
FROM
    Suppliers s
JOIN
    Products p ON p.SupplierID = s.SupplierID
ORDER BY
    s.CompanyName;
```

![[Pasted image 20250521110602.png]]

## C.  Zamodeluj relację dwustronną jak poniżej:
Jest to połączenie punkt a i b.
## Modyfikacja Supplier:
```cs
public class Supplier

{
    public int SupplierID { get; set; }
    public string? CompanyName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();

}
```

## Modyfikacja Products:
```cs
public class Product
{
    public int ProductID { get; set; }
    public string? ProductName { get; set; }
    public int UnitsInStock { get; set; }

    public int SupplierID { get; set; } // teraz nie nullable, bo każdy produkt musi mieć dostawcę
    public Supplier Supplier { get; set; } = null!;
}
```

```
dotnet ef migrations add AddBidirectionalRelation
dotnet ef database update
```

## Sprawdzenie dodania:
```sql
SELECT
    s.CompanyName AS Dostawca,
    p.ProductName AS Produkt,
    p.UnitsInStock AS "Ilość w magazynie"
FROM
    Suppliers s
JOIN
    Products p ON p.SupplierID = s.SupplierID
ORDER BY
    s.CompanyName;
```
![[Pasted image 20250603124529.png]]
Jak widać na 3 ostatnich pozycjach znajdują się nowe produkty wraz z ich dostawcą. 

# D. 
## Modyfikacja Product:
```cs
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
```
- Dodajemy nawigację InvoiceProducts, która prowadzi do encji pośredniej (łączącej produkt i fakturę).
- Dzięki temu EF Core będzie wiedział, że ten produkt może pojawiać się na wielu fakturach.

## Definicja faktury:
```cs
public class Invoice
{
    public int InvoiceID { get; set; }                // Klucz główny faktury
    public string InvoiceNumber { get; set; } = string.Empty;   // Numer faktury

    // Nawigacja — lista produktów przypisanych do faktury
    public ICollection<InvoiceProduct> InvoiceProducts { get; set; } = new List<InvoiceProduct>();
}
```

- Klasyczna encja faktury.
- Analogicznie do Product — nawigacja InvoiceProducts pozwala łączyć fakturę z produktami.

##  Encja pośrednia (kluczowa):
```cs
public class InvoiceProduct
{
    public int InvoiceID { get; set; }          // Klucz obcy do faktury
    public Invoice Invoice { get; set; } = null!;

    public int ProductID { get; set; }          // Klucz obcy do produktu
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }           // Ilość sprzedanych sztuk tego produktu na tej fakturze
}
```
- Tworzymy osobną encję łączącą fakturę i produkt.
- Dodatkowo dodajemy Quantity, bo właśnie dlatego nie możemy użyć zwykłego wiele-do-wielu.

## Modyfikacja ProdContext:
```cs
using Microsoft.EntityFrameworkCore;

public class ProductContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<InvoiceProduct> InvoiceProducts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Datasource=MyProductDatabase");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<InvoiceProduct>()
            .HasKey(ip => new { ip.InvoiceID, ip.ProductID });  // Definiujemy klucz złożony

        modelBuilder.Entity<InvoiceProduct>()
            .HasOne(ip => ip.Invoice)
            .WithMany(i => i.InvoiceProducts)
            .HasForeignKey(ip => ip.InvoiceID);  // Powiązanie z Invoice

        modelBuilder.Entity<InvoiceProduct>()
            .HasOne(ip => ip.Product)
            .WithMany(p => p.InvoiceProducts)
            .HasForeignKey(ip => ip.ProductID);  // Powiązanie z Product
    }
}
```

- Musimy jawnie powiedzieć EF Core, że InvoiceProduct ma klucz złożony (InvoiceID + ProductID).
- Konfigurujemy relacje wiele-do-jednego w obu kierunkach:
    - InvoiceProduct wskazuje na Invoice.
    - InvoiceProduct wskazuje na Product.

```
dotnet ef migrations add AddManyToManyWithQuantity
dotnet ef database update
```

## Program.cs
```cs
using var context = new ProdContext();
// Tworzymy dostawcę
var supplier = new Supplier
{
	CompanyName = "TechWorld",
	Street = "Elektronowa 12",
	City = "Poznań"
};

// Tworzymy produkty i przypisujemy im dostawcę
var product1 = new Product
{
	ProductName = "Laptop X200",
	UnitsInStock = 15,
	Supplier = supplier
};

var product2 = new Product
{
	ProductName = "Monitor 24\"",
	UnitsInStock = 25,
	Supplier = supplier
};

var product3 = new Product
{
	ProductName = "Klawiatura Gamingowa",
	UnitsInStock = 50,
	Supplier = supplier
};

// Dodajemy dostawcę i produkty za jednym razem
context.AddRange(product1, product2, product3);
context.SaveChanges();

Console.WriteLine("Dodano dostawcę wraz z produktami.");

// Tworzymy faktury
var invoice1 = new Invoice { InvoiceNumber = "FV/2025/001" };
var invoice2 = new Invoice { InvoiceNumber = "FV/2025/002" };
  
context.Invoices.AddRange(invoice1, invoice2);
context.SaveChanges();

// Teraz tworzymy powiązania: na jakiej fakturze jaki produkt i ile sztuk sprzedano
var ip1 = new InvoiceProduct
{
	InvoiceID = invoice1.InvoiceID,
	ProductID = product1.ProductID,
	Quantity = 2
};

var ip2 = new InvoiceProduct
{
	InvoiceID = invoice1.InvoiceID,
	ProductID = product2.ProductID,
	Quantity = 1
};

var ip3 = new InvoiceProduct
{
	InvoiceID = invoice2.InvoiceID,
	ProductID = product1.ProductID,
	Quantity = 1
};

var ip4 = new InvoiceProduct
{
	InvoiceID = invoice2.InvoiceID,
	ProductID = product3.ProductID,
	Quantity = 5
};

context.InvoiceProducts.AddRange(ip1, ip2, ip3, ip4);
context.SaveChanges();

Console.WriteLine("Dodano faktury oraz sprzedaż na fakturach.");
```

##  Wszystkie faktury oraz produkty na nich
```sql
SELECT 
    i.InvoiceNumber AS Faktura, 
    p.ProductName AS Produkt, 
    ip.Quantity AS Ilosc
FROM 
    InvoiceProducts ip
JOIN 
    Invoices i ON ip.InvoiceID = i.InvoiceID
JOIN 
    Products p ON ip.ProductID = p.ProductID
ORDER BY 
    i.InvoiceNumber;
```
![[Pasted image 20250603131547.png]]

##  Pokaż na jakich fakturach występuje dany produkt (np. Laptop X200)
```sql
SELECT 
    p.ProductName AS Produkt,
    i.InvoiceNumber AS Faktura,
    ip.Quantity AS Ilosc
FROM 
    InvoiceProducts ip
JOIN 
    Invoices i ON ip.InvoiceID = i.InvoiceID
JOIN 
    Products p ON ip.ProductID = p.ProductID
WHERE 
    p.ProductID = 11;
```

![[Pasted image 20250603131833.png]]

```
.schema
```
![[Pasted image 20250603132118.png]]

## E.
LOGIKA TPH
- Klasa bazowa: Company.
- Dwie klasy dziedziczące: Supplier i Customer.
- Każdy rekord w tabeli Company będzie miał dodatkową kolumnę (Discriminator), która mówi, czy to jest Supplier czy Customer.

## Dodanie NOWEGO CompanyContext.cs
```cs
using Microsoft.EntityFrameworkCore;

public class CompanyContext : DbContext
{
	public DbSet<Company> Companies { get; set; }
	
	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseSqlite("Data Source=CompanyDatabase.db");
	}
	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<SupplierCompany>();
		modelBuilder.Entity<Customer>();
	}
}
```

- Dodajemy DbSet Company — bo EF Core potrzebuje tylko DbSet klasy bazowej do mapowania całej hierarchii dziedziczenia.

## Utworzenie klasy bazowe: Company
```cs
public class Company

{
    public int CompanyID { get; set; }
    public string? CompanyName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
}
```

## Nowa klasa: SupplierCompany
Uwaga: **zmieniamy nazwę klasy Supplier na SupplierCompany,** żeby nie konfliktowało z dotychczasowym Supplier (który modelował dostawcę produktów).
```cs
public class SupplierCompany : Company

{
    public string? BankAccountNumber { get; set; }
}
```

## Customer:
```cs
public class Customer : Company
{
    public double Discount { get; set; }
}
```

## Program.cs

```cs
using var context = new CompanyContext();

// Dostawcy
var supplier1 = new SupplierCompany
{
    CompanyName = "TechWorld",
    Street = "Elektronowa 12",
    City = "Poznań",
    ZipCode = "60-123",
    BankAccountNumber = "1234567890"
};

var supplier2 = new SupplierCompany
{
    CompanyName = "OfficePlus",
    Street = "Biurkowa 8",
    City = "Wrocław",
    ZipCode = "50-001",
    BankAccountNumber = "9876543210"
};

var supplier3 = new SupplierCompany
{
    CompanyName = "GastroHurt",
    Street = "Młyńska 5",
    City = "Gdańsk",
    ZipCode = "80-123",
    BankAccountNumber = "5555555555"
};

// Klienci
var customer1 = new Customer
{
    CompanyName = "Jan Nowak",
    Street = "Słoneczna 5",
    City = "Warszawa",
    ZipCode = "00-001",
    Discount = 0.15
};

var customer2 = new Customer
{
    CompanyName = "Kancelaria Kowalski",
    Street = "Legalna 20",
    City = "Kraków",
    ZipCode = "30-001",
    Discount = 0.10
};

var customer3 = new Customer
{
    CompanyName = "Startup X",
    Street = "Innowacyjna 11",
    City = "Katowice",
    ZipCode = "40-001",
    Discount = 0.20
};

// Dodajemy dane do bazy
context.AddRange(supplier1, supplier2, supplier3);
context.AddRange(customer1, customer2, customer3);
context.SaveChanges();

Console.WriteLine("Dodano kilku dostawców i klientów w modelu dziedziczenia TPH.");
```

```
dotnet ef migrations add InitialCompany --context CompanyContext
dotnet ef database update --context CompanyContext
dotnet run
sqlite3 CompanyDatabase.db
.schema Companies
```
![[Pasted image 20250603140742.png]]
```sql
SELECT * FROM Companies;
```
![[Pasted image 20250603183357.png]]

## F.
##  **Klasy domenowe pozostają te same.**
```cs
public class Company
{
    public int CompanyID { get; set; }
    public string? CompanyName { get; set; }
    public string? Street { get; set; }
    public string? City { get; set; }
    public string? ZipCode { get; set; }
}

public class SupplierCompany : Company
{
    public string? BankAccountNumber { get; set; }
}

public class Customer : Company
{
    public double Discount { get; set; }
}
```

#### **DbContext dla TPT** 
```cs
using Microsoft.EntityFrameworkCore;

public class CompanyContextTPT : DbContext
{
    public DbSet<Company> Companies { get; set; }
    public DbSet<SupplierCompany> SupplierCompanies { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=CompanyDatabaseTPT.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Company>().ToTable("Companies");
        modelBuilder.Entity<SupplierCompany>().ToTable("SupplierCompanies");
        modelBuilder.Entity<Customer>().ToTable("Customers");
    }
}
```

- Używamy ToTable() aby powiedzieć EF Core, że mamy TPT — osobne tabele.
- EF Core utworzy 3 fizyczne tabele w bazie.


```
dotnet ef migrations add InitialCompanyTPT --context CompanyContextTPT
dotnet ef database update --context CompanyContextTPT
sqlite3 CompanyDatabaseTPT.db
.schema
```
![[Pasted image 20250603141546.png]]

## Program.cs
```cs
using var context = new CompanyContextTPT();

// Dostawcy
var supplier1 = new SupplierCompany
{
    CompanyName = "TechWorld",
    Street = "Elektronowa 12",
    City = "Poznań",
    ZipCode = "60-123",
    BankAccountNumber = "1234567890"
};

var supplier2 = new SupplierCompany
{
    CompanyName = "OfficePlus",
    Street = "Biurkowa 8",
    City = "Wrocław",
    ZipCode = "50-001",
    BankAccountNumber = "9876543210"
};

var supplier3 = new SupplierCompany
{
    CompanyName = "GastroHurt",
    Street = "Młyńska 5",
    City = "Gdańsk",
    ZipCode = "80-123",
    BankAccountNumber = "5555555555"
};

// Klienci
var customer1 = new Customer
{
    CompanyName = "Jan Nowak",
    Street = "Słoneczna 5",
    City = "Warszawa",
    ZipCode = "00-001",
    Discount = 0.15
};

var customer2 = new Customer
{
    CompanyName = "Kancelaria Kowalski",
    Street = "Legalna 20",
    City = "Kraków",
    ZipCode = "30-001",
    Discount = 0.10
};

var customer3 = new Customer
{
    CompanyName = "Startup X",
    Street = "Innowacyjna 11",
    City = "Katowice",
    ZipCode = "40-001",
    Discount = 0.20
};

// Dodajemy dane do bazy
context.AddRange(supplier1, supplier2, supplier3);
context.AddRange(customer1, customer2, customer3);
context.SaveChanges();

Console.WriteLine("Dodano testowe dane (dostawcy i klienci) w modelu TPT.");
```

```sql
SELECT * FROM Companies;
```
![[Pasted image 20250603183001.png]]


# **Wyświetlenie dostawców**
```sql
SELECT c.CompanyID, c.CompanyName, c.City, s.BankAccountNumber
FROM Companies c
JOIN SupplierCompanies s ON c.CompanyID = s.CompanyID;
```

![[Pasted image 20250603183036.png]]
## Wyświetlanie klientów:
```sql
SELECT c.CompanyID, c.CompanyName, c.City, cu.Discount
FROM Companies c
JOIN Customers cu ON c.CompanyID = cu.CompanyID;
```

![[Pasted image 20250603183049.png]]

## G. Porównanie strategii dziedziczenia TPH i TPT w Entity Framework Core

---

### 1.  Table-Per-Hierarchy (TPH)

- Cała hierarchia dziedziczenia jest zapisana w jednej tabeli w bazie danych.  
- EF Core tworzy dodatkową kolumnę `Discriminator`, w której przechowuje typ obiektu.  
- Wszystkie właściwości klas dziedziczących znajdują się w jednej tabeli — pola specyficzne dla podtypów mogą być puste (`NULL`) dla innych rekordów.

Przykładowa tabela:

```sql
CREATE TABLE "Companies" (
    "CompanyID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "CompanyName" TEXT,
    "Street" TEXT,
    "City" TEXT,
    "ZipCode" TEXT,
    "BankAccountNumber" TEXT,
    "Discount" REAL NOT NULL,
    "Discriminator" TEXT NOT NULL
);

```
#### **Zalety TPH:**

- Bardzo wydajna pod względem odczytu (jedna tabela = jeden JOIN).
    
- Prosta i szybka implementacja.
    
- Mało skomplikowane migracje.
    
- W pełni zgodne z domyślnym zachowaniem EF Core.
    

  

#### **Wady TPH:**

- Dużo pól NULL dla niektórych typów.
    
- Przy dużej liczbie podtypów tabela może stać się szeroka i trudna do zarządzania.
    
- Trudniej zapewnić spójność danych na poziomie schematu (np. wymuszanie NOT NULL na właściwościach występujących tylko w podtypach).
### 2. **Table-Per-Type (TPT)**

  

- Każda klasa w hierarchii ma własną tabelę w bazie danych.

- Klasy pochodne zawierają tylko dodatkowe kolumny, specyficzne dla danego typu.

- Powiązania między tabelami realizowane są przez klucz główny CompanyID oraz klucze obce (FK).

- EF Core automatycznie wykonuje odpowiednie JOIN-y podczas zapytań.

Przykładowe wygenerowane tabele:  

```sql
CREATE TABLE "Companies" (
    "CompanyID" INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
    "CompanyName" TEXT,
    "Street" TEXT,
    "City" TEXT,
    "ZipCode" TEXT
);

CREATE TABLE "SupplierCompanies" (
    "CompanyID" INTEGER NOT NULL PRIMARY KEY,
    "BankAccountNumber" TEXT,
    FOREIGN KEY ("CompanyID") REFERENCES "Companies" ("CompanyID") ON DELETE CASCADE
);

CREATE TABLE "Customers" (
    "CompanyID" INTEGER NOT NULL PRIMARY KEY,
    "Discount" REAL NOT NULL,
    FOREIGN KEY ("CompanyID") REFERENCES "Companies" ("CompanyID") ON DELETE CASCADE
);
```
#### **Zalety TPT:**

- Dane są lepiej rozdzielone — brak zbędnych NULL w tabelach.
    
- Schemat bazy wiernie odzwierciedla model klas w kodzie.
    
- Lepsza walidacja schematu na poziomie bazy (łatwiej wymusić NOT NULL tam gdzie trzeba).
#### **Wady TPT:**

- Mniej wydajne zapytania (konieczność wykonywania JOIN-ów przy odczycie).
    
- Bardziej skomplikowane migracje i zmiany schematu.
    
- Może powodować więcej komplikacji przy dużych i złożonych hierarchiach dziedziczenia.