using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ShopDB
{
    class Program
    {
        private static readonly string _connectionString = @"Server=DESKTOP-4AK6IC5\SQLEXPRESS;Database=Shop;Trusted_Connection=True;";

        static void Main(string[] args)
        {
            Console.WriteLine("| {0, 30} | {1, 12} | {2, 12} |", "Customer Name", "Orders Count", "Total Price");
            Console.WriteLine("|--------------------------------------------------------------|");
            List<Statistics> statistics = GetStatistics();
            foreach (Statistics statisticsLine in statistics)
            {
                Console.WriteLine("| {0, 30} | {1, 12} | {2, 12} |", statisticsLine.Name, statisticsLine.OrdersCount, statisticsLine.TotalPrice);
            }
            Console.WriteLine("|--------------------------------------------------------------|");
        }

        private static List<Customer> ReadCustomers()
        {
            List<Customer> customers = new();
            using (SqlConnection connection = new(_connectionString))
            { 
                connection.Open();
                using SqlCommand command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT
                        [Id],
                        [Name],
                        [City]
                    FROM [Customer]";
                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Customer customer = new()
                    {
                        Id = (int)reader["Id"],
                        Name = (string)reader["Name"],
                        City = (string)reader["City"]
                    };
                    customers.Add(customer);
                }
            }

            return customers;
        }

        private static int InsertCustomer(string name, string city)
        {
            using (SqlConnection connection = new(_connectionString))
            { 
                connection.Open();
                using SqlCommand command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO [Customer] 
                        ([Name], 
                         [City]) 
                    VALUES 
                        (@name, 
                         @city) 
                    SELECT SCOPE_IDENTITY()";
                command.Parameters.Add("@name", SqlDbType.VarChar).Value = name;
                command.Parameters.Add("@city", SqlDbType.VarChar).Value = city;

                return (int)command.ExecuteScalar();
            }
        }

        private static List<Statistics> GetStatistics()
        {
            List<Statistics> statistics = new();
            using (SqlConnection connection = new(_connectionString))
            {
                connection.Open();
                using SqlCommand command = connection.CreateCommand();
                command.CommandText = @"
                    SELECT [Customer].[Name], 
	                       COUNT([Order].[Id]) as OrdersCount, 
	                       SUM([Order].[Price]) as TotalPrice 
                           FROM [Customer] 
                    LEFT JOIN [Order] 
	                       ON ([Customer].[Id] = [Order].[CustomerId]) 
                    GROUP BY [Customer].[Name]";

                using SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Statistics statisticsLine = new()
                    {
                        Name = (string)reader["Name"],
                        OrdersCount = (int)reader["OrdersCount"],
                        TotalPrice = (int)reader["TotalPrice"]
                    };
                    statistics.Add(statisticsLine);
                }
            }

            return statistics;
        }
    }
}
