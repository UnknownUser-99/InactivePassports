namespace InactivePassports.Data.Entities.Database
{
    public record Operation
    {
        public int Id { get; init; }
        public int Passport { get; init; }
        public DateTime Date { get; init; }
        public string Action { get; init; }
    }
}