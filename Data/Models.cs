using System;

namespace ParkingInterface.Data
{
    public class Client
    {
        public int ClientId { get; set; } // Первинний ключ
        public string LastName { get; set; } // Прізвище
        public string FirstName { get; set; } // Ім'я
        public string MiddleName { get; set; } // По батькові
        public string PhoneNumber { get; set; } // Номер телефону
    }

    public class Car
    {
        public int CarId { get; set; } // Первинний ключ
        public string LicensePlate { get; set; } // Державний номер
        public string Brand { get; set; } // Марка
        public string Model { get; set; } // Модель
        public int ClientId { get; set; } // Зовнішній ключ до клієнта

        // Пов'язаний клієнт
        public Client Client { get; set; }
    }

    public class Contract
    {
        public int ContractId { get; set; } // Первинний ключ
        public int ClientId { get; set; } // Зовнішній ключ до клієнта
        public DateTime StartDate { get; set; } // Дата початку
        public DateTime EndDate { get; set; } // Дата завершення
        public decimal Amount { get; set; } // Сума

        public Client Client { get; set; }
        public string ClientName
        {
            get
            {
                return Client != null ? $"{Client.FirstName} {Client.LastName}" : "";
            }
        }
    }

    public class ParkingSpot
    {
        public int SpotId { get; set; } // Первинний ключ
        public int? ContractId { get; set; } // Зовнішній ключ до договору (може бути NULL)
        public bool IsOccupied { get; set; } // Чи зайнято місце

        // Пов'язаний договір
        public Contract Contract { get; set; }

        // Для відображення інформації про договір
        public string ContractInfo
        {
            get
            {
                return Contract != null ? $"Договір №{Contract.ContractId}" : "Незайняте";
            }
        }
    }

    public class Payment
    {
        public int PaymentId { get; set; } // Первинний ключ
        public int ContractId { get; set; } // Зовнішній ключ до договору
        public decimal Amount { get; set; } // Сума
        public DateTime PaymentDate { get; set; } // Дата платежу

        // Пов'язаний договір
        public Contract Contract { get; set; }

        // Для відображення інформації про договір
        public string ContractInfo
        {
            get
            {
                return Contract != null ? $"Договір №{Contract.ContractId}" : "";
            }
        }
    }

    public class Tariff
    {
        public int TariffId { get; set; } // Первинний ключ
        public string Name { get; set; } // Назва тарифу
        public decimal Price { get; set; } // Ціна
        public TimeSpan Duration { get; set; } // Тривалість
    }

    public class EntranceRecord
    {
        public int RecordId { get; set; } // Первинний ключ
        public int CarId { get; set; } // Зовнішній ключ до автомобіля
        public DateTime EntryTime { get; set; } // Час в'їзду
        public DateTime? ExitTime { get; set; } // Час виїзду (може бути NULL)

        // Пов'язаний автомобіль
        public Car Car { get; set; }

        // Для відображення інформації про автомобіль
        public string CarInfo
        {
            get
            {
                return Car != null ? $"{Car.LicensePlate} ({Car.Brand} {Car.Model})" : "";
            }
        }
    }

    public class Admission
    {
        public int AdmissionId { get; set; } // Первинний ключ
        public int CarId { get; set; } // Зовнішній ключ до автомобіля
        public bool IsAllowed { get; set; } // Чи дозволено в'їзд

        // Пов'язаний автомобіль
        public Car Car { get; set; }

        // Для відображення інформації про автомобіль
        public string CarInfo
        {
            get
            {
                return Car != null ? $"{Car.LicensePlate} ({Car.Brand} {Car.Model})" : "";
            }
        }
    }
}