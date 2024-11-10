using System.Data.SqlClient;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Entities.File;

namespace InactivePassports.Services.Repositories
{
    public class DatabaseUpdateRepository : IDatabaseUpdate
    {
        private readonly string _connectionString;

        private readonly IPassport _passportRepository;
        private readonly IOperation _operationRepository;

        public DatabaseUpdateRepository(IPassport passportRepository,IOperation operationRepository, string connectionString)
        {
            _passportRepository = passportRepository;
            _operationRepository = operationRepository;

            _connectionString = connectionString;
        }

        public async Task Insert(Passport[] passports, int batchSize)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int id = await _passportRepository.GetPassportId(batchSize, connection, transaction);

                        await _passportRepository.InsertPassports(passports, id, connection, transaction);

                        await _operationRepository.InsertOperations(id, passports.Length, ActionType.Deactivated, connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task Update(Passport[] passports, ActionType actionType)
        {
            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction transaction = connection.BeginTransaction())
                {
                    try
                    {
                        int[] ids = await _passportRepository.UpdatePassports(passports, actionType, connection, transaction);

                        await _operationRepository.InsertOperations(ids, actionType, connection, transaction);

                        transaction.Commit();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();

                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}