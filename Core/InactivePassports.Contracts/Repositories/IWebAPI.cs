using InactivePassports.Data.Entities.Database;
using InactivePassports.Data.Entities.WebAPI;

namespace InactivePassports.Contracts.Repositories
{
    public interface IWebAPI
    {
        Task<bool> FindPassport(PassportRequest seriesNumber);
        Task<Operation[]> GetPassportHistory(PassportRequest seriesNumber);
        Task<DateHistoryResult[]> GetDateHistory(DateTime startDate, DateTime endDate);
    }
}