namespace InactivePassports.Data.Entities.WebAPI
{
    public record PassportOperation
    {
        public int Id { get; init; }
        public int Series { get; init; }
        public int Number { get; init; }
        public string Action { get; init; }
    }
}