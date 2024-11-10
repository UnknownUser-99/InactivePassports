namespace InactivePassports.Data.Entities.Database
{
    public class Operation
    {
        public int Id { get; set; }
        public int Passport { get; set; }
        public DateTime Date { get; set; }
        public string Action { get; set; }
    }
}