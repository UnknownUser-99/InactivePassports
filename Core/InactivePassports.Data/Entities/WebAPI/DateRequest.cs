namespace InactivePassports.Data.Entities.WebAPI
{
    public record DateRequest
    {
        public DateTime StartDate { get; init; }
        public DateTime EndDate { get; init; }
    }
}