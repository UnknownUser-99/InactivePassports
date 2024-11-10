namespace InactivePassports.Data.Entities.File
{
    public record Passport
    {
        public int Series { get; init; }
        public int Number { get; init; }
    }
}