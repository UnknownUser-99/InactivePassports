using System.Data;
using System.Data.Common;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Configurations;
using InactivePassports.Services.Repositories;
using Dapper;
using Moq;
using Moq.Dapper;

namespace InactivePassports.Services.Tests.Unit.Repositories
{
    [TestClass]
    public class PassportRepositoryTests
    {
        private RepositoryOptions _options;
        private IPassport _repository;

        private Mock<DbConnection> _connection;
        private Mock<DbTransaction> _transaction;

        [TestInitialize]
        public void Setup()
        {
            _connection = new Mock<DbConnection>();
            _transaction = new Mock<DbTransaction>();
            _options = new RepositoryOptions { Timeout = 600 };
            _repository = new PassportRepository(_options);
        }

        [DataTestMethod]
        [DataRow(1, 1)]
        [DataRow(2, 50001)]
        [DataRow(1000, 49950001)]
        public async Task GetPassportId_Valid_Id(int count, int expectedId)
        {
            int batchSize = 50000;

            _connection.SetupDapperAsync(c => c.QuerySingleAsync<int>(It.IsAny<string>(), null, _transaction.Object, null, null)).ReturnsAsync(count);

            var result = await _repository.GetPassportId(batchSize, _connection.Object, _transaction.Object);

            Assert.AreEqual(expectedId, result);
        }
    }
}