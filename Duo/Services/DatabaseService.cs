using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Duo.Models;

namespace Duo.Services
{
    /// <summary>
    /// Service for handling database operations with SQL Server
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Initialize database and create tables if they don't exist
        /// </summary>
        public async Task InitializeDatabaseAsync()
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                try
                {
                    await connection.OpenAsync();

                    // Create Items table if it doesn't exist
                    string createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Items')
                    BEGIN
                        CREATE TABLE Items (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            Name NVARCHAR(255) NOT NULL
                        )
                    END";

                    using (SqlCommand command = new SqlCommand(createTableQuery, connection))
                    {
                        await command.ExecuteNonQueryAsync();
                    }
                }
                catch (Exception ex)
                {
                    // In a real app, you'd want to log this error or show it to the user
                    Console.WriteLine($"Database initialization error: {ex.Message}");
                    throw;
                }
            }
        }

        /// <summary>
        /// Get all items from the database
        /// </summary>
        public async Task<List<Item>> GetAllItemsAsync()
        {
            List<Item> items = new List<Item>();

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Id, Name FROM Items ORDER BY Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            items.Add(new Item
                            {
                                Id = reader.GetInt32(0),
                                Name = reader.GetString(1)
                            });
                        }
                    }
                }
            }

            return items;
        }

        /// <summary>
        /// Add a new item to the database
        /// </summary>
        public async Task<int> AddItemAsync(Item item)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "INSERT INTO Items (Name) VALUES (@Name); SELECT SCOPE_IDENTITY()";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = item.Name;
                    
                    // Get the new ID
                    var result = await command.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        /// <summary>
        /// Update an existing item in the database
        /// </summary>
        public async Task UpdateItemAsync(Item item)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "UPDATE Items SET Name = @Name WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = item.Id;
                    command.Parameters.Add("@Name", SqlDbType.NVarChar).Value = item.Name;
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
        }

        /// <summary>
        /// Delete an item from the database
        /// </summary>
        public async Task DeleteItemAsync(int id)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "DELETE FROM Items WHERE Id = @Id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    
                    await command.ExecuteNonQueryAsync();
                }
            }
        }
    }
} 