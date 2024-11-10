using InactivePassports.Data.Entities.File;

namespace InactivePassports.Contracts.DatabaseUpdate
{
    public interface IDataDownload
    {
        void DownloadFile();
        HashSet<Passport> ReadPassports(string path);
        IEnumerable<List<Passport>> ReadFile();
    }
}