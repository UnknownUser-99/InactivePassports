namespace InactivePassports.Data.Entities.WebAPI
{
    public record DateHistoryResult
    {
        public DateOnly Date { get; init; }
        public PassportOperation[] PassportOperation { get; init; }
    }
}