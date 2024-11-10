namespace InactivePassports.Data.Entities.WebAPI
{
    public record DateAction
    {
        public DateOnly Date { get; init; }
        public string Action { get; init; }
    }
}