using System.Data.SqlClient;
using InactivePassports.Data.Entities.File;

namespace InactivePassports.Contracts.Repositories
{
    public interface IOperation
    {
        Task InsertOperations(int id, int batchSize, ActionType actionType, SqlConnection connection, SqlTransaction transaction);
        Task InsertOperations(int[] ids, ActionType actionType, SqlConnection connection, SqlTransaction transaction);
    }
}