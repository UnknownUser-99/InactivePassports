namespace InactivePassports.Data.Configurations
{
    public record DataDownloadOptions
    {
        public string ApplicationName { get; init; }
        public string CredentialsPath { get; init; }
        public string TokensPath { get; init; }
        public string FileId { get; init; }
        public string DataPath { get; init; }
        public int SizeCollection { get; init; }
    }
}