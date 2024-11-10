using System.Data.SqlClient;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Entities.Database;
using InactivePassports.Data.Entities.WebAPI;
using Dapper;

namespace InactivePassports.Services.Repositories
{
    public class WebAPIRepository : IWebAPI
    {
        private readonly string _connectionString;

        public WebAPIRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<bool> FindPassport(PassportRequest seriesNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT COUNT(1) FROM Passports WHERE Series = @Series AND Number = @Number AND Status = @Status";

                var count = await connection.ExecuteScalarAsync<int>(query, new 
                {
                    Series = seriesNumber.Series,
                    Number = seriesNumber.Number,
                    Status = 0 
                });

                return count > 0;
            }
        }

        public async Task<Operation[]> GetPassportHistory(PassportRequest seriesNumber)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                string query = @"
                    SELECT o.Passport, o.Date, o.Action
                    FROM Operations o
                    INNER JOIN Passports p ON o.Passport = p.Id
                    WHERE p.Series = @Series AND p.Number = @Number";

                var operations = await connection.QueryAsync<Operation>(query, new 
                {
                    Series = seriesNumber.Series, 
                    Number = seriesNumber.Number 
                });

                return operations.ToArray();
            }
        }

        public async Task<DateHistoryResult[]> GetDateHistory(DateTime startDate, DateTime endDate)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT o.Date, p.Id AS PassportId, p.Series, p.Number, o.Action
                    FROM Operations o
                    INNER JOIN Passports p ON o.Passport = p.Id
                    WHERE o.Date >= @StartDate AND o.Date <= @EndDate
                    ORDER BY o.Date";
                
                var result = await connection.QueryAsync(query, new
                {
                    StartDate = startDate,
                    EndDate = endDate
                });
                
                var groupedResult = result
                    .GroupBy(r => DateOnly.FromDateTime(r.Date))
                    .Select(g => new DateHistoryResult
                    {
                        Date = g.Key,
                        PassportOperation = g.Select(r => new PassportOperation
                        {
                            Id = r.PassportId,
                            Series = r.Series,
                            Number = r.Number,
                            Action = r.Action
                        }).ToArray()
                    })
                    .ToArray();

                return groupedResult;
            }
        }
    }
}