using System.Data;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Entities.Database;
using InactivePassports.Data.Entities.WebAPI;
using Moq;

namespace InactivePassports.Services.Tests.Unit.Repositories
{
    [TestClass]
    public class WebAPIRepositoryTests
    {
        private Mock<IWebAPI> _repository;

        [TestInitialize]
        public void Setup()
        {
            _repository = new Mock<IWebAPI>();
        }

        [DataTestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task FindPassport_Valid_Result(bool expectedResult)
        {
            var seriesNumber = new PassportRequest { Series = 1234, Number = 123456 };

            _repository
                .Setup(m => m.FindPassport(seriesNumber))
                .ReturnsAsync(expectedResult);

            var result = await _repository.Object.FindPassport(seriesNumber);

            Assert.AreEqual(expectedResult, result);

            _repository.Verify(m => m.FindPassport(seriesNumber), Times.Once);
        }

        [TestMethod]
        public async Task GetPassportHistory_Valid_Result()
        {
            var seriesNumber = new PassportRequest { Series = 1234, Number = 123456 };

            var operations = new[]
            {
                new Operation { Passport = 1, Date = DateTime.Now.AddDays(-1), Action = "Deactivated" },
                new Operation { Passport = 1, Date = DateTime.Now, Action = "Activated" }
            };

            _repository
                .Setup(m => m.GetPassportHistory(seriesNumber))
                .ReturnsAsync(operations);

            var result = await _repository.Object.GetPassportHistory(seriesNumber);

            Assert.AreEqual(operations.Length, result.Length);
            Assert.AreEqual(operations[0].Action, result[0].Action);
            Assert.AreEqual(operations[1].Action, result[1].Action);

            _repository.Verify(m => m.GetPassportHistory(seriesNumber), Times.Once);
        }

        [TestMethod]
        public async Task GetPassportHistory_Valid_NotFound()
        {
            var seriesNumber = new PassportRequest { Series = 1234, Number = 987654 };

            var operations = Array.Empty<Operation>();

            _repository
                .Setup(m => m.GetPassportHistory(seriesNumber))
                .ReturnsAsync(operations);

            var result = await _repository.Object.GetPassportHistory(seriesNumber);

            Assert.AreEqual(0, result.Length);

            _repository.Verify(m => m.GetPassportHistory(seriesNumber), Times.Once);
        }

        [TestMethod]
        public async Task GetDateHistory_Valid_Result()
        {
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;

            var dateHistoryResult = new[]
            {
                new DateHistoryResult
                {
                    Date = DateOnly.FromDateTime(DateTime.Now),
                    PassportOperation = new[]
                    {
                        new PassportOperation { Id = 1, Series = 1234, Number = 123456, Action = "Deactivated" }
                    }
                }
            };

            _repository
                .Setup(m => m.GetDateHistory(startDate, endDate))
                .ReturnsAsync(dateHistoryResult);

            var result = await _repository.Object.GetDateHistory(startDate, endDate);

            Assert.AreEqual(dateHistoryResult.Length, result.Length);
            Assert.AreEqual(dateHistoryResult[0].PassportOperation.Length, result[0].PassportOperation.Length);
            Assert.AreEqual(dateHistoryResult[0].PassportOperation[0].Action, result[0].PassportOperation[0].Action);

            _repository.Verify(m => m.GetDateHistory(startDate, endDate), Times.Once);
        }

        [TestMethod]
        public async Task GetDateHistory_Valid_NotFound()
        {
            var startDate = DateTime.Now.AddDays(-30);
            var endDate = DateTime.Now;

            var dateHistoryResult = Array.Empty<DateHistoryResult>();

            _repository
                .Setup(m => m.GetDateHistory(startDate, endDate))
                .ReturnsAsync(dateHistoryResult);

            var result = await _repository.Object.GetDateHistory(startDate, endDate);

            Assert.AreEqual(0, result.Length);

            _repository.Verify(m => m.GetDateHistory(startDate, endDate), Times.Once);
        }
    }
}