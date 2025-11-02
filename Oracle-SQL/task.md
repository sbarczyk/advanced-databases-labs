

# Oracle PL/Sql

widoki, funkcje, procedury, triggery
ćwiczenie

---


Imiona i nazwiska autorów :
Szymon Barczyk, Jan Dyląg

---
<style>
  {
    font-size: 16pt;
  }
</style> 

<style scoped>
 li, p {
    font-size: 14pt;
  }
</style> 

<style scoped>
 pre {
    font-size: 10pt;
  }
</style> 

# Tabele

![](_img/ora-trip1-0.png)


- `Trip`  - wycieczki
	- `trip_id` - identyfikator, klucz główny
	- `trip_name` - nazwa wycieczki
	- `country` - nazwa kraju
	- `trip_date` - data
	- `max_no_places` -  maksymalna liczba miejsc na wycieczkę
- `Person` - osoby
	- `person_id` - identyfikator, klucz główny
	- `firstname` - imię
	- `lastname` - nazwisko


- `Reservation`  - rezerwacje/bilety na wycieczkę
	- `reservation_id` - identyfikator, klucz główny
	- `trip_id` - identyfikator wycieczki
	- `person_id` - identyfikator osoby
	- `status` - status rezerwacji
		- `N` – New - Nowa
		- `P` – Confirmed and Paid – Potwierdzona  i zapłacona
		- `C` – Canceled - Anulowana
- `Log` - dziennik zmian statusów rezerwacji 
	- `log_id` - identyfikator, klucz główny
	- `reservation_id` - identyfikator rezerwacji
	- `log_date` - data zmiany
	- `status` - status


```sql
create sequence s_person_seq  
   start with 1  
   increment by 1;

create table person  
(  
  person_id int not null
      constraint pk_person  
         primary key,
  firstname varchar(50),  
  lastname varchar(50)
)  

alter table person  
    modify person_id int default s_person_seq.nextval;
   
```


```sql
create sequence s_trip_seq  
   start with 1  
   increment by 1;

create table trip  
(  
  trip_id int  not null
     constraint pk_trip  
         primary key, 
  trip_name varchar(100),  
  country varchar(50),  
  trip_date date,  
  max_no_places int
);  

alter table trip 
    modify trip_id int default s_trip_seq.nextval;
```


```sql
create sequence s_reservation_seq  
   start with 1  
   increment by 1;

create table reservation  
(  
  reservation_id int not null
      constraint pk_reservation  
         primary key, 
  trip_id int,  
  person_id int,  
  status char(1)
);  

alter table reservation 
    modify reservation_id int default s_reservation_seq.nextval;


alter table reservation  
add constraint reservation_fk1 foreign key  
( person_id ) references person ( person_id ); 
  
alter table reservation  
add constraint reservation_fk2 foreign key  
( trip_id ) references trip ( trip_id );  
  
alter table reservation  
add constraint reservation_chk1 check  
(status in ('N','P','C'));

```


```sql
create sequence s_log_seq  
   start with 1  
   increment by 1;


create table log  
(  
    log_id int not null
         constraint pk_log  
         primary key,
    reservation_id int not null,  
    log_date date not null,  
    status char(1)
);  

alter table log 
    modify log_id int default s_log_seq.nextval;
  
alter table log  
add constraint log_chk1 check  
(status in ('N','P','C')) enable;
  
alter table log  
add constraint log_fk1 foreign key  
( reservation_id ) references reservation ( reservation_id );
```


---
# Dane


Należy wypełnić  tabele przykładowymi danymi 
- 4 wycieczki
- 10 osób
- 10  rezerwacji

Dane testowe powinny być różnorodne (wycieczki w przyszłości, wycieczki w przeszłości, rezerwacje o różnym statusie itp.) tak, żeby umożliwić testowanie napisanych procedur.

W razie potrzeby należy zmodyfikować dane tak żeby przetestować różne przypadki.


```sql
-- Dodanie 4 wycieczek (przeszłe i przyszłe daty, różne kraje)
insert into trip(trip_name, country, trip_date, max_no_places)
values ('Weekend w Gdańsku', 'Polska', to_date('2023-06-10', 'YYYY-MM-DD'), 4);

insert into trip(trip_name, country, trip_date, max_no_places)
values ('Rzym – klasyczne zwiedzanie', 'Włochy', to_date('2024-08-15', 'YYYY-MM-DD'), 5);

insert into trip(trip_name, country, trip_date, max_no_places)
values ('Praga z przewodnikiem', 'Czechy', to_date('2025-04-20', 'YYYY-MM-DD'), 3);

insert into trip(trip_name, country, trip_date, max_no_places)
values ('Tatry – szlakami Doliny Pięciu Stawów', 'Polska', to_date('2025-07-10', 'YYYY-MM-DD'), 6);

-- Dodanie 10 osób
insert into person(firstname, lastname) values ('Adam', 'Kozłowski');
insert into person(firstname, lastname) values ('Barbara', 'Witkowska');
insert into person(firstname, lastname) values ('Cezary', 'Lis');
insert into person(firstname, lastname) values ('Dagmara', 'Rogalska');
insert into person(firstname, lastname) values ('Emil', 'Błaszczyk');
insert into person(firstname, lastname) values ('Filip', 'Makowski');
insert into person(firstname, lastname) values ('Gabriela', 'Lewicka');
insert into person(firstname, lastname) values ('Hubert', 'Ziółko');
insert into person(firstname, lastname) values ('Iwona', 'Szczepańska');
insert into person(firstname, lastname) values ('Jakub', 'Krawczyk');

-- Dodanie 10 rezerwacji (różne statusy i liczba miejsc)
insert into reservation(trip_id, person_id, status, no_tickets) values (1, 1, 'P', 2);
insert into reservation(trip_id, person_id, status, no_tickets) values (1, 2, 'C', 1);
insert into reservation(trip_id, person_id, status, no_tickets) values (2, 3, 'N', 1);
insert into reservation(trip_id, person_id, status, no_tickets) values (2, 4, 'P', 2);
insert into reservation(trip_id, person_id, status, no_tickets) values (2, 5, 'C', 1);
insert into reservation(trip_id, person_id, status, no_tickets) values (3, 6, 'N', 1);
insert into reservation(trip_id, person_id, status, no_tickets) values (3, 7, 'P', 2);
insert into reservation(trip_id, person_id, status, no_tickets) values (4, 8, 'P', 3);
insert into reservation(trip_id, person_id, status, no_tickets) values (4, 9, 'N', 1);
insert into reservation(trip_id, person_id, status, no_tickets) values (4, 10, 'C', 1);

-- Dodanie logów zmian statusów dla wybranych rezerwacji
insert into log(reservation_id, log_date, status, no_tickets)
values (1, to_date('2023-05-01', 'YYYY-MM-DD'), 'N', 2);

insert into log(reservation_id, log_date, status, no_tickets)
values (1, to_date('2023-05-10', 'YYYY-MM-DD'), 'P', 2);

insert into log(reservation_id, log_date, status, no_tickets)
values (2, to_date('2023-05-02', 'YYYY-MM-DD'), 'N', 1);

insert into log(reservation_id, log_date, status, no_tickets)
values (2, to_date('2023-05-20', 'YYYY-MM-DD'), 'C', 1);

insert into log(reservation_id, log_date, status, no_tickets)
values (4, to_date('2024-07-15', 'YYYY-MM-DD'), 'N', 2);

insert into log(reservation_id, log_date, status, no_tickets)
values (4, to_date('2024-07-30', 'YYYY-MM-DD'), 'P', 2);

commit;
```


---
# Zadanie 0 - modyfikacja danych, transakcje

Należy zmodyfikować model danych tak żeby rezerwacja mogła dotyczyć kilku miejsc/biletów na wycieczkę
- do tabeli reservation należy dodać pole
	- no_tickets
- do tabeli log należy dodac pole
	- no_tickets
	
Należy zmodyfikować zestaw danych testowych

Należy przeprowadzić kilka eksperymentów związanych ze wstawianiem, modyfikacją i usuwaniem danych
oraz wykorzystaniem transakcji

Skomentuj dzialanie transakcji. Jak działa polecenie `commit`, `rollback`?.
Co się dzieje w przypadku wystąpienia błędów podczas wykonywania transakcji? Porównaj sposób programowania operacji wykorzystujących transakcje w Oracle PL/SQL ze znanym ci systemem/językiem MS Sqlserver T-SQL

pomocne mogą być materiały dostępne tu:
https://upel.agh.edu.pl/mod/folder/view.php?id=311899
w szczególności dokument: `1_ora_modyf.pdf`

## Dodanie pola no_tickets do tabel reservation i log:

```sql
alter table reservation add no_tickets int default 1;
alter table log add no_tickets int default 1;
```

## Jak działają transakcje?
Transakcja to zestaw operacji na bazie danych, które powinny zostać wykonane razem – albo wszystkie, albo żadna. Dzięki temu, nawet jeśli coś pójdzie nie tak (np. błąd w trakcie działania), dane w bazie pozostają spójne.


W Oracle można sterować przebiegiem transakcji za pomocą poleceń:

• **COMMIT** – zatwierdza wszystkie zmiany w bazie danych, które wykonaliśmy od początku transakcji (czyli np. od ostatniego COMMIT lub ROLLBACK). Po tym punkcie zmiany są już trwałe.

• **ROLLBACK** – wycofuje wszystkie niezapisane zmiany. Dzięki temu możemy wrócić do stanu sprzed rozpoczęcia transakcji, jeśli np. wystąpił błąd.

• **SAVEPOINT** - To coś w stylu „punktu kontrolnego” — jeśli coś pójdzie nie tak, można powiedzieć: „OK, cofnij wszystko, ale tylko do tego miejsca”.

W Oracle PL/SQL transakcje rozpoczynają się automatycznie po pierwszej operacji DML i trwają do jawnego `COMMIT` lub `ROLLBACK`, co daje pełną kontrolę nad momentem zatwierdzenia zmian. W T-SQL transakcje domyślnie działają w trybie autocommit, a ręczne sterowanie transakcją wymaga użycia `BEGIN TRAN`, `COMMIT` i `ROLLBACK`. Obsługa błędów w Oracle opiera się na bloku `EXCEPTION`, natomiast w T-SQL wykorzystuje się `TRY...CATCH`, który jest mniej elastyczny. Oracle lepiej wspiera pracę z `SAVEPOINT` i częściowym wycofywaniem zmian.

## Obsługa błędów
W przypadku błędu, np. próby dodania rezerwacji ponad limit miejsc, wykonywana jest sekcja EXCEPTION, w której można zastosować ROLLBACK. Dzięki temu wszystkie wcześniejsze zmiany w ramach tej transakcji są cofane. Dane pozostają w stanie spójnym i zgodnym z logiką aplikacyjną.
# Zadanie 1 - widoki


Tworzenie widoków. Należy przygotować kilka widoków ułatwiających dostęp do danych. Należy zwrócić uwagę na strukturę kodu (należy unikać powielania kodu)

Widoki:
-   `vw_reservation`
	- widok łączy dane z tabel: `trip`,  `person`,  `reservation`
	- zwracane dane:  `reservation_id`,  `country`, `trip_date`, `trip_name`, `firstname`, `lastname`, `status`, `trip_id`, `person_id`, `no_tickets`
- `vw_trip` 
	- widok pokazuje liczbę wolnych miejsc na każdą wycieczkę
	- zwracane dane: `trip_id`, `country`, `trip_date`, `trip_name`, `max_no_places`, `no_available_places` (liczba wolnych miejsc)
-  `vw_available_trip`
	- podobnie jak w poprzednim punkcie, z tym że widok pokazuje jedynie dostępne wycieczki (takie które są w przyszłości i są na nie wolne miejsca)


Proponowany zestaw widoków można rozbudować wedle uznania/potrzeb
- np. można dodać nowe/pomocnicze widoki, funkcje
- np. można zmienić def. widoków, dodając nowe/potrzebne pola

# Zadanie 1  - rozwiązanie
## Widok nr 1. 
```sql
create or replace view vw_reservation as
select
  r.reservation_id,
  t.country,
  t.trip_date,
  t.trip_name,
  p.firstname,
  p.lastname,
  r.status,
  r.trip_id,
  r.person_id,
  r.no_tickets
from reservation r
join trip t on r.trip_id = t.trip_id
join person p on r.person_id = p.person_id;
```

Przykładowe użycie (dane testowe):

``` sql
SELECT * from vw_reservation;
```
![[Pasted image 20250325192219.png]]

## Widok nr 2.
```sql
create or replace view vw_trip as
select
  t.trip_id,
  t.country,
  t.trip_date,
  t.trip_name,
  t.max_no_places,
  case 
    when rs.total_reserved is null then t.max_no_places
    else t.max_no_places - rs.total_reserved
  end as no_available_places
from trip t
left join (
  select trip_id, sum(no_tickets) as total_reserved
  from reservation
  where status in ('P', 'N')
  group by trip_id
) rs on t.trip_id = rs.trip_id;
```

**Ważna adnotacja:**
*Zarówno potwierdzone ('P') jak i nowe ('N') rezerwacje są uwzględniane w zajętych miejscach.*

**Przykładowe wywołanie:**. 
```sql
SELECT * from vw_trip;
```

![[Pasted image 20250325193243.png]]

## Widok nr 3.
```sql
create or replace view vw_available_trip as
select *
from vw_trip
where trip_date > sysdate
  and no_available_places > 0;
```

_Bazuje na_  poprzednim widoku: _vw_trip_, unika powielania kodu.

```sql
SELECT * from vw_available_trip;
```

![[Pasted image 20250325193329.png]]

---
# Zadanie 2  - funkcje


Tworzenie funkcji pobierających dane/tabele. Podobnie jak w poprzednim przykładzie należy przygotować kilka funkcji ułatwiających dostęp do danych

Procedury:
- `f_trip_participants`
	- zadaniem funkcji jest zwrócenie listy uczestników wskazanej wycieczki
	- parametry funkcji: `trip_id`
	- funkcja zwraca podobny zestaw danych jak widok  `vw_eservation`
-  `f_person_reservations`
	- zadaniem funkcji jest zwrócenie listy rezerwacji danej osoby 
	- parametry funkcji: `person_id`
	- funkcja zwraca podobny zestaw danych jak widok `vw_reservation`
-  `f_available_trips_to`
	- zadaniem funkcji jest zwrócenie listy wycieczek do wskazanego kraju, dostępnych w zadanym okresie czasu (od `date_from` do `date_to`)
	- parametry funkcji: `country`, `date_from`, `date_to`


Funkcje powinny zwracać tabelę/zbiór wynikowy. Należy rozważyć dodanie kontroli parametrów, (np. jeśli parametrem jest `trip_id` to można sprawdzić czy taka wycieczka istnieje). Podobnie jak w przypadku widoków należy zwrócić uwagę na strukturę kodu

Czy kontrola parametrów w przypadku funkcji ma sens?
- jakie są zalety/wady takiego rozwiązania?

Proponowany zestaw funkcji można rozbudować wedle uznania/potrzeb
- np. można dodać nowe/pomocnicze funkcje/procedury

# Zadanie 2  - rozwiązanie
## Funkcja 1: *f_trip_participants*

Definicja zwracanego obiektu:

```sql
create or replace type trip_participant_row as object (
  reservation_id number,
  country        varchar2(50),
  trip_date      date,
  trip_name      varchar2(100),
  firstname      varchar2(50),
  lastname       varchar2(50),
  status         char(1),
  trip_id        number,
  person_id      number,
  no_tickets     number
);
/

create or replace type trip_participant_tab as table of trip_participant_row;
/
```

Definicja funkcji:

```sql
CREATE OR REPLACE FUNCTION f_trip_participants(p_trip_id NUMBER)
  RETURN trip_participant_tab
AS
  result trip_participant_tab;
BEGIN
  SELECT trip_participant_row(
    r.reservation_id,
    t.country,
    t.trip_date,
    t.trip_name,
    p.firstname,
    p.lastname,
    r.status,
    r.trip_id,
    r.person_id,
    r.no_tickets
  )
  BULK COLLECT INTO result
  FROM reservation r
  JOIN trip t ON r.trip_id = t.trip_id
  JOIN person p ON r.person_id = p.person_id
  WHERE r.trip_id = p_trip_id;

  RETURN result;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    -- Obsługa przypadku, gdy nie znaleziono danych
    DBMS_OUTPUT.PUT_LINE('Brak uczestników dla podanego ID wycieczki.');
    RETURN NULL;
  WHEN TOO_MANY_ROWS THEN
    -- Obsługa przypadku, gdy zapytanie zwraca zbyt wiele wierszy
    DBMS_OUTPUT.PUT_LINE('Zapytanie zwróciło zbyt wiele wierszy.');
    RETURN NULL;
  WHEN OTHERS THEN
    -- Obsługa wszystkich innych wyjątków
    DBMS_OUTPUT.PUT_LINE('Wystąpił nieoczekiwany błąd: ' || SQLERRM);
    RETURN NULL;
END f_trip_participants;


```

Przykładowe wywołanie funkcji:
``` sql
select * from table(f_trip_participants(2));
```

![[Pasted image 20250326100523.png]]
---

## Funkcja 2: `f_person_reservations

```sql
create or replace type trip_participant_row as object (
  reservation_id number,
  country        varchar2(50),
  trip_date      date,
  trip_name      varchar2(100),
  firstname      varchar2(50),
  lastname       varchar2(50),
  status         char(1),
  trip_id        number,
  person_id      number,
  no_tickets     number
);
/

create or replace type trip_participant_tab as table of trip_participant_row;
/
```


```sql
CREATE OR REPLACE FUNCTION f_person_reservations(p_person_id NUMBER)
  RETURN trip_participant_tab
AS
  result trip_participant_tab;
BEGIN
  SELECT trip_participant_row(
    r.reservation_id,
    t.country,
    t.trip_date,
    t.trip_name,
    p.firstname,
    p.lastname,
    r.status,
    r.trip_id,
    r.person_id,
    r.no_tickets
  )
  BULK COLLECT INTO result
  FROM reservation r
  JOIN trip t ON r.trip_id = t.trip_id
  JOIN person p ON r.person_id = p.person_id
  WHERE r.person_id = p_person_id;

  RETURN result;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    DBMS_OUTPUT.PUT_LINE('Brak rezerwacji dla podanego ID osoby.');
    RETURN NULL;
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Wystąpił nieoczekiwany błąd: ' || SQLERRM);
    RETURN NULL;
END f_person_reservations;

```

```sql
select * from table(f_person_reservations(2));
```
![[Pasted image 20250326101152.png]]

## Funkcja 3:  `f_available_trips_to`

```sql
CREATE OR REPLACE FUNCTION f_available_trips_to(
  p_country   VARCHAR2,
  p_date_from DATE,
  p_date_to   DATE
) RETURN trip_info_tab
AS
  result trip_info_tab;
BEGIN
  SELECT trip_info_row(
    v.trip_id,
    v.country,
    v.trip_date,
    v.trip_name,
    v.max_no_places,
    v.no_available_places
  )
  BULK COLLECT INTO result
  FROM vw_trip v
  WHERE LOWER(v.country) = LOWER(p_country)
    AND v.trip_date BETWEEN p_date_from AND p_date_to
    AND v.trip_date > SYSDATE
    AND v.no_available_places > 0;

  RETURN result;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    DBMS_OUTPUT.PUT_LINE('Brak dostępnych wycieczek do podanego kraju w podanym zakresie dat.');
    RETURN NULL;
  WHEN OTHERS THEN
    DBMS_OUTPUT.PUT_LINE('Wystąpił błąd: ' || SQLERRM);
    RETURN NULL;
END f_available_trips_to;

```

```sql
select * from table(f_available_trips_to('Polska', date '2025-01-01', date '2025-12-31'));
```

# Zadanie 3  - procedury

Tworzenie procedur modyfikujących dane. Należy przygotować zestaw procedur pozwalających na modyfikację danych oraz kontrolę poprawności ich wprowadzania

Procedury
- `p_add_reservation`
	- zadaniem procedury jest dopisanie nowej rezerwacji
	- parametry: `trip_id`, `person_id`,  `no_tickets`
	- procedura powinna kontrolować czy wycieczka jeszcze się nie odbyła, i czy sa wolne miejsca
	- procedura powinna również dopisywać inf. do tabeli `log`
- `p_modify_reservation_status
	- zadaniem procedury jest zmiana statusu rezerwacji 
	- parametry: `reservation_id`, `status`
	- procedura powinna kontrolować czy możliwa jest zmiana statusu, np. zmiana statusu już anulowanej wycieczki (przywrócenie do stanu aktywnego nie zawsze jest możliwa – może już nie być miejsc)
	- procedura powinna również dopisywać inf. do tabeli `log`
- `p_modify_reservation
	- zadaniem procedury jest zmiana statusu rezerwacji 
	- parametry: `reservation_id`, `no_iickets`
	- procedura powinna kontrolować czy możliwa jest zmiana liczby sprzedanych/zarezerwowanych biletów – może już nie być miejsc
	- procedura powinna również dopisywać inf. do tabeli `log`
- `p_modify_max_no_places`
	- zadaniem procedury jest zmiana maksymalnej liczby miejsc na daną wycieczkę 
	- parametry: `trip_id`, `max_no_places`
	- nie wszystkie zmiany liczby miejsc są dozwolone, nie można zmniejszyć liczby miejsc na wartość poniżej liczby zarezerwowanych miejsc

Należy rozważyć użycie transakcji

Należy zwrócić uwagę na kontrolę parametrów (np. jeśli parametrem jest trip_id to należy sprawdzić czy taka wycieczka istnieje, jeśli robimy rezerwację to należy sprawdzać czy są wolne miejsca itp..)


Proponowany zestaw procedur można rozbudować wedle uznania/potrzeb
- np. można dodać nowe/pomocnicze funkcje/procedury

# Zadanie 3  - rozwiązanie

## Procedura 1
```sql
CREATE OR REPLACE PROCEDURE p_add_reservation (  
  p_trip_id     IN trip.trip_id%TYPE,  
  p_person_id   IN person.person_id%TYPE,  
  p_no_tickets  IN reservation.no_tickets%TYPE  
)  
AS  
  v_available_places NUMBER;  
  v_trip_date        DATE;  
  v_reservation_id   reservation.reservation_id%TYPE;  
  v_dummy            NUMBER; -- pomocnicza zmienna do sprawdzenia osoby
BEGIN  
  -- Sprawdzenie, czy osoba istnieje
  SELECT 1 INTO v_dummy
  FROM person
  WHERE person_id = p_person_id;

  -- Sprawdzenie danych wycieczki
  SELECT no_available_places, trip_date  
  INTO v_available_places, v_trip_date  
  FROM vw_trip  
  WHERE trip_id = p_trip_id;  
  
  IF v_trip_date <= SYSDATE THEN  
    RAISE_APPLICATION_ERROR(-20001, 'Wycieczka już się odbyła.');  
  END IF;  
  
  IF v_available_places < p_no_tickets THEN  
    RAISE_APPLICATION_ERROR(-20002, 'Brak wystarczającej liczby miejsc.');  
  END IF;  
  
  -- Dodanie rezerwacji
  INSERT INTO reservation (  
    trip_id, person_id, no_tickets, status  
  ) VALUES (  
    p_trip_id, p_person_id, p_no_tickets, 'N'  
  ) RETURNING reservation_id INTO v_reservation_id;  
  
  -- Dodanie wpisu do logu
  INSERT INTO log (  
    reservation_id, log_date, status, no_tickets  
  ) VALUES (  
    v_reservation_id, SYSDATE, 'N', p_no_tickets  
  );  

EXCEPTION  
  WHEN NO_DATA_FOUND THEN
    ROLLBACK;
    DBMS_OUTPUT.PUT_LINE('Błąd: Nie znaleziono osoby lub wycieczki.');
    RAISE_APPLICATION_ERROR(-20003, 'Nie znaleziono osoby lub wycieczki.');
  WHEN OTHERS THEN  
    ROLLBACK;
    DBMS_OUTPUT.PUT_LINE('Błąd: ' || SQLERRM);  
    RAISE; 
END;  
/


```
``` sql
begin  
  p_add_reservation(  
    p_trip_id    => 3,         
    p_person_id  => 5,          
    p_no_tickets => 4            
);  
end;  
/
```
## Procedura 2
```sql
CREATE OR REPLACE PROCEDURE p_modify_reservation_status (
  p_reservation_id IN reservation.reservation_id%TYPE,
  p_status         IN reservation.status%TYPE
)
AS
  v_current_status reservation.status%TYPE;
  v_trip_id        reservation.trip_id%TYPE;
  v_no_tickets     reservation.no_tickets%TYPE;
  v_available      NUMBER;
BEGIN
  -- Sprawdzenie czy rezerwacja istnieje
  SELECT status, trip_id, no_tickets
  INTO v_current_status, v_trip_id, v_no_tickets
  FROM reservation
  WHERE reservation_id = p_reservation_id;

  -- Jeśli próbujemy przywrócić rezerwację z C na N/P, to sprawdź dostępność
  IF v_current_status = 'C' AND p_status IN ('N', 'P') THEN
    SELECT no_available_places
    INTO v_available
    FROM vw_trip
    WHERE trip_id = v_trip_id;

    IF v_available < v_no_tickets THEN
      RAISE_APPLICATION_ERROR(-20003, 'Brak wolnych miejsc na przywrócenie rezerwacji.');
    END IF;
  END IF;

  -- Zmiana statusu
  UPDATE reservation
  SET status = p_status
  WHERE reservation_id = p_reservation_id;

  -- Logowanie zmiany
  INSERT INTO log (
    reservation_id, log_date, status, no_tickets
  ) VALUES (
    p_reservation_id, SYSDATE, p_status, v_no_tickets
  );

  COMMIT;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20001, 'Rezerwacja lub wycieczka nie istnieje.');
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
```

```sql
begin  
  p_modify_reservation_status(  
    p_reservation_id => 47,  
    p_status         => 'P'  
  );  
end;  
/
```
## Procedura 3
```sql
CREATE OR REPLACE PROCEDURE p_modify_reservation (
    p_reservation_id IN reservation.reservation_id%TYPE,
    p_no_tickets     IN NUMBER
)
IS
    v_trip_id       reservation.trip_id%TYPE;
    v_status        reservation.status%TYPE;
    v_max_places    trip.max_no_places%TYPE;
    v_reserved_sum  NUMBER;
    v_available     NUMBER;
BEGIN
    -- Sprawdzenie, czy rezerwacja istnieje i pobranie danych
    SELECT trip_id, status INTO v_trip_id, v_status
    FROM reservation
    WHERE reservation_id = p_reservation_id;

    -- Sprawdzenie, czy rezerwacja nie jest anulowana
    IF v_status = 'C' THEN
        RAISE_APPLICATION_ERROR(-20006, 'Nie można modyfikować anulowanej rezerwacji.');
    END IF;

    -- Pobranie liczby dostępnych miejsc z widoku
    SELECT no_available_places INTO v_available
    FROM vw_trip
    WHERE trip_id = v_trip_id;

    -- Widok uwzględnia aktualną rezerwację, więc odejmujemy jej stare no_tickets
    SELECT NVL(SUM(no_tickets), 0) INTO v_reserved_sum
    FROM reservation
    WHERE trip_id = v_trip_id AND status IN ('N', 'P') AND reservation_id != p_reservation_id;

    SELECT max_no_places INTO v_max_places FROM trip WHERE trip_id = v_trip_id;

    IF v_reserved_sum + p_no_tickets > v_max_places THEN
        RAISE_APPLICATION_ERROR(-20010, 'Brak wystarczającej liczby wolnych miejsc.');
    END IF;

    -- Aktualizacja rezerwacji
    UPDATE reservation
    SET no_tickets = p_no_tickets
    WHERE reservation_id = p_reservation_id;

    -- Logowanie zmiany
    INSERT INTO log (reservation_id, log_date, status)
    VALUES (p_reservation_id, SYSDATE, v_status);

    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20011, 'Rezerwacja lub wycieczka nie istnieje.');
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;
```
```sql
begin  
  p_modify_reservation(  
    p_reservation_id => 42,  
    p_no_tickets     => 2  
  );  
end;  
/

```
## Procedura 4
```sql
CREATE OR REPLACE PROCEDURE p_modify_max_no_places (
  p_trip_id       IN trip.trip_id%TYPE,
  p_max_no_places IN trip.max_no_places%TYPE
)
AS
  v_no_reserved_places NUMBER;
BEGIN
  -- Pobranie liczby już zarezerwowanych miejsc z widoku
  SELECT no_reserved_places
  INTO v_no_reserved_places
  FROM vw_trip
  WHERE trip_id = p_trip_id;

  -- Sprawdzenie, czy można ustawić nową wartość
  IF p_max_no_places < v_no_reserved_places THEN
    RAISE_APPLICATION_ERROR(-20005, 'Nie można ustawić liczby miejsc poniżej już zarezerwowanej.');
  END IF;

  -- Aktualizacja liczby miejsc
  UPDATE trip
  SET max_no_places = p_max_no_places
  WHERE trip_id = p_trip_id;

  COMMIT;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20006, 'Wycieczka nie istnieje.');
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/


begin  
  p_modify_max_no_places(  
    p_trip_id => 4,  
    p_max_no_places => 7  
  );  
end;  
/
```

---

# Zadanie 4  - triggery


Zmiana strategii zapisywania do dziennika rezerwacji. Realizacja przy pomocy triggerów

Należy wprowadzić zmianę, która spowoduje, że zapis do dziennika będzie realizowany przy pomocy trigerów

Triggery:
- trigger/triggery obsługujące 
	- dodanie rezerwacji
	- zmianę statusu
	- zmianę liczby zarezerwowanych/kupionych biletów
- trigger zabraniający usunięcia rezerwacji

Oczywiście po wprowadzeniu tej zmiany należy "uaktualnić" procedury modyfikujące dane. 

>UWAGA
Należy stworzyć nowe wersje tych procedur (dodając do nazwy dopisek 4 - od numeru zadania). Poprzednie wersje procedur należy pozostawić w celu  umożliwienia weryfikacji ich poprawności

Należy przygotować procedury: `p_add_reservation_4`, `p_modify_reservation_status_4` , `p_modify_reservation_4`


# Zadanie 4  - rozwiązanie
## Trigger 1 - dodanie rezerwacji

``` sql
create or replace trigger trg_log_insert_reservation
after insert on reservation
for each row
begin
  insert into log(reservation_id, log_date, status, no_tickets)
  values (:new.reservation_id, sysdate, :new.status, :new.no_tickets);
end;
/
```

## Trigger 2 - zmiana statusu
```sql
create or replace trigger trg_log_status_change
after update of status on reservation
for each row
when (old.status != new.status)
begin
  insert into log(reservation_id, log_date, status, no_tickets)
  values (:new.reservation_id, sysdate, :new.status, :new.no_tickets);
end;
/
```

## Trigger 3 - zmiana liczby zarezerwowanych/kupionych biletów
```sql
create or replace trigger trg_log_ticket_change
after update of no_tickets on reservation
for each row
when (old.no_tickets != new.no_tickets)
begin
  insert into log(reservation_id, log_date, status, no_tickets)
  values (:new.reservation_id, sysdate, :new.status, :new.no_tickets);
end;
/
```

## Trigger 4 - trigger zabraniający usunięcia rezerwacji
``` sql
create or replace trigger trg_prevent_reservation_delete
before delete on reservation
for each row
begin
  raise_application_error(-20010, 'Usuwanie rezerwacji jest zabronione.');
end;
/
```
----
## Zmodyfikowane procedury:
#### Procedura 1 - dodanie nowej rezerwacji
``` sql
create or replace procedure p_add_reservation_4 (
    p_trip_id     in trip.trip_id%type,
    p_person_id   in person.person_id%type,
    p_no_tickets  in number
) is
    v_trip_date        trip.trip_date%type;
    v_no_available     number;
    v_dummy            number;
begin
    -- Sprawdzenie wycieczki i dostępnych miejsc
    select trip_date, no_available_places
    into v_trip_date, v_no_available
    from vw_trip
    where trip_id = p_trip_id;

    -- Wycieczka w przeszłości
    if v_trip_date < sysdate then
        raise_application_error(-20001, 'Wycieczka już się odbyła.');
    end if;

    -- Sprawdzenie czy osoba istnieje
    begin
        select 1 into v_dummy
        from person
        where person_id = p_person_id;
    exception
        when no_data_found then
            raise_application_error(-20002, 'Osoba nie istnieje.');
    end;

    -- Sprawdzenie miejsc
    if p_no_tickets > v_no_available then
        raise_application_error(-20003, 'Brak wystarczającej liczby wolnych miejsc.');
    end if;

    -- Dodanie rezerwacji (log zapisze trigger)
    insert into reservation (trip_id, person_id, status, no_tickets)
    values (p_trip_id, p_person_id, 'N', p_no_tickets);

    commit;
exception
    when others then
        rollback;
        raise;
end;
/
```
**Przykładowe wywołanie: **
```sql
BEGIN
  p_add_reservation_4(
    p_trip_id    => 2,
    p_person_id  => 4,
    p_no_tickets => 1
  );
END;
/
```

#### Procedura 2 - zmiana statusu rezerwacji
```sql 
CREATE OR REPLACE PROCEDURE p_modify_reservation_status (
  p_reservation_id IN reservation.reservation_id%TYPE,
  p_status         IN reservation.status%TYPE
)
AS
  v_current_status reservation.status%TYPE;
  v_trip_id        reservation.trip_id%TYPE;
  v_no_tickets     reservation.no_tickets%TYPE;
  v_available      NUMBER;
BEGIN
  -- Sprawdzenie czy rezerwacja istnieje
  SELECT status, trip_id, no_tickets
  INTO v_current_status, v_trip_id, v_no_tickets
  FROM reservation
  WHERE reservation_id = p_reservation_id;

  -- Jeśli próbujemy przywrócić rezerwację z C na N/P, to sprawdź dostępność
  IF v_current_status = 'C' AND p_status IN ('N', 'P') THEN
    SELECT no_available_places
    INTO v_available
    FROM vw_trip
    WHERE trip_id = v_trip_id;

    IF v_available < v_no_tickets THEN
      RAISE_APPLICATION_ERROR(-20003, 'Brak wolnych miejsc na przywrócenie rezerwacji.');
    END IF;
  END IF;

  -- Zmiana statusu
  UPDATE reservation
  SET status = p_status
  WHERE reservation_id = p_reservation_id;

  COMMIT;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20001, 'Rezerwacja lub wycieczka nie istnieje.');
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
```

**Przykładowe wywołanie:**
```sql
BEGIN
  p_modify_reservation_status_4(
    p_reservation_id => 5,
    p_new_status     => 'C'
  );
END;
/
```

#### Procedura 3 - zmiana liczby biletów rezerwacji

```sql
CREATE OR REPLACE PROCEDURE p_modify_reservation (
    p_reservation_id IN reservation.reservation_id%TYPE,
    p_no_tickets     IN NUMBER
)
IS
    v_trip_id       reservation.trip_id%TYPE;
    v_status        reservation.status%TYPE;
    v_max_places    trip.max_no_places%TYPE;
    v_reserved_sum  NUMBER;
    v_available     NUMBER;
BEGIN
    -- Sprawdzenie, czy rezerwacja istnieje i pobranie danych
    SELECT trip_id, status INTO v_trip_id, v_status
    FROM reservation
    WHERE reservation_id = p_reservation_id;

    -- Sprawdzenie, czy rezerwacja nie jest anulowana
    IF v_status = 'C' THEN
        RAISE_APPLICATION_ERROR(-20006, 'Nie można modyfikować anulowanej rezerwacji.');
    END IF;

    -- Pobranie liczby dostępnych miejsc z widoku
    SELECT no_available_places INTO v_available
    FROM vw_trip
    WHERE trip_id = v_trip_id;

    -- Widok uwzględnia aktualną rezerwację, więc odejmujemy jej stare no_tickets
    SELECT NVL(SUM(no_tickets), 0) INTO v_reserved_sum
    FROM reservation
    WHERE trip_id = v_trip_id AND status IN ('N', 'P') AND reservation_id != p_reservation_id;

    SELECT max_no_places INTO v_max_places FROM trip WHERE trip_id = v_trip_id;

    IF v_reserved_sum + p_no_tickets > v_max_places THEN
        RAISE_APPLICATION_ERROR(-20010, 'Brak wystarczającej liczby wolnych miejsc.');
    END IF;

    -- Aktualizacja rezerwacji
    UPDATE reservation
    SET no_tickets = p_no_tickets
    WHERE reservation_id = p_reservation_id;

    COMMIT;

EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RAISE_APPLICATION_ERROR(-20011, 'Rezerwacja lub wycieczka nie istnieje.');
    WHEN OTHERS THEN
        ROLLBACK;
        RAISE;
END;

```
**Przykładowe wywołanie:**
```sql
BEGIN
  p_modify_reservation_4(
    p_reservation_id => 6,
    p_no_tickets     => 2
  );
END;
/
```
---

---
# Zadanie 5  - triggery


Zmiana strategii kontroli dostępności miejsc. Realizacja przy pomocy triggerów

Należy wprowadzić zmianę, która spowoduje, że kontrola dostępności miejsc na wycieczki (przy dodawaniu nowej rezerwacji, zmianie statusu) będzie realizowana przy pomocy trigerów

Triggery:
- Trigger/triggery obsługujące: 
	- dodanie rezerwacji
	- zmianę statusu
	- zmianę liczby zakupionych/zarezerwowanych miejsc/biletów

Oczywiście po wprowadzeniu tej zmiany należy "uaktualnić" procedury modyfikujące dane. 

>UWAGA
Należy stworzyć nowe wersje tych procedur (np. dodając do nazwy dopisek 5 - od numeru zadania). Poprzednie wersje procedur należy pozostawić w celu  umożliwienia weryfikacji ich poprawności. 

Należy przygotować procedury: `p_add_reservation_5`, `p_modify_reservation_status_5`, `p_modify_reservation_status_5`


# Zadanie 5  - rozwiązanie
## Trigger 1 - dodanie nowej rezerwacji
```sql
create or replace trigger trg_check_capacity_insert
before insert on reservation
for each row
declare
  v_available number;
begin
  select no_available_places
  into v_available
  from vw_trip
  where trip_id = :new.trip_id;

  if :new.status in ('N', 'P') and :new.no_tickets > v_available then
    raise_application_error(-20501, 'Brak dostępnych miejsc na wycieczce.');
  end if;
end;
/
```

## Trigger 2 - zmiana statusu
```sql
create or replace trigger trg_check_capacity_status
before update of status on reservation
for each row
declare
  v_available number;
begin
  if :new.status in ('N', 'P') and :old.status not in ('N', 'P') then
    select no_available_places
    into v_available
    from vw_trip
    where trip_id = :new.trip_id;

    if :new.no_tickets > v_available then
      raise_application_error(-20502, 'Brak miejsc przy zmianie statusu rezerwacji.');
    end if;
  end if;
end;
/
```

## Trigger 3 - zmiana liczby zakupionych/zarezerwowanych miejsc/biletów
```sql
create or replace trigger trg_check_capacity_tickets
before update of no_tickets on reservation
for each row
declare
  v_available number;
begin
  if :new.status in ('N', 'P') and :new.no_tickets > :old.no_tickets then
    select no_available_places
    into v_available
    from vw_trip
    where trip_id = :new.trip_id;

    if (:new.no_tickets - :old.no_tickets) > v_available then
      raise_application_error(-20503, 'Brak miejsc przy zwiększeniu liczby biletów.');
    end if;
  end if;
end;
/
```

----
## Zmodyfikowane procedury uwzględniające nowe triggery:

**WAŻNA UWAGA!**
Polecenie jest delikatnie niedoprecyzowane. 
Nasza interpretacja zakłada, że zadanie 5 jest KONTYNUACJĄ zadania 4, zatem mamy już do dyspozycji triggery, które automatycznie logują wprowadzane zmiany (nie trzeba tego robić na poziomie procedur).

## Procedura 1 - dodanie rezerwacji
```sql
create or replace procedure p_add_reservation_5 (
    p_trip_id     in reservation.trip_id%type,
    p_person_id   in reservation.person_id%type,
    p_no_tickets  in reservation.no_tickets%type
)
as
    v_trip_date trip.trip_date%type;
    v_dummy     number;
begin
    -- Sprawdzenie czy wycieczka istnieje i pobranie daty
    begin
        select trip_date into v_trip_date
        from trip
        where trip_id = p_trip_id;
    exception
        when no_data_found then
            raise_application_error(-20001, 'Wycieczka nie istnieje.');
    end;

    -- Sprawdzenie, czy wycieczka jest w przyszłości
    if v_trip_date <= sysdate then
        raise_application_error(-20002, 'Wycieczka już się odbyła.');
    end if;

    -- Sprawdzenie czy osoba istnieje
    begin
        select 1 into v_dummy
        from person
        where person_id = p_person_id;
    exception
        when no_data_found then
            raise_application_error(-20003, 'Osoba nie istnieje.');
    end;

    -- Wstawienie rezerwacji (kontrola liczby dostępnych miejsc i logowanie odbywa się przez triggery)
    insert into reservation (trip_id, person_id, status, no_tickets)
    values (p_trip_id, p_person_id, 'N', p_no_tickets);

    commit;
exception
    when others then
        rollback;
        raise;
end;
/
```

**Przykładowe wywołanie:**
```sql
BEGIN
  p_add_reservation_5(
    p_trip_id    => 1,
    p_person_id  => 6,
    p_no_tickets => 1
  );
END;
/
```

## Procedura 2 - zmiana statusu rezerwacji

```sql
create or replace procedure p_modify_reservation_status_5 (
    p_reservation_id in reservation.reservation_id%type,
    p_new_status     in reservation.status%type
)
as
    v_dummy number;
begin
    -- Sprawdzenie czy rezerwacja istnieje
    begin
        select 1 into v_dummy
        from reservation
        where reservation_id = p_reservation_id;
    exception
        when no_data_found then
            raise_application_error(-20004, 'Rezerwacja nie istnieje.');
    end;

    -- Zmiana statusu (kontrola liczby dostępnych miejsc i logowanie przez triggery)
    update reservation
    set status = p_new_status
    where reservation_id = p_reservation_id;

    commit;
exception
    when others then
        rollback;
        raise;
end;
/
```

**Przykładowe wywołanie:**
```sql
BEGIN
  p_modify_reservation_status_5(
    p_reservation_id => 8,
    p_new_status     => 'C'
  );
END;
/
```

## Procedura 3 - zmiana liczby zarezerwowanych biletów
```sql
create or replace procedure p_modify_reservation_5 (
    p_reservation_id in reservation.reservation_id%type,
    p_no_tickets     in reservation.no_tickets%type
)
as
    v_status reservation.status%type;
begin
    -- Sprawdzenie czy rezerwacja istnieje i pobranie statusu
    begin
        select status into v_status
        from reservation
        where reservation_id = p_reservation_id;
    exception
        when no_data_found then
            raise_application_error(-20005, 'Rezerwacja nie istnieje.');
    end;

    -- Nie można zmieniać biletów w anulowanej rezerwacji
    if v_status = 'C' then
        raise_application_error(-20006, 'Nie można zmienić liczby biletów w anulowanej rezerwacji.');
    end if;

    -- Zmiana liczby biletów (kontrola i logowanie – trigger)
    update reservation
    set no_tickets = p_no_tickets
    where reservation_id = p_reservation_id;

    commit;
exception
    when others then
        rollback;
        raise;
end;
/
```
**Przykładowe wywołanie:**
```sql
BEGIN
  p_modify_reservation_5(
    p_reservation_id => 5,
    p_no_tickets     => 3
  );
END;
/
```
---
# Zadanie 6


Zmiana struktury bazy danych. W tabeli `trip`  należy dodać  redundantne pole `no_available_places`.  Dodanie redundantnego pola uprości kontrolę dostępnych miejsc, ale nieco skomplikuje procedury dodawania rezerwacji, zmiany statusu czy też zmiany maksymalnej liczby miejsc na wycieczki.

Należy przygotować polecenie/procedurę przeliczającą wartość pola `no_available_places` dla wszystkich wycieczek (do jednorazowego wykonania)

Obsługę pola `no_available_places` można zrealizować przy pomocy procedur lub triggerów

Należy zwrócić uwagę na spójność rozwiązania.

>UWAGA
Należy stworzyć nowe wersje tych widoków/procedur/triggerów (np. dodając do nazwy dopisek 6 - od numeru zadania). Poprzednie wersje procedur należy pozostawić w celu  umożliwienia weryfikacji ich poprawności. 


- zmiana struktury tabeli

```sql
alter table trip add  
    no_available_places int null
```

- polecenie przeliczające wartość `no_available_places`
	- należy wykonać operację "przeliczenia"  liczby wolnych miejsc i aktualizacji pola  `no_available_places`

# Zadanie 6  - rozwiązanie

```sql
create or replace procedure p_recalculate_no_available_places6 as  
begin  
  update trip t  
    set no_available_places = (  
  t.max_no_places -  
  (select nvl(sum(r.no_tickets), 0)  
   from reservation r  
   where r.trip_id = t.trip_id  
     and r.status in ('N', 'P'))  
)  
end;

```
### Procedura 1:

```sql

CREATE OR REPLACE PROCEDURE p_add_reservation6 (  
  p_trip_id     IN trip.trip_id%TYPE,  
  p_person_id   IN person.person_id%TYPE,  
  p_no_tickets  IN reservation.no_tickets%TYPE  
)  
AS  
  v_available_places NUMBER;  
  v_trip_date        DATE;  
BEGIN  
  -- Sprawdzenie daty i dostępności miejsc  
  SELECT no_available_places, trip_date  
  INTO v_available_places, v_trip_date  
  FROM trip  
  WHERE trip_id = p_trip_id;  
  
  IF v_trip_date <= SYSDATE THEN  
    RAISE_APPLICATION_ERROR(-20001, 'Wycieczka już się odbyła.');  
  END IF;  
  
  IF v_available_places < p_no_tickets THEN  
    RAISE_APPLICATION_ERROR(-20002, 'Brak wystarczającej liczby miejsc.');  
  END IF;  
  
  -- Wstawienie rezerwacji 
  INSERT INTO reservation (
    reservation_id, trip_id, person_id, no_tickets, status
  ) VALUES (
    s_reservation_seq.NEXTVAL, p_trip_id, p_person_id, p_no_tickets, 'N'
  );

  -- Zapis do logu
  INSERT INTO log (
    reservation_id, log_date, status, no_tickets
  ) VALUES (
    s_reservation_seq.CURRVAL, SYSDATE, 'N', p_no_tickets
  );

  -- Przeliczenie dostępnych miejsc
  p_recalculate_no_available_places6;

  COMMIT;

EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
```

### Procedura 2:
```sql
CREATE OR REPLACE PROCEDURE p_modify_reservation_status6 (  
  p_reservation_id IN reservation.reservation_id%TYPE,  
  p_status         IN reservation.status%TYPE  
)  
AS  
  v_current_status reservation.status%TYPE;  
  v_trip_id        reservation.trip_id%TYPE;  
  v_no_tickets     reservation.no_tickets%TYPE;  
  v_available      NUMBER;  
BEGIN  
  -- Pobierz dane rezerwacji  
  SELECT status, trip_id, no_tickets  
  INTO v_current_status, v_trip_id, v_no_tickets  
  FROM reservation  
  WHERE reservation_id = p_reservation_id;  
  
  -- Jeśli przywracamy anulowaną rezerwację  
  IF v_current_status = 'C' AND p_status IN ('N', 'P') THEN  
    SELECT no_available_places  
    INTO v_available  
    FROM trip  
    WHERE trip_id = v_trip_id;  
  
    IF v_available < v_no_tickets THEN  
      RAISE_APPLICATION_ERROR(-20003, 'Za mało miejsc na przywrócenie rezerwacji.');  
    END IF;  
  END IF;  
  
  -- Aktualizacja statusu rezerwacji  
  UPDATE reservation  
  SET status = p_status  
  WHERE reservation_id = p_reservation_id;  
  
  -- Dodanie nowego wpisu do logu  
  INSERT INTO log (
    reservation_id, log_date, status, no_tickets
  ) VALUES (
    p_reservation_id, SYSDATE, p_status, v_no_tickets
  );  

  -- Przeliczenie dostępnych miejsc  
  p_recalculate_no_available_places6;

  COMMIT;

EXCEPTION  
  WHEN NO_DATA_FOUND THEN  
    RAISE_APPLICATION_ERROR(-20004, 'Rezerwacja nie istnieje.');  
  WHEN OTHERS THEN  
    ROLLBACK;  
    RAISE;  
END;
/
```

```sql
BEGIN
  p_modify_reservation_status6(
    p_reservation_id => 42,
    p_status         => 'C'
  );
END;
/

BEGIN
  p_modify_reservation_status6(
    p_reservation_id => 42,
    p_status         => 'N'
  );
END;
/

```
### Procedura 3:
```sql
CREATE OR REPLACE PROCEDURE p_modify_reservation6 (
  p_reservation_id IN reservation.reservation_id%TYPE,
  p_no_tickets     IN reservation.no_tickets%TYPE
)
AS
  v_old_tickets reservation.no_tickets%TYPE;
  v_trip_id     reservation.trip_id%TYPE;
  v_available   NUMBER;
  v_status      reservation.status%TYPE;
BEGIN
  -- Pobranie danych rezerwacji
  SELECT no_tickets, trip_id, status
  INTO v_old_tickets, v_trip_id, v_status
  FROM reservation
  WHERE reservation_id = p_reservation_id;

  -- Sprawdzenie dostępności miejsc przy zwiększeniu liczby biletów
  IF p_no_tickets > v_old_tickets AND v_status IN ('N', 'P') THEN
    SELECT no_available_places
    INTO v_available
    FROM trip
    WHERE trip_id = v_trip_id;

    IF v_available < (p_no_tickets - v_old_tickets) THEN
      RAISE_APPLICATION_ERROR(-20004, 'Brak wolnych miejsc.');
    END IF;
  END IF;

  -- Aktualizacja rezerwacji
  UPDATE reservation
  SET no_tickets = p_no_tickets
  WHERE reservation_id = p_reservation_id;

  -- Dodanie nowego wpisu do logu
  INSERT INTO log (
    reservation_id, log_date, status, no_tickets
  ) VALUES (
    p_reservation_id, SYSDATE, v_status, p_no_tickets
  );

  -- Przeliczenie dostępnych miejsc
  p_recalculate_no_available_places6;

  COMMIT;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20005, 'Rezerwacja nie istnieje.');
  WHEN OTHERS THEN
    ROLLBACK;
    RAISE;
END;
/
```
### Procedura 4:
```sql 
CREATE OR REPLACE PROCEDURE p_modify_max_no_places6 (
  p_trip_id       IN trip.trip_id%TYPE,
  p_max_no_places IN trip.max_no_places%TYPE
)
AS
  v_reserved NUMBER;
BEGIN
  SELECT NVL(SUM(no_tickets), 0)
  INTO v_reserved
  FROM reservation
  WHERE trip_id = p_trip_id
    AND status IN ('N', 'P');

  IF p_max_no_places < v_reserved THEN
    RAISE_APPLICATION_ERROR(-20005, 'Liczba miejsc nie może być mniejsza niż liczba rezerwacji.');
  END IF;

  UPDATE trip
  SET max_no_places = p_max_no_places
  WHERE trip_id = p_trip_id;

  -- Przeliczenie dostępnych miejsc
  p_recalculate_no_available_places6;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20006, 'Nie znaleziono wycieczki.');
  WHEN OTHERS THEN
    RAISE;
END;
/


begin  
  p_modify_max_no_places6(  
    p_trip_id        => 4,  
    p_max_no_places  => 25  
  );  
end;  
/
```

### WIDOKI:
```sql
CREATE OR REPLACE VIEW vw_reservation6 AS
SELECT
  r.reservation_id,
  t.country,
  t.trip_date,
  t.trip_name,
  p.firstname,
  p.lastname,
  r.status,
  r.trip_id,
  r.person_id,
  r.no_tickets
FROM reservation r
JOIN trip t ON r.trip_id = t.trip_id
JOIN person p ON r.person_id = p.person_id;

CREATE OR REPLACE VIEW vw_trip6 AS
SELECT
  t.trip_id,
  t.country,
  t.trip_date,
  t.trip_name,
  t.max_no_places,
  t.no_available_places
FROM trip t;

CREATE OR REPLACE VIEW vw_available_trip6 AS
SELECT *
FROM vw_trip6
WHERE trip_date > SYSDATE
  AND no_available_places > 0;
```

---
## Zadanie 6a  - procedury



Obsługę pola `no_available_places` należy zrealizować przy pomocy procedur
- procedura dodająca rezerwację powinna aktualizować pole `no_available_places` w tabeli trip
- podobnie procedury odpowiedzialne za zmianę statusu oraz zmianę maksymalnej liczby miejsc na wycieczkę
- należy przygotować procedury oraz jeśli jest to potrzebne, zaktualizować triggery oraz widoki



>UWAGA
Należy stworzyć nowe wersje tych widoków/procedur/triggerów (np. dodając do nazwy dopisek 6a - od numeru zadania). Poprzednie wersje procedur należy pozostawić w celu  umożliwienia weryfikacji ich poprawności. 
- może  być potrzebne wyłączenie 'poprzednich wersji' triggerów 


## Zadanie 6a  - rozwiązanie

### Procedura 1:

```sql

CREATE OR REPLACE PROCEDURE p_add_reservation6a (
  p_trip_id     IN trip.trip_id%TYPE,
  p_person_id   IN person.person_id%TYPE,
  p_no_tickets  IN reservation.no_tickets%TYPE
)
AS
  v_trip_date        trip.trip_date%TYPE;
  v_available_places trip.no_available_places%TYPE;
  v_reservation_id   reservation.reservation_id%TYPE;
BEGIN
  SELECT trip_date, no_available_places
  INTO v_trip_date, v_available_places
  FROM trip
  WHERE trip_id = p_trip_id;

  IF v_trip_date <= SYSDATE THEN
    RAISE_APPLICATION_ERROR(-20001, 'Wycieczka już się odbyła.');
  END IF;

  IF v_available_places < p_no_tickets THEN
    RAISE_APPLICATION_ERROR(-20002, 'Brak wystarczającej liczby miejsc.');
  END IF;

  INSERT INTO reservation (trip_id, person_id, no_tickets, status)
  VALUES (p_trip_id, p_person_id, p_no_tickets, 'N')
  RETURNING reservation_id INTO v_reservation_id;

  UPDATE trip
  SET no_available_places = no_available_places - p_no_tickets
  WHERE trip_id = p_trip_id;

  INSERT INTO log (reservation_id, log_date, status, no_tickets)
  VALUES (v_reservation_id, SYSDATE, 'N', p_no_tickets);

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20010, 'Wycieczka nie istnieje.');
  WHEN TOO_MANY_ROWS THEN
    RAISE_APPLICATION_ERROR(-20011, 'Znaleziono więcej niż jedną wycieczkę.');
  WHEN OTHERS THEN
    RAISE_APPLICATION_ERROR(-20999, 'Błąd dodawania rezerwacji: ' || SQLERRM);
END;
/

BEGIN
  p_add_reservation6a(
    p_trip_id     => 3,
    p_person_id   => 8,
    p_no_tickets  => 2
  );
END;
/
```

### Procedura 2:
```sql
CREATE OR REPLACE PROCEDURE p_modify_reservation_status6a (
  p_reservation_id IN reservation.reservation_id%TYPE,
  p_status         IN reservation.status%TYPE
)
AS
  v_trip_id        reservation.trip_id%TYPE;
  v_no_tickets     reservation.no_tickets%TYPE;
  v_current_status reservation.status%TYPE;
  v_available      trip.no_available_places%TYPE;
BEGIN
  SELECT trip_id, no_tickets, status
  INTO v_trip_id, v_no_tickets, v_current_status
  FROM reservation
  WHERE reservation_id = p_reservation_id;

  IF v_current_status = 'C' AND p_status IN ('N', 'P') THEN
    SELECT no_available_places INTO v_available FROM trip WHERE trip_id = v_trip_id;

    IF v_available < v_no_tickets THEN
      RAISE_APPLICATION_ERROR(-20003, 'Brak miejsc na przywrócenie rezerwacji.');
    END IF;

    UPDATE trip
    SET no_available_places = no_available_places - v_no_tickets
    WHERE trip_id = v_trip_id;

  ELSIF p_status = 'C' AND v_current_status IN ('N', 'P') THEN
    UPDATE trip
    SET no_available_places = no_available_places + v_no_tickets
    WHERE trip_id = v_trip_id;
  END IF;

  UPDATE reservation
  SET status = p_status
  WHERE reservation_id = p_reservation_id;

  INSERT INTO log (reservation_id, log_date, status, no_tickets)
  VALUES (p_reservation_id, SYSDATE, p_status, v_no_tickets);

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20004, 'Rezerwacja nie istnieje.');
  WHEN TOO_MANY_ROWS THEN
    RAISE_APPLICATION_ERROR(-20005, 'Znaleziono wiele rezerwacji o tym ID.');
  WHEN OTHERS THEN
    RAISE_APPLICATION_ERROR(-20999, 'Błąd zmiany statusu rezerwacji: ' || SQLERRM);
END;
/
```

```sql
BEGIN
  p_modify_reservation_status6a(
    p_reservation_id => 42,
    p_status         => 'C'
  );
END;
/

BEGIN
  p_modify_reservation_status6a(
    p_reservation_id => 42,
    p_status         => 'N'
  );
END;
/

```
### Procedura 3:
```sql
CREATE OR REPLACE PROCEDURE p_modify_reservation6a (
  p_reservation_id IN reservation.reservation_id%TYPE,
  p_no_tickets     IN reservation.no_tickets%TYPE
)
AS
  v_old_tickets reservation.no_tickets%TYPE;
  v_trip_id     reservation.trip_id%TYPE;
  v_status      reservation.status%TYPE;
  v_available   trip.no_available_places%TYPE;
BEGIN
  SELECT no_tickets, trip_id, status
  INTO v_old_tickets, v_trip_id, v_status
  FROM reservation
  WHERE reservation_id = p_reservation_id;

  IF v_status IN ('N', 'P') AND p_no_tickets > v_old_tickets THEN
    SELECT no_available_places INTO v_available FROM trip WHERE trip_id = v_trip_id;

    IF v_available < (p_no_tickets - v_old_tickets) THEN
      RAISE_APPLICATION_ERROR(-20006, 'Brak miejsc na zwiększenie liczby biletów.');
    END IF;

    UPDATE trip
    SET no_available_places = no_available_places - (p_no_tickets - v_old_tickets)
    WHERE trip_id = v_trip_id;

  ELSIF v_status IN ('N', 'P') AND p_no_tickets < v_old_tickets THEN
    UPDATE trip
    SET no_available_places = no_available_places + (v_old_tickets - p_no_tickets)
    WHERE trip_id = v_trip_id;
  END IF;

  UPDATE reservation
  SET no_tickets = p_no_tickets
  WHERE reservation_id = p_reservation_id;

  INSERT INTO log (reservation_id, log_date, status, no_tickets)
  VALUES (p_reservation_id, SYSDATE, v_status, p_no_tickets);

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20007, 'Rezerwacja nie istnieje.');
  WHEN TOO_MANY_ROWS THEN
    RAISE_APPLICATION_ERROR(-20008, 'Znaleziono wiele rezerwacji o tym ID.');
  WHEN OTHERS THEN
    RAISE_APPLICATION_ERROR(-20999, 'Błąd zmiany liczby biletów: ' || SQLERRM);
END;
/

BEGIN
  p_modify_reservation6a(
    p_reservation_id => 44,
    p_no_tickets     => 3
  );
END;
/
```
### Procedura 4:
```sql
CREATE OR REPLACE PROCEDURE p_modify_max_no_places6a (
  p_trip_id       IN trip.trip_id%TYPE,
  p_max_no_places IN trip.max_no_places%TYPE
)
AS
  v_reserved NUMBER;
BEGIN
  -- Oblicz sumę aktywnych rezerwacji
  SELECT NVL(SUM(no_tickets), 0)
  INTO v_reserved
  FROM reservation
  WHERE trip_id = p_trip_id
    AND status IN ('N', 'P');

  -- Sprawdź, czy nowy limit miejsc jest poprawny
  IF p_max_no_places < v_reserved THEN
    RAISE_APPLICATION_ERROR(-20009, 'Liczba miejsc nie może być mniejsza niż liczba rezerwacji.');
  END IF;

  -- Zaktualizuj max_no_places i no_available_places bez użycia funkcji
  UPDATE trip
  SET max_no_places       = p_max_no_places,
      no_available_places = p_max_no_places - v_reserved
  WHERE trip_id = p_trip_id;

EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20010, 'Wycieczka nie istnieje.');
  WHEN OTHERS THEN
    RAISE_APPLICATION_ERROR(-20999, 'Błąd zmiany limitu miejsc: ' || SQLERRM);
END;
/

BEGIN
  p_modify_max_no_places6a(
    p_trip_id        => 4,
    p_max_no_places  => 25
  );
END;
/
```
Widoki pozostają niezmienne względem zadania 6.
---
## Zadanie 6b -  triggery


Obsługę pola `no_available_places` należy zrealizować przy pomocy triggerów
- podczas dodawania rezerwacji trigger powinien aktualizować pole `no_available_places` w tabeli trip
- podobnie, podczas zmiany statusu rezerwacji
- należy przygotować trigger/triggery oraz jeśli jest to potrzebne, zaktualizować procedury modyfikujące dane oraz widoki


>UWAGA
Należy stworzyć nowe wersje tych widoków/procedur/triggerów (np. dodając do nazwy dopisek 6b - od numeru zadania). Poprzednie wersje procedur należy pozostawić w celu  umożliwienia weryfikacji ich poprawności. 
- może  być potrzebne wyłączenie 'poprzednich wersji' triggerów 



## Zadanie 6b  - rozwiązanie
# Triggery
## Trigger 1

```sql
create or replace trigger tr_reservation_aiu_6b  
after insert or update on reservation  
for each row  
declare  
  v_change number := 0;  
begin  
  -- Nowa rezerwacja (insert)  
  if inserting then  
    if :new.status in ('N', 'P') then  
      update trip  
      set no_available_places = no_available_places - :new.no_tickets  
      where trip_id = :new.trip_id;  
    end if;  
  
  -- Zmiana rezerwacji (update)  
  elsif updating then  
    -- Zmiana liczby biletów lub statusu aktywnego  
    if :old.status in ('N', 'P') then  
      v_change := v_change + :old.no_tickets;  
    end if;  
  
    if :new.status in ('N', 'P') then  
      v_change := v_change - :new.no_tickets;  
    end if;  
  
    update trip  
    set no_available_places = no_available_places + v_change  
    where trip_id = :new.trip_id;  
  end if;  
end;  
/  
  
  
create or replace trigger tr_reservation_ad_6b  
after delete on reservation  
for each row  
begin  
  if :old.status in ('N', 'P') then  
    update trip  
    set no_available_places = no_available_places + :old.no_tickets  
    where trip_id = :old.trip_id;  
  end if;  
end;  
/

insert into reservation (reservation_id, trip_id, person_id, no_tickets, status)  
values (998, 4, 5, 5, 'N');


```

## Trigger 2

```sql
create or replace trigger tr_reservation_ad_6b  
after delete on reservation  
for each row  
begin  
  if :old.status in ('N', 'P') then  
    update trip  
    set no_available_places = no_available_places + :old.no_tickets  
    where trip_id = :old.trip_id;  
  end if;  
end;  
/


delete from reservation  
where reservation_id = 998;

update reservation  
set status = 'P'  
where reservation_id = 9;
```
# Procedury
## Procedura 1
```sql
create or replace procedure p_add_reservation6b (
  p_trip_id     in trip.trip_id%type,
  p_person_id   in person.person_id%type,
  p_no_tickets  in reservation.no_tickets%type
)
as
  v_available_places number;
  v_trip_date        date;
  v_reservation_id   reservation.reservation_id%type;
begin
  select no_available_places, trip_date
  into v_available_places, v_trip_date
  from trip
  where trip_id = p_trip_id;

  if v_trip_date <= sysdate then
    raise_application_error(-20001, 'Wycieczka już się odbyła.');
  end if;

  if v_available_places < p_no_tickets then
    raise_application_error(-20002, 'Brak dostępnych miejsc.');
  end if;

  -- Wstawienie i pobranie ID
  insert into reservation (trip_id, person_id, no_tickets, status)
  values (p_trip_id, p_person_id, p_no_tickets, 'N')
  returning reservation_id into v_reservation_id;

  -- Log wpisu
  insert into log (reservation_id, log_date, status, no_tickets)
  values (v_reservation_id, sysdate, 'N', p_no_tickets);

exception
  when no_data_found then
    raise_application_error(-20010, 'Nie znaleziono wycieczki o podanym ID.');
  when too_many_rows then
    raise_application_error(-20011, 'Znaleziono więcej niż jedną wycieczkę — błąd danych.');
  when others then
    raise_application_error(-20999, 'Nieoczekiwany błąd: ' || sqlerrm);
end;
/

begin  
  p_add_reservation6b(  
    p_trip_id    => 4,  
    p_person_id  => 5,  
    p_no_tickets => 2  
  );  
end;
```

## Procedura 2
```sql
create or replace procedure p_modify_reservation_status6b (  
  p_reservation_id in reservation.reservation_id%type,  
  p_status         in reservation.status%type  
)  
as  
  v_trip_id        reservation.trip_id%type;  
  v_no_tickets     reservation.no_tickets%type;  
  v_current_status reservation.status%type;  
  v_available      number;
begin  
  -- Pobranie danych rezerwacji
  begin  
    select trip_id, no_tickets, status  
    into v_trip_id, v_no_tickets, v_current_status  
    from reservation  
    where reservation_id = p_reservation_id;  
  exception  
    when no_data_found then  
      raise_application_error(-20099, 'Rezerwacja o podanym ID nie istnieje.');  
    when too_many_rows then
      raise_application_error(-20100, 'Znaleziono więcej niż jedną rezerwację o tym ID.');
  end;  
  
  -- Sprawdzenie, czy można przywrócić anulowaną rezerwację
  if v_current_status = 'C' and p_status in ('N', 'P') then  
    begin
      select no_available_places  
      into v_available  
      from trip  
      where trip_id = v_trip_id;

      if v_available < v_no_tickets then  
        raise_application_error(-20003, 'Brak wolnych miejsc na przywrócenie rezerwacji.');  
      end if;
    exception
      when no_data_found then
        raise_application_error(-20101, 'Wycieczka powiązana z rezerwacją nie istnieje.');
      when too_many_rows then
        raise_application_error(-20102, 'Znaleziono więcej niż jedną wycieczkę o tym ID.');
    end;
  end if;  
  
  -- Aktualizacja statusu
  update reservation  
  set status = p_status  
  where reservation_id = p_reservation_id;  
  
  -- Dodanie wpisu do logu
  insert into log (
    reservation_id, log_date, status, no_tickets
  ) values (
    p_reservation_id, sysdate, p_status, v_no_tickets
  );  

exception
  when others then
    raise_application_error(-20999, 'Nieoczekiwany błąd: ' || sqlerrm);
end;  
/


begin  
  p_modify_reservation_status6b(  
    p_reservation_id => 1000,  
    p_status         => 'P'  
  );  
end;
```

## Procedura 3
```sql
create or replace procedure p_modify_reservation6b (  
  p_reservation_id in reservation.reservation_id%type,  
  p_no_tickets     in reservation.no_tickets%type  
)  
as  
  v_old_tickets reservation.no_tickets%type;  
  v_trip_id     reservation.trip_id%type;  
  v_status      reservation.status%type;  
  v_available   number;  
begin  
  -- Pobranie danych rezerwacji
  begin  
    select no_tickets, trip_id, status  
    into v_old_tickets, v_trip_id, v_status  
    from reservation  
    where reservation_id = p_reservation_id;  
  exception  
    when no_data_found then  
      raise_application_error(-20098, 'Rezerwacja o podanym ID nie istnieje.');  
    when too_many_rows then
      raise_application_error(-20097, 'Znaleziono więcej niż jedną rezerwację o tym ID.');
  end;  
  
  -- Sprawdzenie dostępności miejsc przy zwiększeniu liczby biletów
  if p_no_tickets > v_old_tickets and v_status in ('N', 'P') then  
    begin
      select no_available_places  
      into v_available  
      from trip  
      where trip_id = v_trip_id;

      if v_available < (p_no_tickets - v_old_tickets) then  
        raise_application_error(-20004, 'Brak wolnych miejsc na zwiększenie liczby biletów.');  
      end if;
    exception
      when no_data_found then
        raise_application_error(-20096, 'Nie znaleziono wycieczki powiązanej z rezerwacją.');
      when too_many_rows then
        raise_application_error(-20095, 'Znaleziono więcej niż jedną wycieczkę o tym ID.');
    end;
  end if;  
  
  -- Aktualizacja liczby biletów w rezerwacji
  update reservation  
  set no_tickets = p_no_tickets  
  where reservation_id = p_reservation_id;  
  
  -- Dodanie nowego wpisu do logu
  insert into log (
    reservation_id, log_date, status, no_tickets
  ) values (
    p_reservation_id, sysdate, v_status, p_no_tickets
  );  

exception
  when others then
    raise_application_error(-20999, 'Nieoczekiwany błąd: ' || sqlerrm);
end;  
/

begin  
  p_modify_reservation6b(  
    p_reservation_id => 1000,  
    p_no_tickets     => 3  
  );  
end;  
/
```

## Procedura 4
```sql
CREATE OR REPLACE PROCEDURE p_modify_max_no_places6b (
  p_trip_id       IN trip.trip_id%TYPE,
  p_max_no_places IN trip.max_no_places%TYPE
)
AS
  v_reserved NUMBER;
BEGIN
  -- Suma aktywnych rezerwacji
  SELECT NVL(SUM(no_tickets), 0)
  INTO v_reserved
  FROM reservation
  WHERE trip_id = p_trip_id
    AND status IN ('N', 'P');

  -- Walidacja nowej wartości
  IF p_max_no_places < v_reserved THEN
    RAISE_APPLICATION_ERROR(-20005, 'Liczba miejsc nie może być mniejsza niż liczba rezerwacji.');
  END IF;

  -- Aktualizacja
  UPDATE trip
  SET max_no_places = p_max_no_places
  WHERE trip_id = p_trip_id;


EXCEPTION
  WHEN NO_DATA_FOUND THEN
    RAISE_APPLICATION_ERROR(-20006, 'Nie znaleziono wycieczki.');
  WHEN OTHERS THEN
    RAISE_APPLICATION_ERROR(-20999, 'Nieoczekiwany błąd: ' || SQLERRM);
END;
/

begin  
  p_modify_max_no_places6b(  
    p_trip_id        => 4,  
    p_max_no_places  => 20  
  );  
end;  
/
```
# Widoki
## Widok 1
```sql
create or replace view vw_reservation6b as  
select  
  r.reservation_id,  
  t.country,  
  t.trip_date,  
  t.trip_name,  
  p.firstname,  
  p.lastname,  
  r.status,  
  r.trip_id,  
  r.person_id,  
  r.no_tickets  
from reservation r  
join trip t on r.trip_id = t.trip_id  
join person p on r.person_id = p.person_id;

select * from vw_reservation6b;
```
## Widok 2
```sql
create or replace view vw_trip6b as
select
  trip_id,
  country,
  trip_date,
  trip_name,
  max_no_places,
  no_available_places
from trip;

select *
from vw_trip6b;

```
## Widok 3
```sql
create or replace view vw_available_trip6b as  
select *  
from vw_trip6b  
where trip_date > sysdate  
  and no_available_places > 0;

select * from vw_available_trip6b;
```
# Zadanie 7 - podsumowanie

Porównaj sposób programowania w systemie Oracle PL/SQL ze znanym ci systemem/językiem MS Sqlserver T-SQL

```
Podczas realizacji zestawu zadań w Oracle PL/SQL, który wykonywaliśmy we dwójkę, zauważyliśmy kilka istotnych różnic między tym środowiskiem a znanym nam wcześniej językiem T-SQL stosowanym w MS SQL Server.

Pierwszą i dość zauważalną różnicą jest sposób zarządzania transakcjami. W Oracle transakcja rozpoczyna się automatycznie po wykonaniu pierwszej operacji DML (np. `INSERT`, `UPDATE`, `DELETE`) i trwa do momentu jawnego `COMMIT`lub `ROLLBACK`. W SQL Server domyślnie działa tryb autocommit — każda operacja jest od razu zatwierdzana, a manualne rozpoczęcie transakcji wymaga użycia `BEGIN TRANSACTION`.

Kolejną różnicą jest obsługa wyjątków. W Oracle zastosowanie bloku `EXCEPTION` pozwala przechwytywać i obsługiwać konkretne błędy w sposób bardziej precyzyjny. W SQL Server podobną funkcję pełni blok `TRY...CATCH`, jednak jego możliwości są nieco bardziej ograniczone, zwłaszcza w zakresie rozróżniania typów błędów.

Istotnym elementem, który doceniliśmy w Oracle, jest możliwość definiowania własnych typów obiektowych oraz funkcji zwracających kolekcje obiektów. Również mechanizm triggerów w Oracle jest bardzo rozbudowany i dobrze zintegrowany z funkcjami i procedurami. W naszym przypadku pozwoliło to przenieść kontrolę dostępności miejsc oraz logowanie zmian bezpośrednio do triggerów, co znacząco uprościło kod procedur. W SQL Server również można używać triggerów, ale ich integracja z funkcjami użytkownika i mechanizmami kontroli danych nie jest tak naturalna.

Zwróciliśmy również uwagę na różnicę w sposobie generowania automatycznych wartości kluczy głównych. W SQL Server korzysta się najczęściej z pól typu `IDENTITY`, które są ściśle powiązane z tabelą. W Oracle używa się sekwencji (`SEQUENCE`), które są niezależnymi obiektami i mogą być wykorzystywane w wielu miejscach — co daje większą elastyczność, ale wymaga jawnego przypisywania wartości (chyba że zdefiniuje się domyślną wartość w kolumnie).

Podsumowując, Oracle PL/SQL oferuje większe możliwości w zakresie budowania zaawansowanej logiki po stronie bazy danych, ale wiąże się to z bardziej rozbudowaną składnią i większym naciskiem na poprawność typów oraz struktur danych. T-SQL jest prostszy w obsłudze i lepszy dla prostych operacji, jednak mniej elastyczny przy bardziej złożonych rozwiązaniach.
