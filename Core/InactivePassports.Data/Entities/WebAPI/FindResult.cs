namespace InactivePassports.Data.Entities.WebAPI
{
    public record FindResult
    {
        public int Series { get; init; }
        public int Number { get; init; }
        public bool Result { get; init; }
    }
}