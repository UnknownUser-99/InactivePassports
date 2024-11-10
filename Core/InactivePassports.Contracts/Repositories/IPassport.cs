using System.Data;
using System.Data.SqlClient;
using InactivePassports.Data.Entities.File;

namespace InactivePassports.Contracts.Repositories
{
    public interface IPassport
    {
        Task InsertPassports(Passport[] passports, int id, SqlConnection connection, SqlTransaction transaction);
        Task<int[]> UpdatePassports(Passport[] passports, ActionType actionType, IDbConnection connection, IDbTransaction transaction);
        Task<int> GetPassportId(int batchSize, IDbConnection connection, IDbTransaction transaction);
    }
}