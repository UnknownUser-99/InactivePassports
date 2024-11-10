using InactivePassports.Contracts.DatabaseUpdate;
using InactivePassports.Data.Configurations;
using InactivePassports.Data.Entities.File;
using Quartz;

namespace InactivePassports.CronJob
{
    public class DataUpdateJob : IJob
    {
        private readonly JobOptions _options;
        private readonly IDataDownload _downloadService;
        private readonly IDataUpdate _updateService;

        public DataUpdateJob(IDataDownload downloadService, IDataUpdate updateService, JobOptions options)
        {
            _options = options;
            _downloadService = downloadService;
            _updateService = updateService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            _downloadService.DownloadFile();

            HashSet<Passport> inactivePassports = _downloadService.ReadPassports(_options.InactivePassportsPath);
            HashSet<Passport> activePassports = _downloadService.ReadPassports(_options.ActivePassportsPath);

            _updateService.GetData(inactivePassports, activePassports);
            _updateService.UpdateData(_downloadService.ReadFile());

            await Task.CompletedTask;
        }
    }
}