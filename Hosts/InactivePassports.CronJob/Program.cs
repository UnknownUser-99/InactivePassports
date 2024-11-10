using InactivePassports.Contracts.DatabaseUpdate;
using InactivePassports.Contracts.Repositories;
using InactivePassports.Data.Configurations;
using InactivePassports.Services.DatabaseUpdate;
using InactivePassports.Services.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;

namespace InactivePassports.CronJob
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = builder.Build();

            var connectionString = configuration.GetValue<string>("ConnectionStrings:DefaultConnection");
            var cron = configuration.GetValue<string>("CronJobSettings:Cron");
            var repositoryOptions = configuration.GetSection("Configurations:Repository").Get<RepositoryOptions>();
            var jobOptions = configuration.GetSection("Configurations:Job").Get<JobOptions>();
            var dataUpdateOptions = configuration.GetSection("Configurations:DataUpdate").Get<DataUpdateOptions>();
            var dataDownloadOptions = configuration.GetSection("Configurations:DataDownload").Get<DataDownloadOptions>();           

            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {      
                    services.AddSingleton<IPassport>(sp =>
                    {
                        return new PassportRepository(repositoryOptions);
                    });
                    services.AddSingleton<IOperation>(sp =>
                    {
                        return new OperationRepository(repositoryOptions);
                    });
                    services.AddSingleton<IDatabaseUpdate>(sp =>
                    {
                        var passportRepository = sp.GetRequiredService<IPassport>();
                        var operationRepository = sp.GetRequiredService<IOperation>();

                        return new DatabaseUpdateRepository(passportRepository, operationRepository, connectionString);
                    });
                    services.AddSingleton<IDataUpdate>(sp =>
                    {
                        var databaseUpdateRepository = sp.GetRequiredService<IDatabaseUpdate>();

                        return new DataUpdateService(databaseUpdateRepository, dataUpdateOptions);
                    });
                    services.AddSingleton<IDataDownload>(sp =>
                    {
                        return new DataDownloadService(dataDownloadOptions);
                    });
                    services.AddSingleton<IJobFactory, MicrosoftDependencyInjectionJobFactory>();
                    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
                    services.AddQuartz(q =>
                    {
                        q.AddJob<DataUpdateJob>(j => j
                            .StoreDurably()
                            .WithDescription("DataUpdateJob")
                        );
                        var jobKey = new JobKey("key");
                        q.AddJob<DataUpdateJob>(jobKey, j => j
                            .WithDescription("DataUpdateKey")
                        );
                        q.AddTrigger(t => t
                            .WithIdentity("trigger")
                            .ForJob(jobKey)
                            .WithCronSchedule(cron)
                        );
                    });
                    services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
                    services.AddSingleton<IJob>(sp =>
                    {
                        var dataDownloadService = sp.GetRequiredService<IDataDownload>();
                        var dataUpdateService = sp.GetRequiredService<IDataUpdate>();

                        return new DataUpdateJob(dataDownloadService, dataUpdateService, jobOptions);
                    });
                })
                .Build();

            await host.RunAsync();
        }
    }
}