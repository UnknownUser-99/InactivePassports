namespace InactivePassports.Data.Entities.WebAPI
{
    public record PassportHistoryResult
    {
        public int Id { get; init; }
        public int Series { get; init; }
        public int Number { get; init; }
        public DateAction[] Actions { get; init; }
    }
}