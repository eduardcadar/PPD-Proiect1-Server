namespace Domain.Domain
{
    public class Payment
    {
        public int Id { get; set; }
        public int? PlanningId { get; set; }
        public DateTime Date { get; set; }
        public string Cnp { get; set; }
        public int Sum { get; set; }
    }
}
