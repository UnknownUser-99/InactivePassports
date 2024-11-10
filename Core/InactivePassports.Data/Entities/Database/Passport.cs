namespace InactivePassports.Data.Entities.Database
{
    public class Passport
    {
        public int Id { get; set; }
        public int Series { get; set; }
        public int Number { get; set; }
        public bool Status { get; set; }
    }
}