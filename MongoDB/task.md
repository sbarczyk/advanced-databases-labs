# Dokumentowe bazy danych – MongoDB

Ćwiczenie/zadanie


---

**Imiona i nazwiska autorów:**
Szymon Barczyk, Jan Dyląg

--- 

Odtwórz z backupu bazę north0

```
mongorestore --nsInclude='north0.*' ./dump/
```

```
use north0
```


# Zadanie 1 - operacje wyszukiwania danych,  przetwarzanie dokumentów

# a)

stwórz kolekcję  `OrdersInfo`  zawierającą następujące dane o zamówieniach
- pojedynczy dokument opisuje jedno zamówienie

```js
[  
  {  
    "_id": ...
    
    OrderID": ... numer zamówienia
    
    "Customer": {  ... podstawowe informacje o kliencie skladającym  
      "CustomerID": ... identyfikator klienta
      "CompanyName": ... nazwa klienta
      "City": ... miasto 
      "Country": ... kraj 
    },  
    
    "Employee": {  ... podstawowe informacje o pracowniku obsługującym zamówienie
      "EmployeeID": ... idntyfikator pracownika 
      "FirstName": ... imie   
      "LastName": ... nazwisko
      "Title": ... stanowisko  
     
    },  
    
    "Dates": {
       "OrderDate": ... data złożenia zamówienia
       "RequiredDate": data wymaganej realizacji
    }

    "Orderdetails": [  ... pozycje/szczegóły zamówienia - tablica takich pozycji 
      {  
        "UnitPrice": ... cena
        "Quantity": ... liczba sprzedanych jednostek towaru
        "Discount": ... zniżka  
        "Value": ... wartośc pozycji zamówienia
        "product": { ... podstawowe informacje o produkcie 
          "ProductID": ... identyfikator produktu  
          "ProductName": ... nazwa produktu 
          "QuantityPerUnit": ... opis/opakowannie
          "CategoryID": ... identyfikator kategorii do której należy produkt
          "CategoryName" ... nazwę tej kategorii
        },  
      },  
      ...   
    ],  

    "Freight": ... opłata za przesyłkę
    "OrderTotal"  ... sumaryczna wartosc sprzedanych produktów

    "Shipment" : {  ... informacja o wysyłce
        "Shipper": { ... podstawowe inf o przewoźniku 
           "ShipperID":  
            "CompanyName":
        }  
        ... inf o odbiorcy przesyłki
        "ShipName": ...
        "ShipAddress": ...
        "ShipCity": ... 
        "ShipCountry": ...
    } 
  } 
]  
```

## Rozwiązanie:
```js
db.orders.aggregate([  
  // 1. Dołącz klienta
  {
    $lookup: {
      from: "customers",
      localField: "CustomerID",
      foreignField: "CustomerID",
      as: "cust"
    }
  },
  { $unwind: "$cust" },

  // 2. Dołącz pracownika
  {
    $lookup: {
      from: "employees",
      localField: "EmployeeID",
      foreignField: "EmployeeID",
      as: "emp"
    }
  },
  { $unwind: "$emp" },

  // 3. Dołącz przewoźnika
  {
    $lookup: {
      from: "shippers",
      localField: "ShipVia",
      foreignField: "ShipperID",
      as: "shipper"
    }
  },
  { $unwind: "$shipper" },

  // 4. Dołącz pozycje zamówienia
  {
    $lookup: {
      from: "orderdetails",
      localField: "OrderID",
      foreignField: "OrderID",
      as: "details"
    }
  },
  { $unwind: "$details" },

  // 5. Dołącz produkt
  {
    $lookup: {
      from: "products",
      localField: "details.ProductID",
      foreignField: "ProductID",
      as: "prod"
    }
  },
  { $unwind: "$prod" },

  // 6. Dołącz kategorię produktu
  {
    $lookup: {
      from: "categories",
      localField: "prod.CategoryID",
      foreignField: "CategoryID",
      as: "cat"
    }
  },
  { $unwind: "$cat" },

  // 7. Dodaj Value pozycji i przygotuj Orderdetail
  {
    $addFields: {
      Orderdetail: {
        UnitPrice: "$details.UnitPrice",
        Quantity: "$details.Quantity",
        Discount: "$details.Discount",
        Value: {
          $multiply: [
            "$details.UnitPrice",
            "$details.Quantity",
            { $subtract: [1, "$details.Discount"] }
          ]
        },
        product: {
          ProductID: "$prod.ProductID",
          ProductName: "$prod.ProductName",
          QuantityPerUnit: "$prod.QuantityPerUnit",
          CategoryID: "$cat.CategoryID",
          CategoryName: "$cat.CategoryName"
        }
      },
      detailValue: {
        $multiply: [
          "$details.UnitPrice",
          "$details.Quantity",
          { $subtract: [1, "$details.Discount"] }
        ]
      }
    }
  },

  // 8. Grupowanie wszystkiego po zamówieniu
  {
    $group: {
      _id: "$OrderID",
      OrderID: { $first: "$OrderID" },
      Customer: { $first: {
        CustomerID: "$cust.CustomerID",
        CompanyName: "$cust.CompanyName",
        City: "$cust.City",
        Country: "$cust.Country"
      }},
      Employee: { $first: {
        EmployeeID: "$emp.EmployeeID",
        FirstName: "$emp.FirstName",
        LastName: "$emp.LastName",
        Title: "$emp.Title"
      }},
      Dates: { $first: {
        OrderDate: "$OrderDate",
        RequiredDate: "$RequiredDate"
      }},
      Freight: { $first: "$Freight" },
      Shipment: { $first: {
        Shipper: {
          ShipperID: "$shipper.ShipperID",
          CompanyName: "$shipper.CompanyName"
        },
        ShipName: "$ShipName",
        ShipAddress: "$ShipAddress",
        ShipCity: "$ShipCity",
        ShipCountry: "$ShipCountry"
      }},
      Orderdetails: { $push: "$Orderdetail" }, // Zbieramy wszystkie produkty
      OrderTotal: { $sum: "$detailValue" } // Suma wartości zamówienia
    }
  },

  // 9. Zapisywanie do kolekcji OrdersInfo
  {
    $merge: {
      into: "OrdersInfo",
      whenMatched: "replace",
      whenNotMatched: "insert"
    }
  }
], { allowDiskUse: true });
```
# b)

stwórz kolekcję  `CustomerInfo`  zawierającą następujące dane kazdym klencie
- pojedynczy dokument opisuje jednego klienta

```js
[  
  {  
    "_id": ...
    
    "CustomerID": ... identyfikator klienta
    "CompanyName": ... nazwa klienta
    "City": ... miasto 
    "Country": ... kraj 

	"Orders": [ ... tablica zamówień klienta o strukturze takiej jak w punkcie a) (oczywiście bez informacji o kliencie)
	  
	]

		  
]  
```

## Rozwiązanie:
```js
db.OrdersInfo.aggregate([

  // 1) Wyciągnij dane klienta i kształt jednego zamówienia bez pola Customer
  {
    $project: {
      CustomerID:  "$Customer.CustomerID",
      CompanyName: "$Customer.CompanyName",
      City:        "$Customer.City",
      Country:     "$Customer.Country",
      Order: {
        OrderID:     "$OrderID",
        Employee:    "$Employee",
        Dates:       "$Dates",
        Orderdetails:"$Orderdetails",
        Freight:     "$Freight",
        OrderTotal:  "$OrderTotal",
        Shipment:    "$Shipment"
      }
    }
  },

  // 2) Grupuj po kliencie, zbierając wszystkie jego zamówienia
  {
    $group: {
      _id:        "$CustomerID",
      CustomerID: { $first: "$CustomerID" },
      CompanyName:{ $first: "$CompanyName" },
      City:       { $first: "$City" },
      Country:    { $first: "$Country" },
      Orders:     { $push: "$Order" }
    }
  },

  // 3) Zapisz do nowej kolekcji
  {
    $merge: {
      into: "CustomerInfo",
      whenMatched:   "replace",
      whenNotMatched:"insert"
    }
  }

], { allowDiskUse: true });
```
# c) 

Napisz polecenie/zapytanie: Dla każdego klienta pokaż wartość zakupionych przez niego produktów z kategorii 'Confections'  w 1997r
- Spróbuj napisać to zapytanie wykorzystując
	- oryginalne kolekcje (`customers, orders, orderdertails, products, categories`)
	- kolekcję `OrderInfo`
	- kolekcję `CustomerInfo`

- porównaj zapytania/polecenia/wyniki

```js
[  
  {  
    "_id": 
    
    "CustomerID": ... identyfikator klienta
    "CompanyName": ... nazwa klienta
	"ConfectionsSale97": ... wartość zakupionych przez niego produktów z kategorii 'Confections'  w 1997r

  }		  
]  
```

## Rozwiązanie:
### Oryginalne kolekcje
```js
db.customers.aggregate([

  // 1) Dołącz zamówienia klienta z 1997
  { 
    $lookup: {
      from: "orders",
      let: { cid: "$CustomerID" },
      pipeline: [
        { $match: {
            $expr: {
              $and: [
                { $eq: ["$CustomerID", "$$cid"] },
                { $eq: [{ $year: "$OrderDate" }, 1997] }
              ]
            }
        }}
      ],
      as: "ord97"
    }
  },
  { $unwind: "$ord97" },

  // 2) Dołącz szczegóły zamówień
  { 
    $lookup: {
      from: "orderdetails",
      localField: "ord97.OrderID",
      foreignField: "OrderID",
      as: "det"
    }
  },
  { $unwind: "$det" },

  // 3) Dołącz produkty
  { 
    $lookup: {
      from: "products",
      localField: "det.ProductID",
      foreignField: "ProductID",
      as: "prod"
    }
  },
  { $unwind: "$prod" },

  // 4) Dołącz kategorię
  { 
    $lookup: {
      from: "categories",
      localField: "prod.CategoryID",
      foreignField: "CategoryID",
      as: "cat"
    }
  },
  { $unwind: "$cat" },

  // 5) Filtrowanie tylko 'Confections'
  { 
    $match: { "cat.CategoryName": "Confections" }
  },

  // 6) Dodaj pole value
  { 
    $addFields: {
      value: {
        $multiply: [
          "$det.UnitPrice",
          "$det.Quantity",
          { $subtract: [1, "$det.Discount"] }
        ]
      }
    }
  },

  // 7) Grupowanie
  { 
    $group: {
      _id: "$_id",
      CustomerID: { $first: "$CustomerID" },
      CompanyName: { $first: "$CompanyName" },
      ConfectionsSale97: { $sum: "$value" }
    }
  }
], { allowDiskUse: true });
```

### Kolekcja OrdersInfo:
```js
db.OrdersInfo.aggregate([

  // 1) Rozwiń wszystkie pozycje
  { $unwind: "$Orderdetails" },

  // 2) Filtrowanie po roku 1997 i kategorii "Confections"
  { $match: {
      $expr: {
        $and: [
          { $eq: [{ $year: "$Dates.OrderDate" }, 1997] },
          { $eq: ["$Orderdetails.product.CategoryName", "Confections"] }
        ]
      }
  }},

  // 3) Grupowanie po CustomerID
  { $group: {
      _id: "$Customer.CustomerID",
      CompanyName: { $first: "$Customer.CompanyName" },
      TotalValue: { $sum: "$Orderdetails.Value" }
  }},

  // 4) Projektuj dokument zgodny ze schematem
  { $project: {
      _id: { $literal: ObjectId() },
      CustomerID: "$_id",
      CompanyName: 1,
      ConfectionsSale97: "$TotalValue"
  }}
]);
```

```js
db.customerinfo.aggregate([
  
  { $unwind: "$Orders" },
  { $unwind: "$Orders.Orderdetails" },

  { 
    $match: {
      $expr: {
        $and: [
          { $eq: [ { $year: "$Orders.Dates.OrderDate" }, 1997 ] },
          { $eq: [ "$Orders.Orderdetails.product.CategoryName", "Confections" ] }
        ]
      }
    }
  },

  { 
    $group: {
      _id: "$CustomerID",
      CompanyName: { $first: "$CompanyName" },
      ConfectionsSale97: { $sum: "$Orders.Orderdetails.Value" }
    }
  },

  { 
    $project: {
      CustomerID: "$_id",
      CompanyName: 1,
      ConfectionsSale97: 1
    }
  }

], { allowDiskUse: true });
```

### Porównanie:
| Kryterium                 | Surowe kolekcje                         | `OrdersInfo`                                      | `CustomersOrders`                                         |
| ------------------------- | --------------------------------------- | ------------------------------------------------- | --------------------------------------------------------- |
| **Liczba etapów**         | ~14 (wielokrotne `$lookup` + `$unwind`) | ~6 (1-2 `$unwind`, `$match`, `$group`)            | ~5 (2 `$unwind`, `$match`, `$group`)                      |
| **Złożoność kodu**        | Dużo „ręcznych” joinów i transformacji  | Prostszym: wiele danych przygotowanych wcześniej. | Najprostszy: operujemy na gotowej strukturze „per klient” |
| **Wydajność**             | Najwolniejsze                           | Szybkie (mniej danych, brak joinów w runtime)     | Najszybsze (niewiele dokumentów + gotowe dane)            |
| **Zależności**            | Brak                                    | Wymaga wcześniejszego utworzenia `OrdersInfo`     | Wymaga wcześniejszego utworzenia `CustomerInfo`           |
| **Elastyczność**          | Największa                              | Średnia – dane już wstępnie przetworzone          | Najmniejsza – struktura dostosowana do konkretnego celu   |
| **Czytelność/utrzymanie** | Najtrudniejsze (dużo etapów)            | Dobre                                             | Bardzo dobre (krótki, czytelny pipeline)                  |
| **Wyniki**                | Identyczne we wszystkich podejściach    | Identyczne we wszystkich podejściach              | Identyczne we wszystkich podejściach                      |
# d)

Napisz polecenie/zapytanie:  Dla każdego klienta podaje wartość sprzedaży z podziałem na lata i miesiące
Spróbuj napisać to zapytanie wykorzystując
	- oryginalne kolekcje (`customers, orders, orderdertails, products, categories`)
	- kolekcję `OrderInfo`
	- kolekcję `CustomerInfo`

- porównaj zapytania/polecenia/wyniki

```js
[  
  {  
    "_id": 
    
    "CustomerID": ... identyfikator klienta
    "CompanyName": ... nazwa klienta

	"Sale": [ ... tablica zawierająca inf o sprzedazy
	    {
            "Year":  ....
            "Month": ....
            "Total": ...	    
	    }
	    ...
	]
  }		  
]  
```

## Rozwiązanie:
### Oryginalne kolekcje:
```js
db.customers.aggregate([

  // 1) Dołącz zamówienia
  { $lookup: {
      from: "orders",
      let: { cid: "$CustomerID" },
      pipeline: [
        { $match: {
            $expr: { $eq: ["$CustomerID","$$cid"] }
        }}
      ],
      as: "ords"
  }},
  { $unwind: "$ords" },

  // 2) Dołącz szczegóły
  { $lookup: {
      from: "orderdetails",
      localField:  "ords.OrderID",
      foreignField:"OrderID",
      as:          "dets"
  }},
  { $unwind: "$dets" },

  // 3) Oblicz wartość pozycji i rok/miesiąc
  { $addFields: {
      Value: {
        $multiply: [
          "$dets.UnitPrice",
          "$dets.Quantity",
          { $subtract: [1, "$dets.Discount"] }
        ]
      },
      Year:  { $year: "$ords.OrderDate" },
      Month: { $month: "$ords.OrderDate" }
  }},

  // 4) Grupuj po kliencie, roku i miesiącu
  { $group: {
      _id: {
        CustomerID: "$CustomerID",
        Year:       "$Year",
        Month:      "$Month"
      },
      CompanyName: { $first: "$CompanyName" },
      Total:       { $sum: "$Value" }
  }},

  // 5) Zbuduj tablicę Sale per klient
  { $group: {
      _id:        "$_id.CustomerID",
      CompanyName:{ $first: "$CompanyName" },
      Sale: {
        $push: {
          Year:  "$_id.Year",
          Month: "$_id.Month",
          Total: "$Total"
        }
      }
  }},

  // 6) Projektuj ostateczny kształt
  { $project: {
      CustomerID: "$_id",
      CompanyName: 1,
      Sale:        1
  }}

]);
```

### Kolekcja OrdersInfo:
```js
db.OrdersInfo.aggregate([

  // 1) Rozwiń wszystkie pozycje
  { $unwind: "$Orderdetails" },

  // 2) Dodaj rok i miesiąc
  { $addFields: {
      Year:  { $year: "$Dates.OrderDate" },
      Month: { $month: "$Dates.OrderDate" }
  }},

  // 3) Grupuj po kliencie, roku i miesiącu
  { $group: {
      _id: {
        CustomerID: "$Customer.CustomerID",
        Year:       "$Year",
        Month:      "$Month"
      },
      CompanyName: { $first: "$Customer.CompanyName" },
      Total:       { $sum: "$Orderdetails.Value" }
  }},

  // 4) Zbuduj tablicę Sale per klient
  { $group: {
      _id:        "$_id.CustomerID",
      CompanyName:{ $first: "$CompanyName" },
      Sale: {
        $push: {
          Year:  "$_id.Year",
          Month: "$_id.Month",
          Total: "$Total"
        }
      }
  }},

  // 5) Finalny projekt
  { $project: {
      CustomerID: "$_id",
      CompanyName: 1,
      Sale:        1
  }}

]);
```

### CustomersInfo
```js
db.CustomerInfo.aggregate([

  // 1) Rozwiń zamówienia i ich pozycje
  { $unwind: "$Orders" },
  { $unwind: "$Orders.Orderdetails" },

  // 2) Dodaj rok i miesiąc
  { $addFields: {
      Year:  { $year: "$Orders.Dates.OrderDate" },
      Month: { $month: "$Orders.Dates.OrderDate" }
  }},

  // 3) Grupuj po kliencie, roku i miesiącu
  { $group: {
      _id: {
        CustomerID: "$CustomerID",
        Year:       "$Year",
        Month:      "$Month"
      },
      CompanyName: { $first: "$CompanyName" },
      Total:       { $sum: "$Orders.Orderdetails.Value" }
  }},

  // 4) Zbuduj tablicę Sale per klient
  { $group: {
      _id:        "$_id.CustomerID",
      CompanyName:{ $first: "$CompanyName" },
      Sale: {
        $push: {
          Year:  "$_id.Year",
          Month: "$_id.Month",
          Total: "$Total"
        }
      }
  }},

  // 5) Finalny projekt
  { $project: {
      CustomerID: "$_id",
      CompanyName: 1,
      Sale:        1
  }}

]);
```

| Kryterium               | Surowe kolekcje                                                | OrdersInfo                                                | CustomerInfo                                    |
| ----------------------- | -------------------------------------------------------------- | --------------------------------------------------------- | ----------------------------------------------- |
| Etapy przetwarzania     | Wiele etapów, w tym `$lookup` i `$unwind`                      | Mniej etapów, bez dodatkowych `$lookup`                   | Mało etapów, bez `$lookup`                      |
| Joiny / rozbijanie      | Wiele `$lookup` (orders, orderdetails, products, categories)   | Brak dodatkowych `$lookup` (gotowe dane)                  | Brak `$lookup`, struktura "per klient"          |
| Obliczenia daty         | W `$addFields` po połączeniu zamówień i pozycji                | W `$addFields` po `$unwind`                               | W `$addFields` po `$unwind`                     |
| Grupowanie              | Dwa poziomy grupowania: po roku/miesiącu, potem po kliencie    | Dwa poziomy grupowania                                    | Dwa poziomy grupowania                          |
| Wydajność               | Najwolniejsze (dużo operacji i joinów)                         | Szybsze (wstępnie przetworzone dane)                      | Najszybsze (mało dokumentów)                    |
| Zależności              | Brak (działa bezpośrednio na bazowych kolekcjach)              | Wymaga wcześniejszego utworzenia `OrdersInfo`             | Wymaga wcześniejszego utworzenia `CustomerInfo` |
| Elastyczność            | Największa (możliwość dowolnych modyfikacji filtrów, obliczeń) | Średnia (ograniczenia wynikające z danych w `OrdersInfo`) | Najmniejsza (struktura ściśle pod klienta)      |
| Czytelność / utrzymanie | Średnia (dłuższy, bardziej złożony kod)                        | Dobra (krótki i logiczny kod)                             | Bardzo dobra (najkrótszy i najprostszy kod)     |
| Wyniki                  | Identyczne (tablica `Sale` z Year, Month, Total)               | Identyczne                                                | Identyczne                                      |
# e)

Załóżmy że pojawia się nowe zamówienie dla klienta 'ALFKI',  zawierające dwa produkty 'Chai' oraz "Ikura"
- pozostałe pola w zamówieniu (ceny, liczby sztuk prod, inf o przewoźniku itp. możesz uzupełnić wg własnego uznania)
Napisz polecenie które dodaje takie zamówienie do bazy
- aktualizując oryginalne kolekcje `orders`, `orderdetails`
- aktualizując kolekcję `OrderInfo`
- aktualizując kolekcję `CustomerInfo`

Napisz polecenie 
- aktualizując oryginalną kolekcję orderdetails`
- aktualizując kolekcję `OrderInfo`
- aktualizując kolekcję `CustomerInfo`

## Dodanie nowego zamówienia:
```js
db.orders.insertOne({
  OrderID: 12900,
  CustomerID: "ALFKI",
  EmployeeID: 1,
  OrderDate: ISODate("2025-04-23T10:00:00Z"),
  RequiredDate: ISODate("2025-05-23T00:00:00Z"),
  ShippedDate: null,
  ShipVia: 1,
  Freight: 10.00,
  ShipName: "Alfred's Futterkiste",
  ShipAddress: "Obere Str. 57",
  ShipCity: "Berlin",
  ShipRegion: null,
  ShipPostalCode: "12209",
  ShipCountry: "Germany"
});

db.orderdetails.insertMany([
  {
    OrderID: 12900,
    ProductID: 1,       // Chai
    UnitPrice: 18.00,
    Quantity: 5,
    Discount: 0
  },
  {
    OrderID: 12900,
    ProductID: 10,      // Ikura
    UnitPrice: 31.00,
    Quantity: 3,
    Discount: 0.10      // 10%
  }
]);
```

## Aktualizacja kolekcji OrdersInfo
```js
db.orders.aggregate([

  { $match: { OrderID: 12900 } },

  { $lookup: { from: "customers", localField: "CustomerID", foreignField: "CustomerID", as: "cust" } },
  { $unwind: "$cust" },

  { $lookup: { from: "employees", localField: "EmployeeID", foreignField: "EmployeeID", as: "emp" } },
  { $unwind: "$emp" },

  { $lookup: { from: "shippers", localField: "ShipVia", foreignField: "ShipperID", as: "shipper" } },
  { $unwind: "$shipper" },

  { $lookup: {
      from: "orderdetails",
      let: { oid: "$OrderID" },
      pipeline: [
        { $match: { $expr: { $eq: ["$OrderID", "$$oid"] } } },
        { $lookup: { from: "products", localField: "ProductID", foreignField: "ProductID", as: "prod" } },
        { $unwind: "$prod" },
        { $lookup: { from: "categories", localField: "prod.CategoryID", foreignField: "CategoryID", as: "cat" } },
        { $unwind: "$cat" },
        { $project: {
            UnitPrice: 1,
            Quantity: 1,
            Discount: 1,
            Value: { $multiply: ["$UnitPrice", "$Quantity", { $subtract: [1, "$Discount"] }] },
            product: {
              ProductID: "$prod.ProductID",
              ProductName: "$prod.ProductName",
              QuantityPerUnit: "$prod.QuantityPerUnit",
              CategoryID: "$cat.CategoryID",
              CategoryName: "$cat.CategoryName"
            }
        }}
      ],
      as: "Orderdetails"
  }},

  { $project: {
      _id: "$OrderID",
      OrderID: 1,
      Customer: {
        CustomerID: "$cust.CustomerID",
        CompanyName: "$cust.CompanyName",
        City: "$cust.City",
        Country: "$cust.Country"
      },
      Employee: {
        EmployeeID: "$emp.EmployeeID",
        FirstName: "$emp.FirstName",
        LastName: "$emp.LastName",
        Title: "$emp.Title"
      },
      Dates: {
        OrderDate: "$OrderDate",
        RequiredDate: "$RequiredDate"
      },
      Orderdetails: 1,
      Freight: 1,
      OrderTotal: { $sum: "$Orderdetails.Value" },
      Shipment: {
        Shipper: {
          ShipperID: "$shipper.ShipperID",
          CompanyName: "$shipper.CompanyName"
        },
        ShipName: "$ShipName",
        ShipAddress: "$ShipAddress",
        ShipCity: "$ShipCity",
        ShipCountry: "$ShipCountry"
      }
  }},

  { $merge: { into: "OrdersInfo", whenMatched: "replace", whenNotMatched: "insert" } }

], { allowDiskUse: true });
```

## Aktualizacja kolekcji CustomerInfo:
```js
db.OrdersInfo.aggregate([

  // 1. Wybierz tylko nowe zamówienie
  { $match: { "Customer.CustomerID": "ALFKI", "OrderID": 12900 } },

  // 2. Przygotuj nowy Order
  { $project: {
      CustomerID: "$Customer.CustomerID",
      CompanyName: "$Customer.CompanyName",
      NewOrder: {
        OrderID: "$OrderID",
        Employee: "$Employee",
        Dates: "$Dates",
        Orderdetails: "$Orderdetails",
        Freight: "$Freight",
        OrderTotal: "$OrderTotal",
        Shipment: "$Shipment"
      }
  }},

  // 3. Merge do CustomerInfo — dopisz zamówienie do Orders
  { $merge: {
      into: "CustomerInfo",
      whenMatched: [
        {
          $set: {
            Orders: { $concatArrays: ["$Orders", ["$NewOrder"]] }
          }
        }
      ],
      whenNotMatched: "insert"
  }}

], { allowDiskUse: true });
```
# f)

Napisz polecenie które modyfikuje zamówienie dodane w pkt e)  zwiększając zniżkę  o 5% (dla każdej pozycji tego zamówienia) 

Napisz polecenie 
- aktualizując oryginalną kolekcję `orderdetails`
- aktualizując kolekcję `OrderInfo`
- aktualizując kolekcję `CustomerInfo`

## Aktualizacja orderdetails

```js
db.orderdetails.updateMany(
  { OrderID: 12900 },
  { $inc: { Discount: 0.05 } }
);
```

Wynik:
## Aktualizacja OrderInfo

```js
db.OrdersInfo.updateOne(
  { _id: 12900 },
  [
    {
      $set: {
        Orderdetails: {
          $map: {
            input: "$Orderdetails",
            as: "d",
            in: {
              UnitPrice: "$$d.UnitPrice",
              Quantity:  "$$d.Quantity",
              Discount:  { $add: ["$$d.Discount", 0.05] },
              Value: {
                $multiply: [
                  "$$d.UnitPrice",
                  "$$d.Quantity",
                  { $subtract: [1, { $add: ["$$d.Discount", 0.05] }] }
                ]
              },
              product:   "$$d.product"
            }
          }
        }
      }
    },
    {
      $set: {
        OrderTotal: { $sum: "$Orderdetails.Value" }
      }
    }
  ]
);
```

## Aktualizacja CustomerInfo
```js
db.CustomerInfo.updateOne(
  { CustomerID: "ALFKI", "Orders.OrderID": 12900 },
  [
    {
      $set: {
        Orders: {
          $map: {
            input: "$Orders",
            as: "order",
            in: {
              $cond: [
                { $eq: ["$$order.OrderID", 12900] },
                {
                  OrderID: "$$order.OrderID",
                  Employee: "$$order.Employee",
                  Dates: "$$order.Dates",
                  Freight: "$$order.Freight",
                  Shipment: "$$order.Shipment",
                  Orderdetails: {
                    $map: {
                      input: "$$order.Orderdetails",
                      as: "d",
                      in: {
                        UnitPrice: "$$d.UnitPrice",
                        Quantity: "$$d.Quantity",
                        Discount: { $add: ["$$d.Discount", 0.05] },
                        Value: {
                          $multiply: [
                            "$$d.UnitPrice",
                            "$$d.Quantity",
                            { $subtract: [1, { $add: ["$$d.Discount", 0.05] }] }
                          ]
                        },
                        product: "$$d.product"
                      }
                    }
                  },
                  OrderTotal: {
                    $sum: {
                      $map: {
                        input: "$$order.Orderdetails",
                        as: "d",
                        in: {
                          $multiply: [
                            "$$d.UnitPrice",
                            "$$d.Quantity",
                            { $subtract: [1, { $add: ["$$d.Discount", 0.05] }] }
                          ]
                        }
                      }
                    }
                  }
                },
                "$$order" // inne zamówienia zostają niezmienione
              ]
            }
          }
        }
      }
    }
  ]
);
```

UWAGA:
W raporcie należy zamieścić kod poleceń oraz uzyskany rezultat, np wynik  polecenia `db.kolekcka.fimd().limit(2)` lub jego fragment


# Zadanie 2 - modelowanie danych


Zaproponuj strukturę bazy danych dla wybranego/przykładowego zagadnienia/problemu

Należy wybrać jedno zagadnienie/problem (A lub B lub C)

Przykład A
- Wykładowcy, przedmioty, studenci, oceny
	- Wykładowcy prowadzą zajęcia z poszczególnych przedmiotów
	- Studenci uczęszczają na zajęcia
	- Wykładowcy wystawiają oceny studentom
	- Studenci oceniają zajęcia

Przykład B
- Firmy, wycieczki, osoby
	- Firmy organizują wycieczki
	- Osoby rezerwują miejsca/wykupują bilety
	- Osoby oceniają wycieczki

Przykład C
- Własny przykład o podobnym stopniu złożoności

a) Zaproponuj  różne warianty struktury bazy danych i dokumentów w poszczególnych kolekcjach oraz przeprowadzić dyskusję każdego wariantu (wskazać wady i zalety każdego z wariantów)
- zdefiniuj schemat/reguły walidacji danych
- wykorzystaj referencje
- dokumenty zagnieżdżone
- tablice

b) Kolekcje należy wypełnić przykładowymi danymi

c) W kontekście zaprezentowania wad/zalet należy zaprezentować kilka przykładów/zapytań/operacji oraz dla których dedykowany jest dany wariant

W sprawozdaniu należy zamieścić przykładowe dokumenty w formacie JSON ( pkt a) i b)), oraz kod zapytań/operacji (pkt c)), wraz z odpowiednim komentarzem opisującym strukturę dokumentów oraz polecenia ilustrujące wykonanie przykładowych operacji na danych

Do sprawozdania należy kompletny zrzut wykonanych/przygotowanych baz danych (taki zrzut można wykonać np. za pomocą poleceń `mongoexport`, `mongdump` …) oraz plik z kodem operacji/zapytań w wersji źródłowej (np. plik .js, np. plik .md ), załącznik powinien mieć format zip

## Zadanie 2  - rozwiązanie

> Wyniki: 
> 
> przykłady, kod, zrzuty ekranów, komentarz ...


### Wariant 1: znormalizowany (reference‑heavy)

W tym wariancie struktura bazy danych **opiera się głównie na referencjach** (`ObjectId`) między dokumentami.  

Dane są przechowywane w osobnych kolekcjach (`users`, `exercises`, `events`, `workouts`, `measurements` itd.), a powiązania między nimi realizowane są poprzez identyfikatory (`userId`, `exerciseId`, `eventId`).

  

### Elementy główne:

- **Każdy użytkownik** jest przechowywany jako osobny dokument w kolekcji `users`.

- **Każde ćwiczenie** ma swój dokument w kolekcji `exercises`.

- **Każdy trening** (`workout`) odwołuje się do użytkownika (`userId`) i zawiera w sobie tablicę serii (`sets`), w której każda seria odwołuje się do ćwiczenia (`exerciseId`).

- **Każdy pomiar ciała** (`measurement`) przypisany jest do użytkownika przez `userId`.

- **Rejestracje na wydarzenia** są przechowywane w osobnej kolekcji `eventRegistrations`, wskazując `userId` oraz `eventId`.

  

---

  

### Elementy zagnieżdżone:

  

Pomimo stosowania podejścia reference-heavy, w strukturze danych znajdują się również **elementy zagnieżdżone** (embedded):

  

- W dokumencie `workout` znajduje się **tablica `sets`**, która zawiera informacje o poszczególnych seriach treningowych (np. liczba powtórzeń, ciężar, ID ćwiczenia).

  Dzięki temu wszystkie serie związane z jednym treningiem są przechowywane razem w jednym dokumencie, co ułatwia operacje na pojedynczych sesjach treningowych i przyspiesza odczyt.

  

Przykład fragmentu dokumentu `workouts`:

  

```json

{
  "_id": ObjectId("665100000000000000000020"),
  "userId": ObjectId("665100000000000000000001"),
  "date": ISODate("2024-05-06T17:00:00Z"),
  "notes": "Chest workout - PR achieved",
  "sets": [

    {
      "exerciseId": ObjectId("665100000000000000000010"),
      "setIndex": 1,
      "reps": 10,
      "weight": 80

    },

    {
      "exerciseId": ObjectId("665100000000000000000010"),
      "setIndex": 2,
      "reps": 8,
      "weight": 85

    }

  ]

}

```

Opis kolekcji i ich pól:

| Collection             | Fields                                                                                                                                                                                                                                                                           | Description                                                                              | Relations                                                     |
| ---------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------- | ------------------------------------------------------------- |
| **users**              | `_id` (ObjectId)<br>`username` (String)<br>`email` (String)<br>`fullName` (String)<br>`createdAt` (Date)                                                                                                                                                                         | Dane zarejestrowanych użytkowników                                                       | –                                                             |
| **exercises**          | `_id` (ObjectId)<br>`name` (String)<br>`category` (String)<br>`defaultUnit` (String)                                                                                                                                                                                             | Katalog wszystkich ćwiczeń                                                               | –                                                             |
| **workouts**           | `_id` (ObjectId)<br>`userId` (ObjectId)<br>`date` (Date)<br>`notes` (String, opcjonalnie)<br>`sets` (Array of Objects):<br>&nbsp;&nbsp;• `exerciseId` (ObjectId)<br>&nbsp;&nbsp;• `setIndex` (Int)<br>&nbsp;&nbsp;• `reps` (Int)<br>&nbsp;&nbsp;• `weight` (Number, opcjonalnie) | Zapis sesji treningowej użytkownika, z wszystkimi seriami osadzonymi w jednym dokumencie | `userId` → `users._id`<br>`sets.exerciseId` → `exercises._id` |
| **measurements**       | `_id` (ObjectId)<br>`userId` (ObjectId)<br>`date` (Date)<br>`weight` (Number)<br>`chest` (Number)<br>`biceps` (Number)<br>`…` (inne obwody)                                                                                                                                      | Pomiary sylwetki (waga+obwody)                                                           | `userId` → `users._id`                                        |
| **events**             | `_id` (ObjectId)<br>`title` (String)<br>`date` (Date)<br>`capacity` (Int)<br>`registeredCount` (Int)                                                                                                                                                                             | Wydarzenia limitowane (testy siły, treningi grupowe)                                     | –                                                             |
| **eventRegistrations** | `_id` (ObjectId)<br>`eventId` (ObjectId)<br>`userId` (ObjectId)<br>`registeredAt` (Date)                                                                                                                                                                                         | Rejestracje użytkowników na wydarzenia                                                   | `eventId` → `events._id`<br>`userId` → `users._id`            |
Utworzenie kolekcji wraz z podstawową walidacją danych:
```js
use GymTracker_ReferenceHeavy;  
  
db.createCollection("users", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["username", "email", "fullName", "createdAt"],  
      properties: {  
        username: { bsonType: "string", description: "Login użytkownika" },  
        email: { bsonType: "string", pattern: "^.+@.+\\..+$", description: "Email użytkownika" },  
        fullName: { bsonType: "string", description: "Pełne imię i nazwisko" },  
        createdAt: { bsonType: "date", description: "Data utworzenia konta" }  
      }  
    }  
  }  
});  
  
db.createCollection("exercises", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["name", "category", "defaultUnit"],  
      properties: {  
        name: { bsonType: "string", description: "Nazwa ćwiczenia" },  
        category: { bsonType: "string", description: "Kategoria ćwiczenia" },  
        defaultUnit: { bsonType: "string", description: "Domyślna jednostka (np. kg, min)" }  
      }  
    }  
  }  
});  
  
  
db.createCollection("workouts", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["userId", "date", "sets"],  
      properties: {  
        userId: { bsonType: "objectId", description: "Id użytkownika wykonującego trening" },  
        date: { bsonType: "date", description: "Data treningu" },  
        notes: { bsonType: "string", description: "Notatki do treningu (opcjonalne)" },  
        sets: {  
          bsonType: "array",  
          description: "Lista serii ćwiczeń",  
          items: {  
            bsonType: "object",  
            required: ["exerciseId", "setIndex", "reps"],  
            properties: {  
              exerciseId: { bsonType: "objectId", description: "Id ćwiczenia" },  
              setIndex: { bsonType: "int", description: "Numer serii" },  
              reps: { bsonType: "int", description: "Liczba powtórzeń" },  
              weight: { bsonType: ["double", "int"], description: "Ciężar (opcjonalny)" }  
            }  
          }  
        }  
      }  
    }  
  }  
});  
  
  
db.createCollection("measurements", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["userId", "date", "weight"],  
      properties: {  
        userId: { bsonType: "objectId", description: "Id użytkownika" },  
        date: { bsonType: "date", description: "Data pomiaru" },  
        weight: { bsonType: ["double", "int"], description: "Waga (kg)" },  
        chest: { bsonType: ["double", "int"], description: "Obwód klatki piersiowej (cm)" },  
        biceps: { bsonType: ["double", "int"], description: "Obwód bicepsa (cm)" }  
  
      }  
    }  
  }  
});  
  
db.createCollection("events", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["title", "date", "capacity", "registeredCount"],  
      properties: {  
        title: { bsonType: "string", description: "Tytuł wydarzenia" },  
        date: { bsonType: "date", description: "Data wydarzenia" },  
        capacity: { bsonType: "int", description: "Maksymalna liczba uczestników" },  
        registeredCount: { bsonType: "int", description: "Aktualna liczba zapisanych" }  
      }  
    }  
  }  
});  
  
  
db.createCollection("eventRegistrations", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["eventId", "userId", "registeredAt"],  
      properties: {  
        eventId: { bsonType: "objectId", description: "Id wydarzenia" },  
        userId: { bsonType: "objectId", description: "Id użytkownika" },  
        registeredAt: { bsonType: "date", description: "Data rejestracji na wydarzenie" }  
      }  
    }  
  }  
});
```

Wypełnienie przykładowymi danymi:
```js

use GymTracker_ReferenceHeavy;

// Wypełnienie kolekcji "users"
db.users.insertMany([
  {
    _id: ObjectId("665100000000000000000001"),
    username: "jdoe",
    email: "jdoe@example.com",
    fullName: "John Doe",
    createdAt: ISODate("2024-05-01T10:00:00Z")
  },
  {
    _id: ObjectId("665100000000000000000002"),
    username: "msmith",
    email: "msmith@example.com",
    fullName: "Mary Smith",
    createdAt: ISODate("2024-05-02T11:00:00Z")
  },
  {
    _id: ObjectId("665100000000000000000003"),
    username: "pparker",
    email: "pparker@example.com",
    fullName: "Peter Parker",
    createdAt: ISODate("2024-05-03T09:30:00Z")
  },
  {
    _id: ObjectId("665100000000000000000004"),
    username: "klane",
    email: "klane@example.com",
    fullName: "Kate Lane",
    createdAt: ISODate("2024-05-04T15:00:00Z")
  },
  {
    _id: ObjectId("665100000000000000000005"),
    username: "bwilliams",
    email: "bwilliams@example.com",
    fullName: "Bruce Williams",
    createdAt: ISODate("2024-05-05T13:45:00Z")
  }
]);

// Wypełnienie kolekcji "exercises"
db.exercises.insertMany([
  {
    _id: ObjectId("665100000000000000000010"),
    name: "Bench Press",
    category: "Chest",
    defaultUnit: "kg"
  },
  {
    _id: ObjectId("665100000000000000000011"),
    name: "Squat",
    category: "Legs",
    defaultUnit: "kg"
  },
  {
    _id: ObjectId("665100000000000000000012"),
    name: "Deadlift",
    category: "Back",
    defaultUnit: "kg"
  },
  {
    _id: ObjectId("665100000000000000000013"),
    name: "Pull Up",
    category: "Back",
    defaultUnit: "reps"
  },
  {
    _id: ObjectId("665100000000000000000014"),
    name: "Overhead Press",
    category: "Shoulders",
    defaultUnit: "kg"
  },
  {
    _id: ObjectId("665100000000000000000015"),
    name: "Running",
    category: "Cardio",
    defaultUnit: "minutes"
  }
]);

// Wypełnienie kolekcji "workouts"
db.workouts.insertMany([
  {
    _id: ObjectId("665100000000000000000020"),
    userId: ObjectId("665100000000000000000001"),
    date: ISODate("2024-05-06T17:00:00Z"),
    notes: "Chest workout - PR achieved",
    sets: [
      {
        exerciseId: ObjectId("665100000000000000000010"),
        setIndex: 1,
        reps: 10,
        weight: 80
      },
      {
        exerciseId: ObjectId("665100000000000000000010"),
        setIndex: 2,
        reps: 8,
        weight: 85
      }
    ]
  },
  {
    _id: ObjectId("665100000000000000000021"),
    userId: ObjectId("665100000000000000000002"),
    date: ISODate("2024-05-06T18:00:00Z"),
    notes: "Leg day",
    sets: [
      {
        exerciseId: ObjectId("665100000000000000000011"),
        setIndex: 1,
        reps: 12,
        weight: 90
      },
      {
        exerciseId: ObjectId("665100000000000000000011"),
        setIndex: 2,
        reps: 10,
        weight: 95
      }
    ]
  },
  {
    _id: ObjectId("665100000000000000000022"),
    userId: ObjectId("665100000000000000000003"),
    date: ISODate("2024-05-06T19:00:00Z"),
    notes: "",
    sets: [
      {
        exerciseId: ObjectId("665100000000000000000015"),
        setIndex: 1,
        reps: 1,
        weight: 30
      }
    ]
  }
]);

// Wypełnienie kolekcji "measurements"
db.measurements.insertMany([
  {
    _id: ObjectId("665100000000000000000030"),
    userId: ObjectId("665100000000000000000001"),
    date: ISODate("2024-05-05T07:30:00Z"),
    weight: 80,
    chest: 105,
    biceps: 38
  },
  {
    _id: ObjectId("665100000000000000000031"),
    userId: ObjectId("665100000000000000000002"),
    date: ISODate("2024-05-05T08:00:00Z"),
    weight: 65,
    chest: 90,
    biceps: 30
  }
]);

// Wypełnienie kolekcji "events"
db.events.insertMany([
  {
    _id: ObjectId("665100000000000000000040"),
    title: "Strength Test May 2024",
    date: ISODate("2024-05-20T09:00:00Z"),
    capacity: 20,
    registeredCount: 2
  },
  {
    _id: ObjectId("665100000000000000000041"),
    title: "Endurance Challenge",
    date: ISODate("2024-05-25T10:00:00Z"),
    capacity: 15,
    registeredCount: 1
  }
]);

// Wypełnienie kolekcji "eventRegistrations"
db.eventRegistrations.insertMany([
  {
    _id: ObjectId("665100000000000000000050"),
    eventId: ObjectId("665100000000000000000040"),
    userId: ObjectId("665100000000000000000001"),
    registeredAt: ISODate("2024-05-06T10:00:00Z")
  },
  {
    _id: ObjectId("665100000000000000000051"),
    eventId: ObjectId("665100000000000000000040"),
    userId: ObjectId("665100000000000000000002"),
    registeredAt: ISODate("2024-05-06T10:05:00Z")
  },
  {
    _id: ObjectId("665100000000000000000052"),
    eventId: ObjectId("665100000000000000000041"),
    userId: ObjectId("665100000000000000000003"),
    registeredAt: ISODate("2024-05-06T11:00:00Z")
  }
]);
```

# Podejście reference-heavy — analiza

## Zalety

### Normalizacja danych
Dane nie są duplikowane między kolekcjami. Zmiana w jednym miejscu od razu wpływa na wszystkie związane dokumenty.

> **Przykład:**  
> Zmiana nazwy ćwiczenia w kolekcji `exercises` (pole `name`) automatycznie będzie widoczna w `workouts`, bo są powiązane przez `exerciseId`.

```javascript
db.exercises.updateOne(
  { _id: ObjectId("665100000000000000000010") },
  { $set: { name: "Flat Bench Press" } }
);
```

---

### Spójność danych
Dane są zawsze aktualne i jednolite, zmniejszając ryzyko niespójności.

> **Przykład:**  
> Aktualizacja imienia i nazwiska w `users` nie wymaga zmiany w `eventRegistrations`.

```javascript
db.users.updateOne(
  { _id: ObjectId("665100000000000000000001") },
  { $set: { fullName: "Jonathan Doe" } }
);
```

---

### Oszczędność miejsca
Przechowujemy tylko identyfikatory zamiast pełnych danych.

> **Przykład:**  
> `workouts` zawierają tylko `userId` i `sets.exerciseId`, nie kopiują pełnych danych użytkownika lub ćwiczenia.

```javascript
db.workouts.find(
  { userId: ObjectId("665100000000000000000001") },
  { sets: 1, _id: 0 }
);
```

---

### Łatwiejsze aktualizacje i rozbudowa schematu
Dodawanie nowych pól w powiązanych dokumentach nie wymaga modyfikowania danych zależnych.

> **Przykład:**  
> Dodanie pola `difficulty` do `exercises` nie wpływa na `workouts`.

```javascript
db.exercises.updateMany({}, { $set: { difficulty: "Intermediate" } });
```

---

### Skalowalność danych
Reference-heavy pozwala przechowywać bardzo duże ilości danych w osobnych kolekcjach i skalować je niezależnie, dzięki odwołaniom przez identyfikatory (`ObjectId`).

> **Przykład:**  
> Dzięki temu `workouts` może zawierać miliony dokumentów opisujących sesje treningowe użytkowników, bez konieczności kopiowania danych użytkownika (`users`) lub ćwiczeń (`exercises`).

```javascript
db.workouts.find(
  { userId: ObjectId("665100000000000000000001") }
);
```

Treningi są przechowywane osobno i odwołują się do użytkownika tylko przez `userId`, co umożliwia efektywne przeszukiwanie i rozproszone przechowywanie danych.

---

## Wady

### Wydajność zapytań
Łączenie danych z różnych kolekcji ($lookup) zwiększa czas wykonania.

> **Przykład:**  
> Pobranie listy wszystkich serii ćwiczeń z nazwami ćwiczeń dla treningu.

```javascript
db.workouts.aggregate([
  { $match: { userId: ObjectId("665100000000000000000001") } },
  { $unwind: "$sets" },
  {
    $lookup: {
      from: "exercises",
      localField: "sets.exerciseId",
      foreignField: "_id",
      as: "exerciseDetails"
    }
  },
  { $unwind: "$exerciseDetails" },
  {
    $project: {
      setIndex: "$sets.setIndex",
      reps: "$sets.reps",
      exerciseName: "$exerciseDetails.name"
    }
  }
]);
```

---

### Złożoność kodu aplikacji
Wymaga więcej logiki do łączenia danych w aplikacji.

> **Przykład:**  
> Pobranie listy wydarzeń, na które użytkownik się zapisał.

```javascript
db.eventRegistrations.aggregate([
  { $match: { userId: ObjectId("665100000000000000000001") } },
  {
    $lookup: {
      from: "events",
      localField: "eventId",
      foreignField: "_id",
      as: "eventDetails"
    }
  },
  { $unwind: "$eventDetails" },
  {
    $project: {
      registeredAt: 1,
      eventTitle: "$eventDetails.title",
      eventDate: "$eventDetails.date"
    }
  }
]);
```

---

### Problemy migracyjne
Usunięcie dokumentu bez czyszczenia referencji powoduje martwe odwołania.

> **Przykład:**  
> Usunięcie ćwiczenia bez aktualizacji `workouts.sets.exerciseId`.

```javascript
// Przed usunięciem ćwiczenia sprawdź powiązania
db.workouts.find({ "sets.exerciseId": ObjectId("665100000000000000000010") });
```

---

### Utrudnione zapytania agregujące
Agregacja danych wymaga rozbudowanych operacji.

> **Przykład:**  
> Policz łączną liczbę powtórzeń danego ćwiczenia we wszystkich treningach.

```javascript
db.workouts.aggregate([
  { $unwind: "$sets" },
  { $match: { "sets.exerciseId": ObjectId("665100000000000000000010") } },
  {
    $group: {
      _id: "$sets.exerciseId",
      totalReps: { $sum: "$sets.reps" }
    }
  }
]);
```

---
# Przykładowe operacje na danych w systemie GymTracker

Poniżej przedstawiono wybrane operacje na danych, pokazujące najważniejsze aspekty struktury bazy danych opartej o podejście reference-heavy.

---

## Podstawowe operacje CRUD

### 1. Dodanie nowego użytkownika (proste)

```javascript
db.users.insertOne({
  username: "newuser",
  email: "newuser@example.com",
  fullName: "New User",
  createdAt: new Date()
});
```
**Komentarz:**  
Prosta operacja — `users` jest samodzielną kolekcją bez powiązań.

---

### 2. Dodanie nowego treningu z zestawem ćwiczeń (średnia trudność)

```javascript
db.workouts.insertOne({
  userId: ObjectId("665100000000000000000001"),
  date: new Date(),
  notes: "Strength session",
  sets: [
    {
      exerciseId: ObjectId("665100000000000000000010"), // Bench Press
      setIndex: 1,
      reps: 8,
      weight: 85
    },
    {
      exerciseId: ObjectId("665100000000000000000011"), // Squat
      setIndex: 2,
      reps: 10,
      weight: 120
    }
  ]
});
```
**Komentarz:**  
Wymaga znajomości `userId` oraz `exerciseId` — typowe dla reference-heavy.

---

### 3. Rejestracja użytkownika na wydarzenie (proste)

```javascript
db.eventRegistrations.insertOne({
  eventId: ObjectId("665100000000000000000040"),
  userId: ObjectId("665100000000000000000001"),
  registeredAt: new Date()
});
```
**Komentarz:**  
Prosta operacja — tylko zapisanie referencji (`eventId`, `userId`).

---

## Operacje analityczne i raportowe

### 4. Znalezienie maksymalnego ciężaru w wyciskaniu na ławce płaskiej dla danego użytkownika (progres siłowy)

```javascript
db.workouts.aggregate([
  { $match: { userId: ObjectId("665100000000000000000001") } }, // John Doe
  { $unwind: "$sets" },
  { 
    $match: { 
      "sets.exerciseId": ObjectId("665100000000000000000010"), // Bench Press
      "sets.weight": { $exists: true, $ne: null }
    }
  },
  {
    $group: {
      _id: "$sets.exerciseId",
      maxWeight: { $max: "$sets.weight" }
    }
  },
  {
    $lookup: {
      from: "exercises",
      localField: "_id",
      foreignField: "_id",
      as: "exerciseDetails"
    }
  },
  { $unwind: "$exerciseDetails" },
  {
    $project: {
      _id: 0,
      exerciseName: "$exerciseDetails.name",
      maxWeight: 1
    }
  }
]);
```
**Komentarz:**  
Wymaga agregacji — `$unwind` i `$group`. Przykład typowego wykorzystania reference-heavy do analizy progresu.

---

### 5. Śledzenie zmian obwodów (np. biceps) w czasie

```javascript
db.measurements.find(
  { userId: ObjectId("665100000000000000000001") },
  { date: 1, biceps: 1 }
).sort({ date: 1 });
```
**Komentarz:**  
Bardzo prosty odczyt — `measurements` powiązane bezpośrednio przez `userId`.

---

### 6. Najczęściej wykonywane ćwiczenia

```javascript
db.workouts.aggregate([
  { $unwind: "$sets" },
  {
    $group: {
      _id: "$sets.exerciseId",
      count: { $sum: 1 }
    }
  },
  { $sort: { count: -1 } },
  { $limit: 5 },
  {
    $lookup: {
      from: "exercises",
      localField: "_id",
      foreignField: "_id",
      as: "exerciseDetails"
    }
  },
  { $unwind: "$exerciseDetails" },
  {
    $project: {
      exerciseName: "$exerciseDetails.name",
      usageCount: "$count"
    }
  }
]);
```
**Komentarz:**  
Zaawansowane zapytanie — potrzebne `$unwind`, `$group`, `$lookup` i `$project`. Typowe dla reference-heavy przy łączeniu danych.

---

### 7. Średnia liczba serii na tydzień

```javascript
db.workouts.aggregate([
  { $match: { userId: ObjectId("665100000000000000000001") } },
  {
    $project: {
      week: { $isoWeek: "$date" },
      year: { $isoWeekYear: "$date" },
      setsCount: { $size: "$sets" }
    }
  },
  {
    $group: {
      _id: { year: "$year", week: "$week" },
      totalSets: { $sum: "$setsCount" }
    }
  },
  {
    $group: {
      _id: null,
      averageSetsPerWeek: { $avg: "$totalSets" }
    }
  }
]);
```

---
## Uwagi dodatkowe

Podejście **reference-heavy** sprawdza się najlepiej, gdy:

- potrzebujemy **spójnych, aktualnych danych** (np. system zarządzania siłownią),
- często zmienia się struktura obiektów (np. nowe typy ćwiczeń, nowe wydarzenia),
- liczba operacji odczytu na pojedynczych dokumentach jest większa niż liczba złożonych agregacji.

Przy bardzo dużych skalach (miliony rekordów) warto rozważyć **częściową denormalizację** danych dla kluczowych zapytań (np. przechowywanie nazwy ćwiczenia bezpośrednio w treningu dla szybszego odczytu).

---

### Czy podejście reference-heavy jest odpowiednie dla projektu GymTracker?

Podejście **reference-heavy** jest odpowiednie dla projektu **GymTracker**, ponieważ:

- Wymagana jest **spójność danych** (np. użytkownik zmienia dane kontaktowe, a wszystkie powiązane treningi, pomiary, zapisy na wydarzenia pozostają aktualne poprzez `userId`),
- **Struktura danych jest podatna na zmiany** (np. nowe ćwiczenia, nowe wydarzenia),
- System przeważnie wykonuje **odczyty pojedynczych dokumentów**,
- Ilość danych w kolekcjach typu `workouts` może rosnąć niezależnie od `users` i `exercises`, co pasuje do modelu referencji.

---
### Wariant 2: Dokumenty zagnieżdżone

W modelowaniu danych opartym na zagnieżdżonych dokumentach (ang. _embedded documents_) dane powiązane ze sobą logicznie są przechowywane razem w jednym dokumencie.

### Struktura bazy
- **users** (kolekcja główna użytkowników)
    
    - Przechowuje podstawowe informacje o użytkowniku oraz zagnieżdżone dokumenty:
        
        - **measurements** – lista pomiarów sylwetki (np. waga, biceps) z datami.
            
        - **workouts** – lista odbytych treningów, zawierająca ćwiczenia, serie, powtórzenia i ciężary.
            
        - **registeredEvents** – lista zapisów użytkownika na wydarzenia (referencje do kolekcji `events`).
            
- **exercises** (kolekcja pomocnicza)
    
    - Lista dostępnych ćwiczeń, np. martwy ciąg, przysiad, wyciskanie.
        
- **events** (kolekcja limitowanych wydarzeń)
    
    - Informacje o wydarzeniach takich jak testy siły lub wspólne treningi.
        
    - Wewnątrz dokumentu przechowywana jest lista zapisanych użytkowników.

Przykład fragmentu dokumentu 'users':

```json
{
  _id: ObjectId("665100000000000000000001"),
  username: "fitguy",
  email: "fitguy@example.com",
  passwordHash: "hashed_password_example",
  measurements: [
    {
      date: ISODate("2025-05-01T00:00:00Z"),
      weight: 80,
      biceps: 35,
      chest: 105,
      waist: 85
    },
    {
      date: ISODate("2025-05-10T00:00:00Z"),
      weight: 81,
      biceps: 36,
      chest: 106,
      waist: 84
    }
  ],
  workouts: [
    {
      date: ISODate("2025-05-10T18:00:00Z"),
      exercises: [
        {
          exerciseName: "Deadlift",
          sets: [
            { reps: 5, weight: 100 },
            { reps: 5, weight: 110 }
          ]
        },
        {
          exerciseName: "Bench Press",
          sets: [
            { reps: 8, weight: 80 },
            { reps: 6, weight: 85 }
          ]
        }
      ]
    }
  ],
  registeredEvents: [
    {
      eventId: ObjectId("665100000000000000000005"),
      registrationDate: ISODate("2025-05-08T12:00:00Z")
    }
  ]
}
```

| Collection | Fields | Type | Description |
|:-----------|:-------|:-----|:------------|
| **users** | _id | ObjectId | Unikalny identyfikator użytkownika |
|  | username | String | Nazwa użytkownika |
|  | email | String | Adres e-mail użytkownika |
|  | passwordHash | String | Zahaszowane hasło użytkownika |
|  | measurements | Array of Embedded Documents | Pomiary sylwetki użytkownika w czasie (data, waga, obwody) |
|  | workouts | Array of Embedded Documents | Lista treningów użytkownika (data, ćwiczenia, serie) |
|  | registeredEvents | Array of Embedded Documents | Zapisane wydarzenia sportowe (referencje do events) |

| Collection | Fields | Type | Description |
|:-----------|:-------|:-----|:------------|
| **exercises** | _id | ObjectId | Unikalny identyfikator ćwiczenia |
|  | name | String | Nazwa ćwiczenia |
|  | muscleGroup | String | Główna grupa mięśniowa |

| Collection | Fields | Type | Description |
|:-----------|:-------|:-----|:------------|
| **events** | _id | ObjectId | Unikalny identyfikator wydarzenia |
|  | name | String | Nazwa wydarzenia (np. test siły) |
|  | date | Date | Data i godzina wydarzenia |
|  | capacity | Number | Maksymalna liczba uczestników |
|  | registeredUsers | Array of Embedded Documents | Lista zapisanych użytkowników (referencje do users) |

Utworzenie kolekcji wraz z podstawową walidacją danych:

```js
use GymTracker_EmbeddedHeavy  
  
db.createCollection("users", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["username", "email", "passwordHash"],  
      properties: {  
        username: {  
          bsonType: "string"  
        },  
        email: {  
          bsonType: "string",  
          pattern: "^.+@.+\\..+$",  
          description: "Musi być poprawnym adresem e-mail"  
        },  
        passwordHash: {  
          bsonType: "string"  
        },  
        measurements: {  
          bsonType: "array"  
        },  
        workouts: {  
          bsonType: "array"  
        },  
        registeredEvents: {  
          bsonType: "array"  
        }  
      }  
    }  
  }  
})


db.createCollection("exercises", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["name", "muscleGroup"],  
      properties: {  
        name: {  
          bsonType: "string",  
          description: "Nazwa ćwiczenia"  
        },  
        muscleGroup: {  
          bsonType: "string",  
          description: "Główna grupa mięśniowa przypisana do ćwiczenia"  
        }  
      }  
    }  
  }  
})  
  
  
db.createCollection("events", {  
  validator: {  
    $jsonSchema: {  
      bsonType: "object",  
      required: ["name", "date", "capacity"],  
      properties: {  
        name: {  
          bsonType: "string",  
          description: "Nazwa wydarzenia np. test siły"  
        },  
        date: {  
          bsonType: "date",  
          description: "Data i godzina wydarzenia"  
        },  
        capacity: {  
          bsonType: "int",  
          minimum: 1,  
          description: "Maksymalna liczba uczestników wydarzenia"  
        },  
        registeredUsers: {  
          bsonType: "array",  
          description: "Tablica zapisanych użytkowników (embedded documents)"  
        }  
      }  
    }  
  }  
})
```

Wypełnianie przykładowymi danymi:

```js
db.exercises.insertMany([  
  {  
    name: "Deadlift",  
    muscleGroup: "Back"  
  },  
  {  
    name: "Bench Press",  
    muscleGroup: "Chest"  
  },  
  {  
    name: "Squat",  
    muscleGroup: "Legs"  
  }  
])

db.events.insertMany([  
  {  
  name: "Strength Test Day",  
  date: ISODate("2025-06-15T10:00:00Z"),  
  capacity: 20,  
  registeredUsers: []  
  },  
  {  
    name: "Strength Test Day",  
    date: ISODate("2025-06-15T10:00:00Z"),  
    capacity: 20,  
    registeredUsers: []  
  },  
  {  
    name: "Bench Press Challenge",  
    date: ISODate("2025-07-01T12:00:00Z"),  
    capacity: 15,  
    registeredUsers: []  
  },  
  {  
    name: "Deadlift Marathon",  
    date: ISODate("2025-07-20T09:00:00Z"),  
    capacity: 25,  
    registeredUsers: []  
  }  
])


db.users.insertMany([  
  {  
    username: "fitguy",  
    email: "fitguy@example.com",  
    passwordHash: "hashedpassword123",  
    measurements: [  
      {  
        date: ISODate("2025-05-01T00:00:00Z"),  
        weight: 80,  
        biceps: 35,  
        chest: 105,  
        waist: 85  
      }  
    ],  
    workouts: [  
      {  
        date: ISODate("2025-05-05T18:00:00Z"),  
        exercises: [  
          {  
            exerciseName: "Deadlift",  
            sets: [  
              { reps: 5, weight: 100 },  
              { reps: 5, weight: 110 }  
            ]  
          }  
        ]  
      }  
    ],  
    registeredEvents: []  
  },  
  {  
    username: "fitgirl",  
    email: "fitgirl@example.com",  
    passwordHash: "hashedpassword456",  
    measurements: [  
      {  
        date: ISODate("2025-05-02T00:00:00Z"),  
        weight: 60,  
        biceps: 28,  
        chest: 85,  
        waist: 65  
      }  
    ],  
    workouts: [  
      {  
        date: ISODate("2025-05-06T17:00:00Z"),  
        exercises: [  
          {  
            exerciseName: "Bench Press",  
            sets: [  
              { reps: 8, weight: 50 },  
              { reps: 6, weight: 55 }  
            ]  
          }  
        ]  
      }  
    ],  
    registeredEvents: []  
  },  
  {  
    username: "powerlifter",  
    email: "powerlifter@example.com",  
    passwordHash: "hashedpassword789",  
    measurements: [],  
    workouts: [],  
    registeredEvents: []  
  }  
])
```

# Podejście dokumentów zagnieżdżonych - analiza

## Zalety 

### 1. Szybszy odczyt danych — wszystko w jednym zapytaniu

Dzięki przechowywaniu powiązanych danych w jednym dokumencie nie musimy robić dodatkowych zapytań ani joinów.

**Przykład: Pobranie użytkownika razem z jego pomiarami i treningami**

```javascript
db.users.findOne({ username: "fitguy" })
```

### 2. Naturalna struktura danych — odzwierciedlenie rzeczywistości

Dane są przechowywane dokładnie tak, jak są używane w aplikacji:
- użytkownik ma listę swoich pomiarów,
- użytkownik ma listę swoich treningów.

**Przykład fragmentu dokumentu użytkownika:**

```javascript
{
  username: "fitgirl",
  measurements: [
    { date: ISODate("2025-05-02T00:00:00Z"), weight: 60, biceps: 28 }
  ],
  workouts: [
    {
      date: ISODate("2025-05-06T17:00:00Z"),
      exercises: [
        { exerciseName: "Bench Press", sets: [{ reps: 8, weight: 50 }] }
      ]
    }
  ]
}
```

### 3. Prostsze operacje na danych powiązanych

Dodawanie nowych pomiarów lub treningów do użytkownika jest bardzo proste dzięki użyciu operatora `$push`.

**Przykład: Dodanie nowego pomiaru sylwetki**

```javascript
db.users.updateOne(
  { username: "fitguy" },
  { $push: { measurements: { date: ISODate("2025-05-10T00:00:00Z"), weight: 81, biceps: 36, chest: 106, waist: 84 } } }
)
```

### 4. Mniej zapytań i mniejsze obciążenie bazy

- Wszystkie powiązane dane są ładowane razem, co zmniejsza liczbę zapytań do bazy danych.
- Aplikacja działa szybciej, bo nie trzeba pobierać danych z wielu kolekcji.

**Przykład: W jednym `findOne` odczytujemy całą aktywność użytkownika bez dodatkowych zapytań.**

```javascript
db.users.findOne({ username: "fitguy" })
```


## Wady 

### 1. Wzrost rozmiaru dokumentu

- Każdy nowy pomiar, trening lub wydarzenie powoduje, że dokument użytkownika rośnie.
- W MongoDB dokumenty mają limit rozmiaru do 16 MB — jeśli użytkownik będzie trenował przez lata i gromadził bardzo dużo danych, może dojść do problemów.

**Przykład: Dodawanie setek pomiarów i treningów**

```javascript
db.users.updateOne(
  { username: "powerlifter" },
  { $push: { measurements: { date: ISODate("2026-01-01T00:00:00Z"), weight: 90 } } }
)
// Jeśli dokument będzie bardzo duży, w pewnym momencie nie będzie można go już aktualizować
```

### 2. Problemy przy bardzo częstych aktualizacjach

- W MongoDB, gdy aktualizujesz bardzo duży dokument, czasami zachodzi konieczność jego przeniesienia na inną stronę dysku (tzw. document move), co może obniżyć wydajność.
- Embedded documents utrudniają optymalizację przy bardzo intensywnych aktualizacjach.

**Przykład: Częste dodawanie serii do workoutu**

```javascript
db.users.updateOne(
  { username: "fitguy", "workouts.date": ISODate("2025-05-05T18:00:00Z") },
  { $push: { "workouts.$.exercises": { exerciseName: "Pull-up", sets: [{ reps: 10, weight: 0 }] } } }
)
```

### 3. Trudności w dostępie do pojedynczych elementów

- Trudniej jest operować na pojedynczym pomiarze, treningu lub zapisanym wydarzeniu, jeśli wszystkie są zagnieżdżone w jednym wielkim dokumencie.
- Brak prostego dostępu jak w przypadku osobnych kolekcji.

**Przykład: Znalezienie tylko konkretnego pomiaru**

```javascript
db.users.findOne(
  { "measurements.date": ISODate("2025-05-01T00:00:00Z") },
  { "measurements.$": 1 }
)
```

### 4. Trudności w skalowaniu

- W przypadku bardzo dużych aplikacji (miliony użytkowników, setki milionów pomiarów) embedded documents mogą ograniczać możliwości skalowania poziomego (sharding w MongoDB).
- Większe dokumenty → większe przesyły sieciowe → wolniejsza praca rozproszonych systemów.


# Przykłady podstawowych operacji CRUD

### Kolekcja: users

#### CREATE (Dodanie nowego użytkownika)

```javascript
db.users.insertOne({
  username: "newuser",
  email: "newuser@example.com",
  passwordHash: "hashedpassword",
  measurements: [],
  workouts: [],
  registeredEvents: []
})
```
### CREATE — Dodanie nowego ćwiczenia do kolekcji exercises

```javascript
db.exercises.insertOne({
  name: "Overhead Press",
  muscleGroup: "Shoulders"
})
```

### CREATE — Dodanie nowego wydarzenia do kolekcji events

```javascript
db.events.insertOne({
  name: "Pull-up Challenge",
  date: ISODate("2025-08-01T12:00:00Z"),
  capacity: 30,
  registeredUsers: []
})
```


## Przykłady operacji analityczno-raportowych

### 1. Największy ciężar w ćwiczeniu "Deadlift" dla użytkownika

Znajdź największy podniesiony ciężar przez użytkownika w ćwiczeniu "Deadlift".

```javascript
db.users.aggregate([
  { $match: { username: "fitguy" } },
  { $unwind: "$workouts" },
  { $unwind: "$workouts.exercises" },
  { $match: { "workouts.exercises.exerciseName": "Deadlift" } },
  { $unwind: "$workouts.exercises.sets" },
  { $group: {
      _id: "$username",
      maxWeight: { $max: "$workouts.exercises.sets.weight" }
  }}
])
```

### 2. Średnia liczba serii wykonywanych na treningu przez użytkownika

Policz średnią liczbę serii na jeden workout.

```javascript
db.users.aggregate([
  { $match: { username: "fitguy" } },
  { $unwind: "$workouts" },
  { $unwind: "$workouts.exercises" },
  { $unwind: "$workouts.exercises.sets" },
  { $group: {
      _id: "$workouts.date",
      totalSets: { $sum: 1 }
  }},
  { $group: {
      _id: null,
      avgSetsPerWorkout: { $avg: "$totalSets" }
  }}
])
```

### 3. Zmiany wagi użytkownika w czasie

Zwróć wszystkie pomiary użytkownika posortowane po dacie (do wykresu zmian wagi).

```javascript
db.users.aggregate([
  { $match: { username: "fitguy" } },
  { $unwind: "$measurements" },
  { $project: {
      _id: 0,
      date: "$measurements.date",
      weight: "$measurements.weight"
  }},
  { $sort: { date: 1 } }
])
```

### 4. Najczęściej wykonywane ćwiczenia przez użytkownika

Zlicz, które ćwiczenia użytkownik wykonywał najczęściej.

```javascript
db.users.aggregate([
  { $match: { username: "fitguy" } },
  { $unwind: "$workouts" },
  { $unwind: "$workouts.exercises" },
  { $group: {
      _id: "$workouts.exercises.exerciseName",
      count: { $sum: 1 }
  }},
  { $sort: { count: -1 } }
])
```

## Uwagi dodatkowe
## Podsumowanie podejścia zagnieżdżonych dokumentów (Embedded Documents)

### Czym jest podejście zagnieżdżonych dokumentów?

Zagnieżdżone dokumenty (embedded documents) w MongoDB polegają na tym, że dane powiązane ze sobą logicznie są przechowywane razem w jednym dokumencie.  
Dzięki temu możemy odczytać całe powiązane dane za pomocą pojedynczego zapytania, bez konieczności wykonywania dodatkowych operacji łączenia (joinów).

---

### Kiedy warto stosować podejście zagnieżdżonych dokumentów?

1.Gdy dane są **silnie powiązane** i zawsze używane razem.  
2.Gdy aplikacja **często odczytuje całą strukturę danych naraz** (np. użytkownik i jego wszystkie treningi).  
3.Gdy dane **nie rosną bardzo dynamicznie** (nie będą setkami megabajtów na użytkownika).  
4.Gdy zależy nam na **szybszym odczycie** i **mniejszej liczbie zapytań** do bazy.  
5.Gdy model danych **naturalnie odzwierciedla rzeczywistość** (np. użytkownik → treningi → ćwiczenia).

---

### Kiedy NIE warto stosować zagnieżdżonych dokumentów?

1.Gdy **podobny zestaw danych** (np. pomiary, treningi) może osiągnąć **bardzo dużą liczbę rekordów** (miliony wpisów).  
2.Gdy trzeba **często aktualizować tylko fragment danych** (np. jeden konkretny pomiar lub jedno wydarzenie).  
3.Gdy dane **muszą być dostępne osobno** (np. lista wszystkich treningów wszystkich użytkowników).  
4.Gdy aplikacja wymaga **łatwego skalowania poziomego** (sharding na poziomie elementów kolekcji).

---

### Podsumowanie

Podejście zagnieżdżonych dokumentów mogłoby się sprawdzić w systemach takich jak **GymTracker**, gdzie użytkownik, jego treningi i pomiary stanowią naturalnie powiązaną grupę danych.  
Jednak przy dużej skali danych lub potrzebie elastycznego dostępu do pojedynczych wpisów warto rozważyć **podejście oparte na referencjach** i osobnych kolekcjach.

---
### Złoty środek: kompromis między embedded documents a referencjami

W praktycznych projektach, takich jak **GymTracker**, najskuteczniejszym podejściem jest zastosowanie **hybrydowego modelu danych**, który łączy zalety embedded documents oraz referencji.

### Główne założenia:

- **Często zmieniające się dane** (np. dane użytkowników, katalog ćwiczeń) powinny być przechowywane w osobnych kolekcjach i powiązane ze sobą za pomocą **referencji (`ObjectId`)**.
- **Rzadko zmieniające się lub intensywnie odczytywane dane** (np. nazwa ćwiczenia w zapisanym treningu) mogą być **wbudowywane (embedded)** bezpośrednio w dokumenty, aby uniknąć kosztownych operacji typu `$lookup`.

---

### Praktyczny przykład kompromisu:

W dokumentach `workouts` użytkownika można zapisywać:

- `exerciseId` (referencja do kolekcji exercises — pełna identyfikowalność),
- oraz dodatkowo `exerciseName` (kopię nazwy ćwiczenia w postaci stringa).

**Dzięki temu:**

- Odczyt treningów jest szybki, bez potrzeby wykonywania `$lookup` za każdym razem.
- W razie zmiany nazwy ćwiczenia, dane można **asynchronicznie zaktualizować** w tle.
- Zachowujemy spójność danych bez utraty wydajności odczytu.

---

### Zalety hybrydowego podejścia:

- Szybsze zapytania odczytujące dane użytkownika.
- Utrzymanie relacji i możliwości walidacji na poziomie referencji (`ObjectId`).
- Ograniczenie liczby operacji agregacji i łączenia danych.
- Elastyczność w zarządzaniu aktualizacjami danych historycznych.

---

### Podsumowanie

**Hybrydowe modelowanie danych** pozwala na optymalizację zarówno **wydajności odczytu**, jak i **utrzymania spójności danych**.  
W projekcie **GymTracker** planowane jest zastosowanie tego kompromisu, aby uzyskać najlepszą równowagę między prostotą systemu a jego skalowalnością i wydajnością. 
W ten sposób znaleźliśmy model, który najbardziej pasuje nam do tworzonego przez nas projektu, jego implementację wykonamy już w zadaniu projektowym.

---

Punktacja:

|         |     |
| ------- | --- |
| zadanie | pkt |
| 1       | 1   |
| 2       | 1   |
| razem   | 2   |



