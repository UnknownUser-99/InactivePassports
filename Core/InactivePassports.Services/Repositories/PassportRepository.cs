using System.Data;
using System.Data.SqlClient;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Configurations;
using InactivePassports.Data.Entities.File;
using Dapper;

namespace InactivePassports.Services.Repositories
{
    public class PassportRepository : IPassport
    {
        private readonly RepositoryOptions _options;

        public PassportRepository(RepositoryOptions options)
        {
            _options = options;
        }

        public async Task InsertPassports(Passport[] passports, int id, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.BulkCopyTimeout = _options.Timeout;

                bulkCopy.DestinationTableName = "Passports";

                bulkCopy.ColumnMappings.Add("Id", "Id");
                bulkCopy.ColumnMappings.Add("Series", "Series");
                bulkCopy.ColumnMappings.Add("Number", "Number");

                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("Id", typeof(int));
                dataTable.Columns.Add("Series", typeof(int));
                dataTable.Columns.Add("Number", typeof(int));

                foreach (var passport in passports)
                {
                    dataTable.Rows.Add(id, passport.Series, passport.Number);

                    id++;
                }

                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }

        public async Task<int[]> UpdatePassports(Passport[] passports, ActionType actionType, IDbConnection connection, IDbTransaction transaction)
        {
            DataTable dataTable = new DataTable();

            dataTable.Columns.Add("Series", typeof(int));
            dataTable.Columns.Add("Number", typeof(int));

            for (int i = 0; i < passports.Length; i++)
            {
                dataTable.Rows.Add(passports[i].Series, passports[i].Number);
            }

            string updateQuery = @$"
                UPDATE Passports
                SET Status = {(int)actionType}
                OUTPUT inserted.Id
                FROM Passports p
                INNER JOIN @PassportParam t ON p.Series = t.Series AND p.Number = t.Number;
            ";

            DynamicParameters parameters = new DynamicParameters();

            parameters.Add("@PassportParam", dataTable.AsTableValuedParameter("PassportType"));

            var ids = await connection.QueryAsync<int>(updateQuery, parameters, transaction: transaction, commandTimeout: _options.Timeout);

            return ids.ToArray();
        }

        public async Task<int> GetPassportId(int batchSize, IDbConnection connection, IDbTransaction transaction)
        {
            var id = await connection.QuerySingleAsync<int>(@"SELECT NEXT VALUE FOR PassportIdSeq", transaction: transaction);

            if (id == 1)
            {
                return id;
            }

            int startId = (id - 1) * batchSize + 1;

            return startId;
        }
    }
}