namespace InactivePassports.Data.Entities.WebAPI
{
    public record PassportRequest
    {
        public int Series { get; init; }
        public int Number { get; init; }
    }
}