using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CliCal
{
    public class CalculatorDatabase
    {
        private const string ConnectionString = "Data Source=calculator.db";

        public CalculatorDatabase()
        {
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
            CREATE TABLE IF NOT EXISTS CalculationLog (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Timestamp TEXT NOT NULL,
                Expression TEXT NOT NULL,
                Result REAL NOT NULL,
                Duration INTEGER NOT NULL
            )";

            command.ExecuteNonQuery();
        }

        public async Task LogCalculationAsync(string expression, double result, TimeSpan duration)
        {
            using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            INSERT INTO CalculationLog (Timestamp, Expression, Result, Duration)
            VALUES ($timestamp, $expression, $result, $duration)";

            command.Parameters.AddWithValue("$timestamp", DateTime.UtcNow.ToString("O"));
            command.Parameters.AddWithValue("$expression", expression);
            command.Parameters.AddWithValue("$result", result);
            command.Parameters.AddWithValue("$duration", duration.TotalMilliseconds);

            await command.ExecuteNonQueryAsync();
        }

        public async Task<List<CalculationRecord>> GetRecentCalculationsAsync(int limit = 10)
        {
            using var connection = new SqliteConnection(ConnectionString);
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = @"
            SELECT Timestamp, Expression, Result, Duration
            FROM CalculationLog
            ORDER BY Id DESC
            LIMIT $limit";

            command.Parameters.AddWithValue("$limit", limit);

            var results = new List<CalculationRecord>();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                results.Add(new CalculationRecord(
                    DateTime.Parse(reader.GetString(0)),
                    reader.GetString(1),
                    reader.GetDouble(2),
                    TimeSpan.FromMilliseconds(reader.GetDouble(3))
                ));
            }

            return results;
        }
    }

    public record CalculationRecord(DateTime Timestamp, string Expression, double Result, TimeSpan Duration);

}
