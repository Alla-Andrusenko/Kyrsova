using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace ParkingInterface.Data
{
    public static class DataAccess
    {
        private static readonly string DatabaseFileName = "ParkingDatabase.db";
        private static readonly string DatabaseFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, DatabaseFileName);
        private static readonly string ConnectionString = $"Data Source={DatabaseFilePath};Version=3;";

        public static void InitializeDatabase()
        {
            bool isNewDatabase = !File.Exists(DatabaseFilePath);

            if (isNewDatabase)
            {
                SQLiteConnection.CreateFile(DatabaseFilePath);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                // Увімкнення підтримки зовнішніх ключів
                using (var command = new SQLiteCommand("PRAGMA foreign_keys = ON;", connection))
                {
                    command.ExecuteNonQuery();
                }

                if (isNewDatabase)
                {
                    CreateTables(connection);
                }
            }
        }

        private static void CreateTables(SQLiteConnection connection)
        {
            using (var command = new SQLiteCommand(connection))
            {
                // Таблиця клієнтів
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Clients (
                        ClientId INTEGER PRIMARY KEY AUTOINCREMENT,
                        LastName TEXT NOT NULL,
                        FirstName TEXT NOT NULL,
                        MiddleName TEXT NOT NULL,
                        PhoneNumber TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();

                // Таблиця автомобілів з каскадним видаленням
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Cars (
                        CarId INTEGER PRIMARY KEY AUTOINCREMENT,
                        LicensePlate TEXT NOT NULL,
                        Brand TEXT NOT NULL,
                        Model TEXT NOT NULL,
                        ClientId INTEGER NOT NULL,
                        FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // Таблиця договорів з каскадним видаленням
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Contracts (
                        ContractId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ClientId INTEGER NOT NULL,
                        StartDate TEXT NOT NULL,
                        EndDate TEXT NOT NULL,
                        Amount REAL NOT NULL,
                        FOREIGN KEY (ClientId) REFERENCES Clients(ClientId) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // Таблиця паркувальних місць
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS ParkingSpots (
                        SpotId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ContractId INTEGER,
                        IsOccupied INTEGER NOT NULL,
                        FOREIGN KEY (ContractId) REFERENCES Contracts(ContractId) ON DELETE SET NULL
                    );";
                command.ExecuteNonQuery();

                // Таблиця платежів з каскадним видаленням
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Payments (
                        PaymentId INTEGER PRIMARY KEY AUTOINCREMENT,
                        ContractId INTEGER NOT NULL,
                        Amount REAL NOT NULL,
                        PaymentDate TEXT NOT NULL,
                        FOREIGN KEY (ContractId) REFERENCES Contracts(ContractId) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // Таблиця тарифів
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Tariffs (
                        TariffId INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Price REAL NOT NULL,
                        Duration TEXT NOT NULL
                    );";
                command.ExecuteNonQuery();

                // Таблиця записів проїзду з каскадним видаленням
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS EntranceRecords (
                        RecordId INTEGER PRIMARY KEY AUTOINCREMENT,
                        CarId INTEGER NOT NULL,
                        EntryTime TEXT NOT NULL,
                        ExitTime TEXT,
                        FOREIGN KEY (CarId) REFERENCES Cars(CarId) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();

                // Таблиця допусків з каскадним видаленням
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Admissions (
                        AdmissionId INTEGER PRIMARY KEY AUTOINCREMENT,
                        CarId INTEGER NOT NULL,
                        IsAllowed INTEGER NOT NULL,
                        FOREIGN KEY (CarId) REFERENCES Cars(CarId) ON DELETE CASCADE
                    );";
                command.ExecuteNonQuery();
            }
        }

        #region Clients

        public static List<Client> GetClients()
        {
            var clients = new List<Client>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Clients;";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            clients.Add(new Client
                            {
                                ClientId = Convert.ToInt32(reader["ClientId"]),
                                LastName = reader["LastName"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                MiddleName = reader["MiddleName"].ToString(),
                                PhoneNumber = reader["PhoneNumber"].ToString()
                            });
                        }
                    }
                }
            }

            return clients;
        }

        public static void InsertClient(Client client)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Clients (LastName, FirstName, MiddleName, PhoneNumber)
                                 VALUES (@LastName, @FirstName, @MiddleName, @PhoneNumber);
                                 SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LastName", client.LastName);
                    command.Parameters.AddWithValue("@FirstName", client.FirstName);
                    command.Parameters.AddWithValue("@MiddleName", client.MiddleName);
                    command.Parameters.AddWithValue("@PhoneNumber", client.PhoneNumber);

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        client.ClientId = (int)id;
                    }
                }
            }
        }

        public static void UpdateClient(Client client)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE Clients SET
                                 LastName = @LastName,
                                 FirstName = @FirstName,
                                 MiddleName = @MiddleName,
                                 PhoneNumber = @PhoneNumber
                                 WHERE ClientId = @ClientId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LastName", client.LastName);
                    command.Parameters.AddWithValue("@FirstName", client.FirstName);
                    command.Parameters.AddWithValue("@MiddleName", client.MiddleName);
                    command.Parameters.AddWithValue("@PhoneNumber", client.PhoneNumber);
                    command.Parameters.AddWithValue("@ClientId", client.ClientId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteClient(int clientId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM Clients WHERE ClientId = @ClientId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", clientId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Cars

        public static List<Car> GetCars()
        {
            var cars = new List<Car>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
                SELECT Cars.*, Clients.FirstName, Clients.LastName
                FROM Cars
                JOIN Clients ON Cars.ClientId = Clients.ClientId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            cars.Add(new Car
                            {
                                CarId = Convert.ToInt32(reader["CarId"]),
                                LicensePlate = reader["LicensePlate"].ToString(),
                                Brand = reader["Brand"].ToString(),
                                Model = reader["Model"].ToString(),
                                ClientId = Convert.ToInt32(reader["ClientId"]),
                                Client = new Client
                                {
                                    ClientId = Convert.ToInt32(reader["ClientId"]),
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString()
                                }
                            });
                        }
                    }
                }
            }

            return cars;
        }

        public static void InsertCar(Car car)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Cars (LicensePlate, Brand, Model, ClientId)
                                 VALUES (@LicensePlate, @Brand, @Model, @ClientId);
                                 SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LicensePlate", car.LicensePlate);
                    command.Parameters.AddWithValue("@Brand", car.Brand);
                    command.Parameters.AddWithValue("@Model", car.Model);
                    command.Parameters.AddWithValue("@ClientId", car.ClientId);

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        car.CarId = (int)id;
                    }
                }
            }
        }

        public static void UpdateCar(Car car)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE Cars SET
                                 LicensePlate = @LicensePlate,
                                 Brand = @Brand,
                                 Model = @Model,
                                 ClientId = @ClientId
                                 WHERE CarId = @CarId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@LicensePlate", car.LicensePlate);
                    command.Parameters.AddWithValue("@Brand", car.Brand);
                    command.Parameters.AddWithValue("@Model", car.Model);
                    command.Parameters.AddWithValue("@ClientId", car.ClientId);
                    command.Parameters.AddWithValue("@CarId", car.CarId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteCar(int carId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM Cars WHERE CarId = @CarId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CarId", carId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Contracts

        public static List<Contract> GetContracts()
        {
            var contracts = new List<Contract>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            SELECT Contracts.*, Clients.FirstName, Clients.LastName
            FROM Contracts
            JOIN Clients ON Contracts.ClientId = Clients.ClientId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var client = new Client
                            {
                                ClientId = Convert.ToInt32(reader["ClientId"]),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString()
                            };

                            contracts.Add(new Contract
                            {
                                ContractId = Convert.ToInt32(reader["ContractId"]),
                                ClientId = Convert.ToInt32(reader["ClientId"]),
                                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                EndDate = DateTime.Parse(reader["EndDate"].ToString()),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                Client = client
                            });
                        }
                    }
                }
            }

            return contracts;
        }

        public static void InsertContract(Contract contract)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Contracts (ClientId, StartDate, EndDate, Amount)
                                VALUES (@ClientId, @StartDate, @EndDate, @Amount);
                                SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", contract.ClientId);
                    command.Parameters.AddWithValue("@StartDate", contract.StartDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@EndDate", contract.EndDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Amount", contract.Amount);

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        contract.ContractId = (int)id;
                    }
                }
            }
        }

        public static void UpdateContract(Contract contract)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE Contracts SET
                                ClientId = @ClientId,
                                StartDate = @StartDate,
                                EndDate = @EndDate,
                                Amount = @Amount
                                WHERE ContractId = @ContractId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ClientId", contract.ClientId);
                    command.Parameters.AddWithValue("@StartDate", contract.StartDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@EndDate", contract.EndDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@Amount", contract.Amount);
                    command.Parameters.AddWithValue("@ContractId", contract.ContractId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteContract(int contractId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM Contracts WHERE ContractId = @ContractId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", contractId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region ParkingSpots

        public static List<ParkingSpot> GetParkingSpots()
        {
            var spots = new List<ParkingSpot>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            SELECT ParkingSpots.*, Contracts.StartDate, Contracts.EndDate, Contracts.Amount
            FROM ParkingSpots
            LEFT JOIN Contracts ON ParkingSpots.ContractId = Contracts.ContractId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Contract contract = null;
                            if (reader["ContractId"] != DBNull.Value)
                            {
                                contract = new Contract
                                {
                                    ContractId = Convert.ToInt32(reader["ContractId"]),
                                    StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                    EndDate = DateTime.Parse(reader["EndDate"].ToString()),
                                    Amount = Convert.ToDecimal(reader["Amount"])
                                };
                            }

                            spots.Add(new ParkingSpot
                            {
                                SpotId = Convert.ToInt32(reader["SpotId"]),
                                ContractId = reader["ContractId"] != DBNull.Value ? (int?)Convert.ToInt32(reader["ContractId"]) : null,
                                IsOccupied = Convert.ToBoolean(reader["IsOccupied"]),
                                Contract = contract
                            });
                        }
                    }
                }
            }

            return spots;
        }

        public static void InsertParkingSpot(ParkingSpot spot)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO ParkingSpots (ContractId, IsOccupied)
                        VALUES (@ContractId, @IsOccupied);
                        SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", (object)spot.ContractId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IsOccupied", spot.IsOccupied ? 1 : 0);

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        spot.SpotId = (int)id;
                    }
                }
            }
        }

        public static void UpdateParkingSpot(ParkingSpot spot)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE ParkingSpots SET
                                ContractId = @ContractId,
                                IsOccupied = @IsOccupied
                                WHERE SpotId = @SpotId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", (object)spot.ContractId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IsOccupied", spot.IsOccupied ? 1 : 0);
                    command.Parameters.AddWithValue("@SpotId", spot.SpotId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteParkingSpot(int spotId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM ParkingSpots WHERE SpotId = @SpotId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@SpotId", spotId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Payments

        public static List<Payment> GetPayments()
        {
            var payments = new List<Payment>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            SELECT Payments.*, Contracts.StartDate, Contracts.EndDate, Contracts.Amount AS ContractAmount
            FROM Payments
            JOIN Contracts ON Payments.ContractId = Contracts.ContractId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var contract = new Contract
                            {
                                ContractId = Convert.ToInt32(reader["ContractId"]),
                                StartDate = DateTime.Parse(reader["StartDate"].ToString()),
                                EndDate = DateTime.Parse(reader["EndDate"].ToString()),
                                Amount = Convert.ToDecimal(reader["ContractAmount"])
                            };

                            payments.Add(new Payment
                            {
                                PaymentId = Convert.ToInt32(reader["PaymentId"]),
                                ContractId = Convert.ToInt32(reader["ContractId"]),
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                PaymentDate = DateTime.Parse(reader["PaymentDate"].ToString()),
                                Contract = contract
                            });
                        }
                    }
                }
            }

            return payments;
        }

        public static void InsertPayment(Payment payment)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Payments (ContractId, Amount, PaymentDate)
                                VALUES (@ContractId, @Amount, @PaymentDate);
                                SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", payment.ContractId);
                    command.Parameters.AddWithValue("@Amount", payment.Amount);
                    command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate.ToString("yyyy-MM-dd"));

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        payment.PaymentId = (int)id;
                    }
                }
            }
        }

        public static void UpdatePayment(Payment payment)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE Payments SET
                                ContractId = @ContractId,
                                Amount = @Amount,
                                PaymentDate = @PaymentDate
                                WHERE PaymentId = @PaymentId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@ContractId", payment.ContractId);
                    command.Parameters.AddWithValue("@Amount", payment.Amount);
                    command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate.ToString("yyyy-MM-dd"));
                    command.Parameters.AddWithValue("@PaymentId", payment.PaymentId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeletePayment(int paymentId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM Payments WHERE PaymentId = @PaymentId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@PaymentId", paymentId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Tariffs

        public static List<Tariff> GetTariffs()
        {
            var tariffs = new List<Tariff>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "SELECT * FROM Tariffs;";
                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tariffs.Add(new Tariff
                            {
                                TariffId = Convert.ToInt32(reader["TariffId"]),
                                Name = reader["Name"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Duration = TimeSpan.Parse(reader["Duration"].ToString())
                            });
                        }
                    }
                }
            }

            return tariffs;
        }

        public static void InsertTariff(Tariff tariff)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Tariffs (Name, Price, Duration)
                                VALUES (@Name, @Price, @Duration);
                                SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", tariff.Name);
                    command.Parameters.AddWithValue("@Price", tariff.Price);
                    command.Parameters.AddWithValue("@Duration", tariff.Duration.ToString());

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        tariff.TariffId = (int)id;
                    }
                }
            }
        }

        public static void UpdateTariff(Tariff tariff)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE Tariffs SET
                                Name = @Name,
                                Price = @Price,
                                Duration = @Duration
                                WHERE TariffId = @TariffId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", tariff.Name);
                    command.Parameters.AddWithValue("@Price", tariff.Price);
                    command.Parameters.AddWithValue("@Duration", tariff.Duration.ToString());
                    command.Parameters.AddWithValue("@TariffId", tariff.TariffId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteTariff(int tariffId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM Tariffs WHERE TariffId = @TariffId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@TariffId", tariffId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region EntranceRecords

        public static List<EntranceRecord> GetEntranceRecords()
        {
            var records = new List<EntranceRecord>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            SELECT EntranceRecords.*, Cars.LicensePlate, Cars.Brand, Cars.Model
            FROM EntranceRecords
            JOIN Cars ON EntranceRecords.CarId = Cars.CarId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var car = new Car
                            {
                                CarId = Convert.ToInt32(reader["CarId"]),
                                LicensePlate = reader["LicensePlate"].ToString(),
                                Brand = reader["Brand"].ToString(),
                                Model = reader["Model"].ToString()
                            };

                            records.Add(new EntranceRecord
                            {
                                RecordId = Convert.ToInt32(reader["RecordId"]),
                                CarId = Convert.ToInt32(reader["CarId"]),
                                EntryTime = DateTime.Parse(reader["EntryTime"].ToString()),
                                ExitTime = reader["ExitTime"] != DBNull.Value ? (DateTime?)DateTime.Parse(reader["ExitTime"].ToString()) : null,
                                Car = car
                            });
                        }
                    }
                }
            }

            return records;
        }

        public static void InsertEntranceRecord(EntranceRecord record)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO EntranceRecords (CarId, EntryTime, ExitTime)
                                VALUES (@CarId, @EntryTime, @ExitTime);
                                SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CarId", record.CarId);
                    command.Parameters.AddWithValue("@EntryTime", record.EntryTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ExitTime", record.ExitTime.HasValue ? (object)record.ExitTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value);

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        record.RecordId = (int)id;
                    }
                }
            }
        }

        public static void UpdateEntranceRecord(EntranceRecord record)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE EntranceRecords SET
                                CarId = @CarId,
                                EntryTime = @EntryTime,
                                ExitTime = @ExitTime
                                WHERE RecordId = @RecordId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CarId", record.CarId);
                    command.Parameters.AddWithValue("@EntryTime", record.EntryTime.ToString("yyyy-MM-dd HH:mm:ss"));
                    command.Parameters.AddWithValue("@ExitTime", record.ExitTime.HasValue ? (object)record.ExitTime.Value.ToString("yyyy-MM-dd HH:mm:ss") : DBNull.Value);
                    command.Parameters.AddWithValue("@RecordId", record.RecordId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteEntranceRecord(int recordId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM EntranceRecords WHERE RecordId = @RecordId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RecordId", recordId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion

        #region Admissions

        public static List<Admission> GetAdmissions()
        {
            var admissions = new List<Admission>();

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"
            SELECT Admissions.*, Cars.LicensePlate, Cars.Brand, Cars.Model
            FROM Admissions
            JOIN Cars ON Admissions.CarId = Cars.CarId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var car = new Car
                            {
                                CarId = Convert.ToInt32(reader["CarId"]),
                                LicensePlate = reader["LicensePlate"].ToString(),
                                Brand = reader["Brand"].ToString(),
                                Model = reader["Model"].ToString()
                            };

                            admissions.Add(new Admission
                            {
                                AdmissionId = Convert.ToInt32(reader["AdmissionId"]),
                                CarId = Convert.ToInt32(reader["CarId"]),
                                IsAllowed = Convert.ToBoolean(reader["IsAllowed"]),
                                Car = car
                            });
                        }
                    }
                }
            }

            return admissions;
        }

        public static void InsertAdmission(Admission admission)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"INSERT INTO Admissions (CarId, IsAllowed)
                                VALUES (@CarId, @IsAllowed);
                                SELECT last_insert_rowid();";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CarId", admission.CarId);
                    command.Parameters.AddWithValue("@IsAllowed", admission.IsAllowed ? 1 : 0);

                    var result = command.ExecuteScalar();
                    if (result != null && long.TryParse(result.ToString(), out long id))
                    {
                        admission.AdmissionId = (int)id;
                    }
                }
            }
        }

        public static void UpdateAdmission(Admission admission)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = @"UPDATE Admissions SET
                                CarId = @CarId,
                                IsAllowed = @IsAllowed
                                WHERE AdmissionId = @AdmissionId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@CarId", admission.CarId);
                    command.Parameters.AddWithValue("@IsAllowed", admission.IsAllowed ? 1 : 0);
                    command.Parameters.AddWithValue("@AdmissionId", admission.AdmissionId);

                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DeleteAdmission(int admissionId)
        {
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string query = "DELETE FROM Admissions WHERE AdmissionId = @AdmissionId;";

                using (var command = new SQLiteCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@AdmissionId", admissionId);
                    command.ExecuteNonQuery();
                }
            }
        }

        #endregion
    }
}