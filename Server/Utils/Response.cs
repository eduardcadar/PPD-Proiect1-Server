using Domain.Domain;

namespace Server.Utils
{
    public class Response
    {
        public ResponseType Type { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
        public int? PlanningId { get; set; }
        public string Name { get; set; }
        public string Cnp { get; set; }
        public DateTime Date { get; set; }
        public int TreatmentLocation { get; set; }
        public Treatment Treatment { get; set; }
        public DateTime TreatmentDate { get; set; }
        public int Sum { get; set; }
    }
}
