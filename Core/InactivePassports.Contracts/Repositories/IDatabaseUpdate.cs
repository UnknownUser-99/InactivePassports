using InactivePassports.Data.Entities.File;

namespace InactivePassports.Contracts.Repositories
{
    public interface IDatabaseUpdate
    {
        Task Insert(Passport[] passports, int batchSize);
        Task Update(Passport[] passports, ActionType actionType);
    }
}