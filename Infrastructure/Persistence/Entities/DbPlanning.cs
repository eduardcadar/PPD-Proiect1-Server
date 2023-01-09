using System.ComponentModel.DataAnnotations.Schema;

namespace Infrastructure.Persistence.Entities
{
    public class DbPlanning
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Cnp { get; set; }
        public DateTime Date { get; set; }
        public int TreatmentLocation { get; set; }
        [ForeignKey("Treatment")]
        public int TreatmentType { get; set; }
        public DbTreatment Treatment { get; set; }
        public DateTime TreatmentDate { get; set; }
    }
}
