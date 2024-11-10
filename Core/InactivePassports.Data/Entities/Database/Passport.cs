namespace InactivePassports.Data.Entities.Database
{
    public record Passport
    {
        public int Id { get; init; }
        public int Series { get; init; }
        public int Number { get; init; }
        public bool Status { get; init; }
    }
}