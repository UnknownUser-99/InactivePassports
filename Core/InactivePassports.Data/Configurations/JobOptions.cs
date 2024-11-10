namespace InactivePassports.Data.Configurations
{
    public record JobOptions
    {
        public string InactivePassportsPath { get; init; }
        public string ActivePassportsPath { get; init; }
    }
}