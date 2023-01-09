namespace Server.Utils
{
    public class Request
    {
        public RequestType Type { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cnp { get; set; }
        public DateTime Date { get; set; }
        public int TreatmentLocation { get; set; }
        public int TreatmentType { get; set; }
        public DateTime TreatmentDate { get; set; }
        public int Sum { get; set; }
    }
}
