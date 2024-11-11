using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Configurations;
using InactivePassports.Data.Entities.File;
using InactivePassports.Services.Repositories;
using Dapper;
using Moq;
using Moq.Dapper;

namespace InactivePassports.Services.Tests.Unit.Repositories
{
    [TestClass]
    public class DatabaseUpdateRepositoryTests
    {
        private string _connectionString;
        private IDatabaseUpdate _repository;

        private Mock<IPassport> _passportRepository;
        private Mock<IOperation> _operationRepository;

        [TestInitialize]
        public void Setup()
        {
            _passportRepository = new Mock<IPassport>();
            _operationRepository = new Mock<IOperation>();
            _connectionString = "Server=localhost,1433;Database=master;User=SA;Password=G!17dJp!x9LrT2Kq;";
            _repository = new DatabaseUpdateRepository(_passportRepository.Object, _operationRepository.Object, _connectionString);
        }

        [TestMethod]
        public async Task Insert_Valid_Completed()
        {
            var passports = new[]
            {
                new Passport { Series = 1234, Number = 123456 },
                new Passport { Series = 4321, Number = 654321 }
            };

            int batchSize = 50000;
            int id = 1;

            _passportRepository
                .Setup(m => m.GetPassportId(batchSize, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .ReturnsAsync(id);

            _passportRepository
                .Setup(m => m.InsertPassports(passports, id, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(Task.CompletedTask);

            _operationRepository
                .Setup(m => m.InsertOperations(id, passports.Length, ActionType.Deactivated, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(Task.CompletedTask);

            await _repository.Insert(passports, batchSize);

            _passportRepository.Verify(m => m.GetPassportId(batchSize, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()), Times.Once);
            _passportRepository.Verify(m => m.InsertPassports(passports, id, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()), Times.Once);
            _operationRepository.Verify(m => m.InsertOperations(id, passports.Length, ActionType.Deactivated, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()), Times.Once);
        }

        [TestMethod]
        public async Task Update_Valid_Completed()
        {
            var passports = new[]
            {
                new Passport { Series = 1234, Number = 123456 },
                new Passport { Series = 4321, Number = 654321 }
            };

            ActionType actionType = ActionType.Deactivated;

            var ids = new[] { 1, 2 };

            _passportRepository
                .Setup(m => m.UpdatePassports(passports, actionType, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .ReturnsAsync(ids);

            _operationRepository
                .Setup(m => m.InsertOperations(ids, actionType, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()))
                .Returns(Task.CompletedTask);

            await _repository.Update(passports, actionType);

            _passportRepository.Verify(m => m.UpdatePassports(passports, actionType, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()), Times.Once);
            _operationRepository.Verify(m => m.InsertOperations(ids, actionType, It.IsAny<SqlConnection>(), It.IsAny<SqlTransaction>()), Times.Once);
        }
    }
}