using System.Data;
using System.Data.SqlClient;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Configurations;
using InactivePassports.Data.Entities.File;

namespace InactivePassports.Services.Repositories
{
    public class OperationRepository : IOperation
    {
        private readonly RepositoryOptions _options;

        public OperationRepository(RepositoryOptions options)
        {
            _options = options;
        }

        public async Task InsertOperations(int id, int batchSize, ActionType actionType, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.BulkCopyTimeout = _options.Timeout;

                bulkCopy.DestinationTableName = "Operations";

                bulkCopy.ColumnMappings.Add("Passport", "Passport");
                bulkCopy.ColumnMappings.Add("Action", "Action");

                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("Passport", typeof(int));
                dataTable.Columns.Add("Action", typeof(string));

                string action = actionType.ToString();

                for (int i = 0; i < batchSize; i++)
                {
                    dataTable.Rows.Add(id, action);

                    id++;
                }

                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }

        public async Task InsertOperations(int[] ids, ActionType actionType, SqlConnection connection, SqlTransaction transaction)
        {
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.BulkCopyTimeout = _options.Timeout;

                bulkCopy.DestinationTableName = "Operations";

                bulkCopy.ColumnMappings.Add("Passport", "Passport");
                bulkCopy.ColumnMappings.Add("Action", "Action");

                DataTable dataTable = new DataTable();

                dataTable.Columns.Add("Passport", typeof(int));
                dataTable.Columns.Add("Action", typeof(string));

                string action = actionType.ToString();

                for (int i = 0; i < ids.Length; i++)
                {
                    dataTable.Rows.Add(ids[i], action);
                }

                await bulkCopy.WriteToServerAsync(dataTable);
            }
        }
    }
}