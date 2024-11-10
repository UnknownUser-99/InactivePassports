using InactivePassports.Data.Entities.File;

namespace InactivePassports.Contracts.DatabaseUpdate
{
    public interface IDataUpdate
    {
        void GetData(HashSet<Passport> passportsInactive, HashSet<Passport> passportsActive);
        void UpdateData(IEnumerable<List<Passport>> passports);
    }
}