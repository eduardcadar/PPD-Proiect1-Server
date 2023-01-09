namespace Domain.Domain
{
    public class Planning
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cnp { get; set; }
        public DateTime Date { get; set; }
        public int TreatmentLocation { get; set; }
        public Treatment Treatment { get; set; }
        public DateTime TreatmentDate { get; set; }
    }
}
