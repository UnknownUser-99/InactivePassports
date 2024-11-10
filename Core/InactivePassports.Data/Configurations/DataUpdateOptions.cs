namespace InactivePassports.Data.Configurations
{
    public record DataUpdateOptions
    {
        public int SemaphoreSize { get; init; }
        public int BatchSize { get; init; }
        public string InactivePassportsPath { get; init; }
        public string ActivePassportsPath { get; init; }
    }
}